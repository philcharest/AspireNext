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
        // TiledDenoise is now set per-trend below (e.g. 0.10 Japandi, 0.20 Cyberpunk,
        // 0.12 for the soft earth-tone/abstract trends to protect grain and texture).
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
                    "a single plum pink cherry blossom branch in soft ink wash against empty negative space",
                    "big successive waves crashing on a beach with a full red sun in the sky in background",
                    "a lone crane standing in still shallow water, sparse ink wash",
                    "an empty ceramic teapot and cup on a low wooden table, soft ink wash",
                    "a quiet bamboo grove dissolving into morning mist",
                    "a thin crescent moon above low rolling hills, minimal ink wash",
                    "chinese man with long hair and playing flute on top of castle"
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
            },
            new ArtTrend {
                Name = "Warm Earth-Tone Abstract",
                // NOTE: the Civitai LoRA that inspired this (models/1269071) is a FLUX LoRA and
                // will NOT load on an SDXL checkpoint, so this trend runs LoRA-free on JuggernautXL.
                // Its descriptive trigger words work fine here as plain SDXL style tokens;
                // "illustration001" is dropped because it only means anything with that LoRA loaded.
                Aesthetic = "minimalist illustration, warm muted colors, grainy paper texture, "
                          + "soft diffused lighting, bold geometric shapes, mid-century modern, "
                          + "flat matte finish, layered organic forms, hand-textured abstract",
                ColorPalette = "terracotta, clay, warm ochre, olive green, sand beige, cream and soft taupe",
                Subjects = new[] {
                    "an abstract composition of overlapping arches and soft circles",
                    "a simplified stacked-band mountain range reduced to flat shapes",
                    "a minimalist rising sun over a horizon rendered as concentric arcs",
                    "abstract botanical forms, simplified leaves and stems as flat cut-paper shapes",
                    "an abstract still life of two vases and a bowl as bold silhouettes",
                    "flowing torn-paper collage ribbons and gently curved organic blocks",
                    "a soft abstract landscape of rolling hills and a single round sun"
                },
                Upscaler = "remacri_original.safetensors",   // UltraSharp fights the grain — avoid here
                RefineDenoise = 0.30,
                Cfg = 5.0,
                NegativeAdds = "photorealistic, photograph, 3d render, glossy, plastic, airbrushed, "
                             + "hard black outlines, high contrast, harsh shadows, neon, cluttered, "
                             + "busy, people, human face, letters",
                LoraName = "",          // no LoRA — runs on JuggernautXL directly
                LoraStrength = 0.8,     // (only used if you later set a LoraName)
                LoraTrigger = "",       // empty so no dead token gets prepended
                TiledDenoise = 0.12,    // low, to protect the soft grainy look through the tiled pass
            },
            new ArtTrend {
                Name = "Abstract Pattern (Lines & Texture)",
                // EXPERIMENTAL / PENDING TEST. Uses Abstract Pattern Style SDXL (Civitai models/346675),
                // base model SDXL 1.0, so it loads on JuggernautXL. This LoRA was trained subject-first
                // (its own example prompt is "AbstractPatternStyle football"), so we prompt with NO concrete
                // subject and suppress subjects hard in NegativeAdds to push it toward pure lines/texture.
                // If it keeps injecting faces/objects in testing, drop this trend and use the LoRA-free
                // "Warm Earth-Tone Abstract" route instead.
                // TODO (1): download the LoRA and make LoraName match the actual file in models/loras,
                //           otherwise this trend's runs will fail (the do/while loop survives it, but you'll
                //           see a red error roughly 1 run in N). Comment this trend out until the file exists.
                // TODO (2): creator tested at Clip Skip 2; this workflow runs Clip Skip 1 (no CLIPSetLastLayer
                //           node). If output drifts from the gallery, that mismatch is the likely cause.
                Aesthetic = "abstract, non-representational, no subject, flowing organic lines and "
                          + "layered texture, matte finish, soft grain",
                ColorPalette = "warm muted earth tones, terracotta, ochre, sand, clay and cream",
                Subjects = new[] {
                    "flowing organic curved lines, calligraphic ribbons, intertwining thin and thick strokes, layered linework",
                    "a dense tactile texture field, layered plaster and dry-brush marks, cracked pigment, rough hand-textured surface",
                    "a network of fine intersecting straight and arced lines, mid-century geometric linework, sparse open composition with negative space",
                    "a soft marbled fluid color field, swirling blended gradients, smooth abstract washes, gentle organic transitions"
                },
                Upscaler = "remacri_original.safetensors",
                RefineDenoise = 0.30,
                Cfg = 5.0,
                NegativeAdds = "person, people, human, man, woman, face, portrait, figure, body, eyes, "
                             + "hands, animal, creature, recognizable object, still life, vase, bottle, "
                             + "landscape, horizon, tree, flower, plant, building, text, letters, logo",
                LoraName = "AbstractPatternStyleXL.safetensors",  // <-- match your downloaded filename
                LoraStrength = 0.8,                               // test 0.8 vs 1.0 (creator recommends 1)
                LoraTrigger = "AbstractPatternStyle",
                TiledDenoise = 0.12,
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
