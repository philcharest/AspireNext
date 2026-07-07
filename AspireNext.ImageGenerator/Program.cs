using AspireNext.ImageGenerator;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _httpClient = new();
    private const string ComfyUIUrl = "http://127.0.0.1:8188";
    private const int StartupTimeoutSeconds = 120;
    private const string WorkflowPath = "commercial_print_workflow_sdxl.json";
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    static async Task Main(string[] args)
    {
        Console.WriteLine("🎨 ComfyUI auto-generator — per-trend recipes");
        Console.WriteLine($"Running every {Interval.TotalMinutes} minutes. Leave this window open.\n");

        // Start ComfyUI ONCE (the port check skips it if already running).
        StartComfyUI();
        await WaitForComfyUIReady();
        Console.WriteLine("✓ ComfyUI ready. Starting timer loop...\n");

        using var timer = new PeriodicTimer(Interval);

        // do/while: run immediately, then every interval. One failure won't kill the loop.
        do
        {
            try
            {
                GenerationRequest request = await GenerateTextPrompt();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Trend : {request.Trend.Name}");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Prompt: {request.Prompt}");

                await Call_ComfyUI_Api(request);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Queued. Next run in {Interval.TotalMinutes} min.\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ {ex.Message}");
                Console.ResetColor();
            }
        }
        while (await timer.WaitForNextTickAsync());
    }

    public static async Task<GenerationRequest> GenerateTextPrompt()
    {
        return await TextGenerator.GetPerfectlyMatchedPromptAsync();
    }

    public static async Task<string> Call_ComfyUI_Api(GenerationRequest request)
    {
        string workflowJson = File.ReadAllText(WorkflowPath);
        if (string.IsNullOrEmpty(workflowJson))
            throw new Exception($"Workflow file is empty or not found: {WorkflowPath}");

        var workflow = JsonNode.Parse(workflowJson);
        ArtTrend trend = request.Trend;

        // 1. Positive prompt -> node 6. Prepend the LoRA trigger word (if any), then add a
        //    framing directive so the output IS the artwork (full-frame), not a photo of a canvas.
        const string framing = "full frame flat artwork, fills the entire frame edge to edge, no border, ";
        string positive = framing + request.Prompt;
        if (!string.IsNullOrEmpty(trend.LoraTrigger))
            positive = trend.LoraTrigger + ", " + positive;
        workflow!["6"]!["inputs"]!["text"] = positive;

        // 2. Negative: the base lives in the JSON (node 71) so it's easy to manage there.
        //    We only append this trend's specific additions on top of it.
        string baseNegative = workflow!["71"]!["inputs"]!["text"]!.GetValue<string>();
        if (!string.IsNullOrEmpty(trend.NegativeAdds))
            workflow!["71"]!["inputs"]!["text"] = baseNegative + ", " + trend.NegativeAdds;

        // 3. Per-trend render recipe
        workflow!["402"]!["inputs"]!["model_name"] = trend.Upscaler;    // upscaler choice
        workflow!["401"]!["inputs"]!["denoise"] = trend.RefineDenoise;  // hires refine strength
        workflow!["294"]!["inputs"]!["cfg"] = trend.Cfg;
        workflow!["401"]!["inputs"]!["cfg"] = Math.Max(3.5, trend.Cfg - 1.0);              // pass 2 CFG

        // 3b. LoRA: configure node 500 for trends that have one, or bypass it for trends that don't.
        if (string.IsNullOrEmpty(trend.LoraName))
        {
            // No LoRA: wire model + clip straight from the checkpoint and drop the loader node.
            workflow!["294"]!["inputs"]!["model"] = MakeLink("4", 0);
            workflow!["401"]!["inputs"]!["model"] = MakeLink("4", 0);
            workflow!["6"]!["inputs"]!["clip"] = MakeLink("4", 1);
            workflow!["71"]!["inputs"]!["clip"] = MakeLink("4", 1);
            workflow!["450"]!["inputs"]!["model"] = MakeLink("4", 0);
            workflow!.AsObject().Remove("500");
        }
        else
        {
            workflow!["500"]!["inputs"]!["lora_name"] = trend.LoraName;
            workflow!["500"]!["inputs"]!["strength_model"] = trend.LoraStrength;
            workflow!["500"]!["inputs"]!["strength_clip"] = trend.LoraStrength;
        }

        // 4. Fresh random seeds every run. Random.Shared avoids same-millisecond collisions
        //    that a `new Random()` would cause when the timer fires runs close together.
        long mainSeed = Random.Shared.NextInt64(0, 999_999_999_999_999);
        workflow!["294"]!["inputs"]!["seed"] = mainSeed;
        workflow!["401"]!["inputs"]!["seed"] = Random.Shared.NextInt64(0, 999_999_999_999_999);
        workflow!["450"]!["inputs"]!["seed"] = Random.Shared.NextInt64(0, 999_999_999_999_999);
        workflow!["450"]!["inputs"]!["cfg"] = Math.Max(3.5, trend.Cfg - 1.0);
        workflow!["450"]!["inputs"]!["batch_size"] = 1;


        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seed {mainSeed} | Upscaler {trend.Upscaler} | Denoise {trend.RefineDenoise} | CFG {trend.Cfg}");

        return await QueuePrompt(workflow!.ToJsonString());
    }

    // Builds a ComfyUI node connection, e.g. ["4", 0], for rewiring inputs at runtime.
    private static JsonArray MakeLink(string nodeId, int outputIndex) =>
        new JsonArray(JsonValue.Create(nodeId), JsonValue.Create(outputIndex));

    private static async Task<string> QueuePrompt(string json)
    {
        var payload = new { prompt = JsonSerializer.Deserialize<object>(json) };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ComfyUIUrl}/prompt", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"ComfyUI rejected the prompt ({(int)response.StatusCode}): {responseBody}");

        return responseBody; // contains prompt_id for progress tracking
    }

    public static bool IsComfyUiPortActive(int port = 8188)
    {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        System.Net.IPEndPoint[] listeners = properties.GetActiveTcpListeners();
        return listeners.Any(l => l.Port == port);
    }

    static Process? StartComfyUI()
    {
        var baseDir = @"C:\ComfyUI_windows_portable";
        var scriptName = "run_nvidia_gpu_fast_fp16_accumulation.bat";

        if (IsComfyUiPortActive())
        {
            Console.WriteLine("✓ ComfyUI is already up.");
            return null;
        }

        var psi = new ProcessStartInfo
        {
            FileName = Path.Combine(baseDir, scriptName),
            WorkingDirectory = baseDir,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        var process = Process.Start(psi);
        if (process == null)
            throw new Exception("Failed to start ComfyUI process");

        Console.WriteLine($"✓ ComfyUI process started (PID: {process.Id})");
        return process;
    }

    static async Task WaitForComfyUIReady()
    {
        var sw = Stopwatch.StartNew();
        var timeout = TimeSpan.FromSeconds(StartupTimeoutSeconds);

        while (sw.Elapsed < timeout)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _httpClient.GetAsync($"{ComfyUIUrl}/", cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✓ ComfyUI responsive after {sw.Elapsed.TotalSeconds:F1} seconds");
                    return;
                }
            }
            catch { }

            await Task.Delay(2000);
        }

        throw new Exception($"ComfyUI did not start within {StartupTimeoutSeconds} seconds");
    }
}
