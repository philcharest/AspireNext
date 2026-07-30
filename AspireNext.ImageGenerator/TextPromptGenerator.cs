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
                Name = "Japandi Ink Wash",
                // FIXES vs before: (1) medium ("sumi-e ink wash painting") now LEADS the aesthetic so the
                // style anchors the image instead of trailing after the subject; (2) NegativeAdds now kills
                // photorealism explicitly (this is why the teapot came out as a photo); (3) dropped the
                // concrete still-life / figurative subjects that pull JuggernautXL back toward photography,
                // keeping nature subjects the ink LoRA handles well; (4) LoRA strength 0.8 -> 0.9.
                Aesthetic = "sumi-e ink wash painting, japandi wabi-sabi minimalism, loose expressive brush "
                          + "strokes, soft bleeding ink gradients, visible rice-paper texture, generous empty "
                          + "negative space, hand-painted, matte",
                ColorPalette = "sumi ink black and soft grey, warm beige, faded terracotta, muted sage green, aged cream paper",
                Subjects = new[] {
                    "a single gnarled plum branch with a few pink cherry blossoms sweeping in from one corner, asymmetric",
                    "one lone windswept pine on a misty ridge with vast open sky",
                    "three overlapping mountain silhouettes dissolving into pale mist",
                    "a solitary crane wading in still shallow water among sparse reeds",
                    "a few koi drifting beneath loosely suggested ripples",
                    "a stand of bamboo bending in the wind, dry-brush strokes on blank paper",
                    "a small wooden boat on a glassy lake under a low pale moon",
                    "layered fog over distant hills with a thin crescent moon, quiet and sparse",
                    "tall wild grasses arcing from the lower corner into empty space"
                },
                Upscaler = "remacri_original.safetensors",
                RefineDenoise = 0.30,
                Cfg = 5.5,
                NegativeAdds = "photograph, photorealistic, realistic, photo, dslr, 3d render, cgi, octane, "
                             + "depth of field, bokeh, sharp focus, glossy, plastic, hyperrealistic, high "
                             + "contrast, neon, cluttered, busy, oversaturated, kanji, hanzi, hanko, red seal "
                             + "stamp, chop mark, chinese characters, artist signature",
                LoraName = "ink-style_A3.1_XL.safetensors",
                LoraStrength = 0.9,
                LoraTrigger = "ink-style, ink_wash_painting",
                TiledDenoise = 0.10,
            },
            new ArtTrend {
                Name = "Warm Earth-Tone Abstract",
                // RESTORED and LoRA-free on JuggernautXL. Your strongest result so far (the boho botanical-
                // abstract image) came from exactly this look, so we lean into layered organic shapes + fine
                // line-art botanicals in full earthy colour, NOT empty texture fields. No LoRA = no attribution
                // or licensing questions, and the base model does flat illustration well once photo is negated.
                Aesthetic = "flat matte boho illustration, hand-painted minimalist abstract, layered translucent "
                          + "organic shapes, fine single-line botanical drawings, gouache and watercolour "
                          + "washes, subtle grain and paper texture, elegant negative space, mid-century modern",
                ColorPalette = "terracotta, burnt sienna, clay, warm ochre, sage green, olive, sand beige, cream",
                Subjects = new[] {
                    "overlapping translucent circles behind a few tall single-line botanical stems and leaves",
                    "large sage and terracotta arch shapes with delicate line-art grasses in the foreground",
                    "simplified abstract leaves and seed pods on thin arcing stems with scattered dots",
                    "three or four floating pebble shapes linked by fine hand-drawn lines",
                    "an abstract sun disc above soft layered hills with a sprig of minimal foliage",
                    "abstract flowers reduced to flat circles and single-line stems, airy composition",
                    "a bold half-circle balanced against a thin leaf branch and wide open space",
                    "stacked abstract landscape bands topped with one delicate botanical line drawing"
                },
                Upscaler = "remacri_original.safetensors",
                RefineDenoise = 0.32,
                Cfg = 6.0,
                NegativeAdds = "photograph, photorealistic, realistic, photo, dslr, 3d render, cgi, depth of "
                             + "field, bokeh, glossy, plastic, harsh shadows, hard black outlines, ultra sharp, "
                             + "high contrast, neon, cluttered, busy, messy, text, letters, watermark, "
                             + "human face, people",
                LoraName = "",          // no LoRA — runs on JuggernautXL directly
                LoraStrength = 0.8,     // (only used if you later set a LoraName)
                LoraTrigger = "",
                TiledDenoise = 0.12,
            },
            new ArtTrend {
                Name = "Abstract Pattern (Lines & Texture)",
                // Keeps the AbstractPatternStyle SDXL LoRA (Civitai models/346675). The OLD version literally
                // put "no subject" in the prompt, which is why it rendered as flat, boring texture swatches.
                // Now we ask for a real abstract COMPOSITION with a focal point (bold gestural lines + shapes),
                // so there is something to look at while staying non-representational. Photo negatives added.
                // TODO: LoraName must match your downloaded file in models/loras, else this trend errors.
                Aesthetic = "abstract expressionist composition, bold gestural brush lines beside fine delicate "
                          + "linework, layered flat organic shapes, torn-paper collage edges, rich hand-painted "
                          + "texture, matte, mid-century modern abstraction, strong asymmetric focal point",
                ColorPalette = "warm muted earth tones, terracotta, ochre, sand, clay, sage and cream",
                Subjects = new[] {
                    "a bold sweeping calligraphic gesture crossed by clusters of fine parallel lines and a few flat circles",
                    "layered torn-paper shapes in warm earth tones with dark linework threading between them",
                    "thick and thin arcs, dots and hand-drawn grids arranged in off-center balance",
                    "intersecting fine line networks over soft blocks of terracotta and sage with open negative space"
                },
                Upscaler = "remacri_original.safetensors",
                RefineDenoise = 0.32,
                Cfg = 6.0,
                NegativeAdds = "photograph, photorealistic, realistic, 3d render, dslr, glossy, person, people, "
                             + "face, portrait, figure, eyes, hands, animal, recognizable object, still life, "
                             + "vase, bottle, landscape, building, text, letters, logo, watermark",
                LoraName = "AbstractPatternStyleXL.safetensors",  // <-- match your downloaded filename
                LoraStrength = 0.85,                              // test 0.85 vs 1.0 (creator recommends 1)
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

            // Front-load the aesthetic/MEDIUM so it anchors the image (the old template trailed it after
            // the subject, letting JuggernautXL's photo prior win). This template is what actually runs
            // whenever HF_TOKEN is unset, so it has to be strong on its own.
            string fallback =
                $"{selectedTrend.Aesthetic}, {selectedSubject}, "
                + $"color palette of {selectedTrend.ColorPalette}, "
                + "fine art wall print, rich texture, intricate detail, masterpiece";

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
                    // Old system prompt told the model to write one "interior design" sentence, which produced
                    // flat, photographic descriptions that dropped the art medium. This one keeps the medium
                    // front and centre and asks for keyword-style prompts SDXL responds to.
                    new { role = "system", content =
                        "You write prompts for a Stable Diffusion XL model that makes gallery-quality WALL ART, "
                        + "never photographs. Return ONE prompt as a comma-separated list of visual tags. Front-load "
                        + "the artistic MEDIUM and technique from the aesthetic (e.g. 'sumi-e ink wash painting', "
                        + "'flat matte gouache illustration') and keep every style, texture and medium word you are "
                        + "given — never swap them for photographic terms. Then add 3-5 concrete tags for composition "
                        + "(negative space, asymmetry, off-center focal point), brushwork or texture, and lighting "
                        + "mood that fit the medium. Keep the given colour palette. Output only the tags — no "
                        + "sentences, no quotes, no preamble." },
                    new { role = "user", content =
                        $"Aesthetic/medium: {selectedTrend.Aesthetic}. Subject: {selectedSubject}. Colour palette: {selectedTrend.ColorPalette}." }
                },
                max_tokens = 160,
                temperature = 0.7
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
