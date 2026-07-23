using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspireNext.ImageGenerator
{
    // A trend package now locks the aesthetic, subject, palette AND the render recipe together,
    // so each art trend renders with settings suited to its texture goals.
    public class ArtTrend
    {
        public string Name { get; set; } = string.Empty;
        public string Aesthetic { get; set; } = string.Empty;
        public string[] Subjects { get; set; } = Array.Empty<string>();
        public string ColorPalette { get; set; } = string.Empty;

        // ---- Per-trend render recipe (injected into the ComfyUI workflow) ----
        public string Upscaler { get; set; } = "4x-UltraSharp.pth"; // -> node 402 model_name
        public double RefineDenoise { get; set; } = 0.45;           // -> node 401 denoise
        public double Cfg { get; set; } = 5.5;                      // -> nodes 294 + 401 cfg
        public string NegativeAdds { get; set; } = string.Empty;    // appended to node 71's base negative
        public string LoraName { get; set; } = string.Empty;        // file in models/loras (empty = no LoRA)
        public double LoraStrength { get; set; } = 0.8;             // LoRA weight when one is set
        public string LoraTrigger { get; set; } = string.Empty;     // trigger word prepended to the prompt
        public double TiledDenoise { get; set; } = 0.20;

        // Japandi:   TiledDenoise = 0.10,
        // Cyberpunk: TiledDenoise = 0.20,
    }

    // Carries BOTH the final prompt and the trend recipe back to the caller.
    public record GenerationRequest(string Prompt, ArtTrend Trend);

    public class TextGenerator
    {
        private static readonly HttpClient client = new HttpClient();

        // Read the token from the environment. Rotate the old hardcoded one — it is compromised.
        private static readonly string HfToken =
            Environment.GetEnvironmentVariable("HF_TOKEN") ?? string.Empty;

        private static readonly List<ArtTrend> TrendLibrary = new()
        {
            new ArtTrend {
                Name = "Japandi Minimalist",
                Aesthetic = "Japandi, Wabi-Sabi minimalist, organic plaster texture, soft matte finish",
                ColorPalette = "warm beige, soft terracotta, muted sage green, and cream",
                Subjects = new[] {
                    "abstract interlocking geometric shapes and flowing botanical lines",
                    "a serene foggy mountain peak with a single ink-washed pine tree",
                    "a steaming bowl of japanese ramen with chopsticks resting on the rim",
                    "a single plum blossom branch in soft ink wash against empty negative space",
                    "three smooth stacked zen stones beside gently raked sand garden lines",
                    "a lone crane standing in still shallow water, sparse ink wash",
                    "an empty ceramic teapot and cup on a low wooden table, soft ink wash",
                    "a quiet bamboo grove dissolving into morning mist",
                    "a thin crescent moon above low rolling hills, minimal ink wash"
                },
                // Soft style comes from the prompt + gentle upscaler + ink LoRA.
                // The refine pass still needs enough strength to clean up artifacts.
                Upscaler = "remacri_original.safetensors",
                RefineDenoise = 0.30,
                Cfg = 5.0,
                NegativeAdds = "sharp harsh edges, high contrast, glossy, neon, cluttered, busy, oversaturated, calligraphy, kanji, hanko, red seal stamp, chop mark, chinese characters, artist signature",
                LoraName = "ink-style_A3.1_XL.safetensors",
                LoraStrength = 0.8,
                LoraTrigger = "ink-style, ink_wash_painting" ,  // triggers for ink-style_A3.1_XL
                TiledDenoise = 0.10,
            }
        };

        public static async Task<GenerationRequest> GetPerfectlyMatchedPromptAsync()
        {
            var rand = Random.Shared;

            // Pick one locked trend package, then a subject that belongs to it.
            ArtTrend selectedTrend = TrendLibrary[rand.Next(TrendLibrary.Count)];
            string selectedSubject = selectedTrend.Subjects[rand.Next(selectedTrend.Subjects.Length)];

            string fallback =
                $"{selectedSubject}, in {selectedTrend.Aesthetic} style, color palette of {selectedTrend.ColorPalette}.";

            // No token configured -> skip the LLM and use the curated template (reliable + free).
            if (string.IsNullOrEmpty(HfToken))
                return new GenerationRequest(fallback, selectedTrend);

            // OpenAI-compatible router endpoint. Verify the current path in Hugging Face's docs.
            string apiUrl = "https://router.huggingface.co/v1/chat/completions";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HfToken);

            var payload = new
            {
                model = "meta-llama/Llama-3.1-8B-Instruct", // 8B writes far better prompts than 1B
                messages = new[]
                {
                    new { role = "system", content = "You are a professional interior design art director. Combine the provided aesthetic, subject, and color palette into one single, cohesive, premium descriptive sentence for an image AI. Do not use conversational intro/outro text." },
                    new { role = "user", content = $"Aesthetic: {selectedTrend.Aesthetic}. Subject: {selectedSubject}. Color Scheme: {selectedTrend.ColorPalette}." }
                },
                max_tokens = 100,
                temperature = 0.5
            };

            try
            {
                var resp = await client.PostAsync(
                    apiUrl,
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (!resp.IsSuccessStatusCode)
                    return new GenerationRequest(fallback, selectedTrend);

                string json = await resp.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);

                // choices is an ARRAY in OpenAI-compatible responses — index [0] first.
                string text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?.Trim() ?? "";

                return new GenerationRequest(string.IsNullOrEmpty(text) ? fallback : text, selectedTrend);
            }
            catch
            {
                return new GenerationRequest(fallback, selectedTrend);
            }
        }
    }
}
