using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
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
    private const string PerchanceUrl = "https://perchance.org/image-synthesis-prompt-generator";
    private const string ComfyUIUrl = "http://127.0.0.1:8188";
    private const int StartupTimeoutSeconds = 120;

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🎨 ComfyUI Image Generation Workflow Started");
            Console.WriteLine("===========================================\n");

            // Step 1: Scrape prompt text from Perchance
            Console.WriteLine("Step 1: Scraping prompt from Perchance...");
            string prompt = await ScrapePromptFromPerchance();
            Console.WriteLine($"✓ Generated prompt: {prompt}\n");

            // Step 2: Start ComfyUI
            Console.WriteLine("Step 2: Starting ComfyUI...");
            Process? comfyProcess = StartComfyUI();
            if (comfyProcess != null )
                Console.WriteLine("✓ ComfyUI process started (PID: {0})", comfyProcess?.Id);

            // Step 3: Wait for ComfyUI to be accessible
            Console.WriteLine($"Step 3: Waiting for ComfyUI to be ready at {ComfyUIUrl}...");
            await WaitForComfyUIReady();
            Console.WriteLine("✓ ComfyUI is ready!\n");

            // Call ComfyUI API
            await Call_ComfyUI_Api(prompt);

            // Step 4: Open browser and interact with ComfyUI
            Console.WriteLine("Step 4: Opening ComfyUI in browser and injecting prompt...");
            //await InjectPromptAndGenerate(prompt);
            Console.WriteLine("✓ Prompt injected and generation started!\n");

            Console.WriteLine("===========================================");
            Console.WriteLine("✓ Workflow completed successfully!");
            Console.WriteLine("Check your ComfyUI browser window for the generated image.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }

    static async Task<string> ScrapePromptFromPerchance()
    {
        IWebDriver driver = null;
        try
        {
            Console.WriteLine("Opening Perchance generator in browser...");
            var options = new ChromeOptions();
            // options.AddArgument("--headless");

            driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(PerchanceUrl);
            Console.WriteLine("Waiting for page to load...");
            await Task.Delay(2000);
            driver.Manage().Window.Maximize();
            driver.SwitchTo().Frame(1);
            Console.WriteLine("Locating and clicking 'Randomize' button...");
            driver.FindElement(By.XPath("//button[contains(text(),'randomize')]")).Click();
            var result = driver.FindElement(By.CssSelector("p:nth-child(4)")).Text;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            

            // Find and click the randomize button
            var randomizeButton = wait.Until(d => 
            {
                return driver.FindElement(By.XPath("//button[contains(text(),'randomize')]"));
            });

            if (randomizeButton == null)
            {
                throw new Exception("Could not find 'Randomize' button on Perchance generator");
            }

            randomizeButton.Click();
            Console.WriteLine("✓ Randomize button clicked");

            Console.WriteLine("Waiting for prompt to be generated...");
            await Task.Delay(2000);

            // Wait for the output to be generated
            var outputElement = wait.Until(d => 
            {
                try
                {
                    // Look for output in common element types
                    var element = driver.FindElement(By.CssSelector("p:nth-child(4)"));
                    if (element != null && element.Displayed)
                    {
                        return element;
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            });

            if (outputElement == null)
            {
                throw new Exception("Could not find generated prompt after clicking Randomize");
            }

            var prompt = outputElement.Text.Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                throw new Exception("Generated prompt is empty");
            }

            Console.WriteLine("✓ Prompt successfully generated");
            return prompt;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to scrape Perchance: {ex.Message}", ex);
        }
        finally
        {
            driver?.Quit();
        }
    }

    public static bool IsComfyUiPortActive(int port = 8188)
    {
        // Get all active TCP listeners
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        System.Net.IPEndPoint[] listeners = properties.GetActiveTcpListeners();

        // Check if any listener is on our port
        return listeners.Any(l => l.Port == port);
    }

    public static bool IsComfyProcessRunning(out Process? process)
    {
        process = null;
        // Note: Portable ComfyUI often runs as "python"
        // This looks for any process named python
        Process[] processes = Process.GetProcessesByName("python");

        foreach (var proc in processes)
        {
            try
            {
                // Optional: Check if the process started from your specific C: folder
                if (proc.MainModule!.FileName.Contains("ComfyUI"))
                {
                    process = proc;
                    return true;
                }
            }
            catch { /* Access denied on some system processes */ }
        }

        return false;
    }

    static Process? StartComfyUI()
    {
        var baseDir = @"C:\ComfyUI_windows_portable";
        var scriptName = "run_nvidia_gpu_fast_fp16_accumulation.bat";
        // Use the port check as a fallback
        if (IsComfyUiPortActive())
        {
            Console.WriteLine("✓ ComfyUI is already up.");
            return null;
        }
        var psi = new ProcessStartInfo
        {
            // Combine them so the system knows exactly where the file is
            FileName = Path.Combine(baseDir, scriptName),

            // Keep this so the .bat file can find its internal ComfyUI folders
            WorkingDirectory = baseDir,

            UseShellExecute = false,
            CreateNoWindow = false
        };

        var process = Process.Start(psi);
        
        if (process == null)
        {
            throw new Exception("Failed to start ComfyUI process");
        }

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

    private static async Task<string> QueuePrompt(string json)
    {
        using var client = new HttpClient();

        // Wrap the workflow in a "prompt" object
        var payload = new { prompt = JsonSerializer.Deserialize<object>(json) };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://127.0.0.1:8188/prompt", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // This returns a 'prompt_id' which you use to track the progress
        return responseBody;
    }

    public static async Task<string> Call_ComfyUI_Api(string prompt)
    {
        // Load the JSON you exported from ComfyUI
        string workflowJson = File.ReadAllText("SD3.5M_example_workflow.json");

        if (string.IsNullOrEmpty(workflowJson))
        {
            throw new Exception("No file found or file is empty");
        }

        // --- STEP 1 & 2: Parse and Modify ---
        var workflow = JsonNode.Parse(workflowJson);

        // 1. Inject Style Modifiers into the Prompt
        // This forces the model to adopt Van Gogh's signature impasto brushstrokes and vivid oil paint textures.
        string stylizedPrompt = $"{prompt}, painting. rich vibrant colors, masterpiece, highly detailed, crisp focus";
        workflow!["6"]!["inputs"]!["text"] = stylizedPrompt;

        // 2. Randomize the Seed
        workflow!["294"]!["inputs"]!["seed"] = 376121516;

        // 3. Optimize Sampler Settings for Crisp Textures
        // Note: Verify the node IDs (e.g., "294" or "3") match your specific KSampler node in the JSON.
        if (workflow!["294"]?["inputs"] != null)
        {
            // Increase steps for finer detail resolution (typically 30-40 is great for SD 3.5 details)
            workflow!["294"]!["inputs"]!["steps"] = 30;

            // A slightly higher CFG scale (around 5.5 to 7.0) helps enforce the prompt styling strictly
            workflow!["294"]!["inputs"]!["cfg"] = 3.0;

            // 'dpmpp_2m' or 'euler' paired with 'karras' or 'sgm_uniform' works exceptionally well for painting textures
            workflow!["294"]!["inputs"]!["sampler_name"] = "euler";
            workflow!["294"]!["inputs"]!["scheduler"] = "sgm_uniform";
        }

        return await QueuePrompt(workflow.ToJsonString());
    }
}
