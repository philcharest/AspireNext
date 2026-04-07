using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _httpClient = new();
    private const string PerchanceUrl = "https://perchance.org/image-synthesis-prompt-generator";
    private const string ComfyUIUrl = "http://127.0.0.1:8188";
    private const int StartupTimeoutSeconds = 60;

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
            Process comfyProcess = StartComfyUI();
            Console.WriteLine("✓ ComfyUI process started (PID: {0})", comfyProcess.Id);

            // Step 3: Wait for ComfyUI to be accessible
            Console.WriteLine($"Step 3: Waiting for ComfyUI to be ready at {ComfyUIUrl}...");
            await WaitForComfyUIReady();
            Console.WriteLine("✓ ComfyUI is ready!\n");

            // Step 4: Open browser and interact with ComfyUI
            Console.WriteLine("Step 4: Opening ComfyUI in browser and injecting prompt...");
            await InjectPromptAndGenerate(prompt);
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

    static Process StartComfyUI()
    {
        var comfyUIDir = GetComfyUIDirectory();
        var pythonPath = Path.Combine(comfyUIDir, "python_embeded", "python.exe");

        if (!File.Exists(pythonPath))
        {
            throw new Exception($"Embedded Python not found at {pythonPath}. Please ensure ComfyUI is properly installed.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = "-s ComfyUI/main.py --windows-standalone-build --fast fp16_accumulation",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            WorkingDirectory = comfyUIDir
        };

        var process = Process.Start(psi);
        if (process == null)
        {
            throw new Exception("Failed to start ComfyUI process");
        }

        return process;
    }


    static string GetComfyUIDirectory()
    {
        // Common ComfyUI installation locations
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ComfyUI"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "ComfyUI"),
            Path.Combine(Directory.GetCurrentDirectory(), "ComfyUI")
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path) && File.Exists(Path.Combine(path, "main.py")))
            {
                return path;
            }
        }

        Console.Write("ComfyUI directory not found automatically. Enter the path to ComfyUI folder: ");
        var customPath = Console.ReadLine();
        
        if (Directory.Exists(customPath) && File.Exists(Path.Combine(customPath, "main.py")))
        {
            return customPath;
        }

        throw new Exception($"ComfyUI not found at {customPath}");
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

    static async Task InjectPromptAndGenerate(string prompt)
    {
        IWebDriver driver = null;
        try
        {
            var options = new ChromeOptions();
            // Uncomment if you want headless mode
            // options.AddArgument("--headless");
            
            driver = new ChromeDriver(options);
            
            Console.WriteLine("Opening browser...");
            driver.Navigate().GoToUrl(ComfyUIUrl);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            Console.WriteLine("Waiting for ComfyUI interface to load...");
            wait.Until(d => d.FindElements(By.CssSelector("canvas, .node")).Count > 0);
            
            await Task.Delay(2000); // Give UI time to fully render

            Console.WriteLine("Locating CLIP Text Encode node...");
            // Wait for and find the CLIP Text Encode node
            var textEncodeNode = wait.Until(d => FindClipTextEncodeNode(d));
            
            if (textEncodeNode == null)
            {
                throw new Exception("Could not find CLIP Text Encode node. Make sure a workflow is loaded in ComfyUI.");
            }

            Console.WriteLine("Injecting prompt...");
            // Find the textarea within the node and input the prompt
            var textArea = textEncodeNode.FindElement(By.CssSelector("textarea"));
            textArea.Clear();
            textArea.SendKeys(prompt);

            await Task.Delay(1000);

            Console.WriteLine("Locating Run button...");
            // Find and click the Run button
            var runButton = wait.Until(d => d.FindElement(By.CssSelector("button[id*='queue']")));
            runButton.Click();

            Console.WriteLine("✓ Generation started!");
            await Task.Delay(2000);
        }
        finally
        {
            driver?.Quit();
        }
    }

    static IWebElement FindClipTextEncodeNode(IWebDriver driver)
    {
        try
        {
            // Look for nodes/elements containing "CLIP Text Encode"
            var nodes = driver.FindElements(By.CssSelector(".node, [class*='node']"));
            
            foreach (var node in nodes)
            {
                try
                {
                    var text = node.Text;
                    if (text.Contains("CLIP Text Encode") || text.Contains("Clip Text") || text.Contains("Text Encode"))
                    {
                        return node;
                    }
                }
                catch { }
            }

            // Alternative: Look for textarea elements in node container
            var textareas = driver.FindElements(By.CssSelector("textarea"));
            if (textareas.Count > 0)
            {
                return textareas[0].FindElement(By.XPath("./ancestor::*[contains(@class, 'node') or contains(@class, 'widget')]"));
            }
        }
        catch { }

        return null;
    }
}
