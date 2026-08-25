using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public static class CatalogSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        var japandi_ink_wash = new Category { Name = "Japandi Ink Wash" };
        var warm_earth_tone_abstract = new Category { Name = "Warm Earth-Tone Abstract" };
        var abstract_pattern = new Category { Name = "Abstract Pattern" };
        var vibrant_palette_knife = new Category { Name = "Vibrant Palette Knife" };

        db.Categories.AddRange(japandi_ink_wash, warm_earth_tone_abstract, abstract_pattern, vibrant_palette_knife);

        // Real AI-generated canvas prints (ComfyUI/SDXL), one product per rendered image,
        // organized by the same 4 trend collections the generator produces.
        db.Products.AddRange(
            new Product
            {
                Name = "Gestural Crossing",
                Description = "A bold sweeping calligraphic gesture crossed by clusters of fine parallel lines, mid-century abstraction.",
                ImageUrl = "/images/products/abstract-pattern/gestural-crossing.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields I",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-i.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields II",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-ii.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields III",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-iii.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields IV",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-iv.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields V",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-v.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields VI",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-vi.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields VII",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-vii.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Torn Paper Fields VIII",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "/images/products/abstract-pattern/torn-paper-fields-viii.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Off-Center Grid I",
                Description = "Thick and thin arcs, dots and hand-drawn grids arranged in off-center balance over soft terracotta blocks.",
                ImageUrl = "/images/products/abstract-pattern/off-center-grid-i.jpg",
                Price = 89.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Off-Center Grid II",
                Description = "Thick and thin arcs, dots and hand-drawn grids arranged in off-center balance over soft terracotta blocks.",
                ImageUrl = "/images/products/abstract-pattern/off-center-grid-ii.jpg",
                Price = 89.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Off-Center Grid III",
                Description = "Thick and thin arcs, dots and hand-drawn grids arranged in off-center balance over soft terracotta blocks.",
                ImageUrl = "/images/products/abstract-pattern/off-center-grid-iii.jpg",
                Price = 89.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Intersecting Lines I",
                Description = "Intersecting fine line networks over soft blocks of terracotta and sage, with open negative space.",
                ImageUrl = "/images/products/abstract-pattern/intersecting-lines-i.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Intersecting Lines II",
                Description = "Intersecting fine line networks over soft blocks of terracotta and sage, with open negative space.",
                ImageUrl = "/images/products/abstract-pattern/intersecting-lines-ii.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Intersecting Lines III",
                Description = "Intersecting fine line networks over soft blocks of terracotta and sage, with open negative space.",
                ImageUrl = "/images/products/abstract-pattern/intersecting-lines-iii.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Intersecting Lines IV",
                Description = "Intersecting fine line networks over soft blocks of terracotta and sage, with open negative space.",
                ImageUrl = "/images/products/abstract-pattern/intersecting-lines-iv.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Intersecting Lines V",
                Description = "Intersecting fine line networks over soft blocks of terracotta and sage, with open negative space.",
                ImageUrl = "/images/products/abstract-pattern/intersecting-lines-v.jpg",
                Price = 99.00m,
                Category = abstract_pattern,
            },
            new Product
            {
                Name = "Plum Branch in Mist I",
                Description = "A gnarled plum branch with sweeping cherry blossoms, rendered in loose sumi-e ink wash on rice-paper texture.",
                ImageUrl = "/images/products/japandi-ink-wash/plum-branch-in-mist-i.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Plum Branch in Mist II",
                Description = "A gnarled plum branch with sweeping cherry blossoms, rendered in loose sumi-e ink wash on rice-paper texture.",
                ImageUrl = "/images/products/japandi-ink-wash/plum-branch-in-mist-ii.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Plum Branch in Mist III",
                Description = "A gnarled plum branch with sweeping cherry blossoms, rendered in loose sumi-e ink wash on rice-paper texture.",
                ImageUrl = "/images/products/japandi-ink-wash/plum-branch-in-mist-iii.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Mountains in Pale Mist",
                Description = "Three overlapping mountain silhouettes dissolving into soft mist, rendered in muted sumi ink layers.",
                ImageUrl = "/images/products/japandi-ink-wash/mountains-in-pale-mist.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Crane Among Reeds",
                Description = "A solitary crane wading through still shallow water among sparse reeds, hand-painted with generous negative space.",
                ImageUrl = "/images/products/japandi-ink-wash/crane-among-reeds.jpg",
                Price = 94.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Bamboo in the Wind",
                Description = "A stand of bamboo bending in the wind, captured in loose dry-brush strokes on open paper.",
                ImageUrl = "/images/products/japandi-ink-wash/bamboo-in-the-wind.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Boat Under Pale Moon I",
                Description = "A small wooden boat adrift on a glassy lake beneath a low pale moon, quiet sumi ink tones.",
                ImageUrl = "/images/products/japandi-ink-wash/boat-under-pale-moon-i.jpg",
                Price = 94.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Boat Under Pale Moon II",
                Description = "A small wooden boat adrift on a glassy lake beneath a low pale moon, quiet sumi ink tones.",
                ImageUrl = "/images/products/japandi-ink-wash/boat-under-pale-moon-ii.jpg",
                Price = 94.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Crescent Over Distant Hills I",
                Description = "Layered fog drifting over distant hills beneath a thin crescent moon, sparse and quiet.",
                ImageUrl = "/images/products/japandi-ink-wash/crescent-over-distant-hills-i.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Crescent Over Distant Hills II",
                Description = "Layered fog drifting over distant hills beneath a thin crescent moon, sparse and quiet.",
                ImageUrl = "/images/products/japandi-ink-wash/crescent-over-distant-hills-ii.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Wild Grasses in Empty Space I",
                Description = "Tall wild grasses arcing into generous negative space, loose expressive brushwork on rice paper.",
                ImageUrl = "/images/products/japandi-ink-wash/wild-grasses-in-empty-space-i.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Wild Grasses in Empty Space II",
                Description = "Tall wild grasses arcing into generous negative space, loose expressive brushwork on rice paper.",
                ImageUrl = "/images/products/japandi-ink-wash/wild-grasses-in-empty-space-ii.jpg",
                Price = 89.00m,
                Category = japandi_ink_wash,
            },
            new Product
            {
                Name = "Autumn Path",
                Description = "An autumn park path lined with trees blazing in red and gold, thick impasto knife strokes.",
                ImageUrl = "/images/products/vibrant-palette-knife/autumn-path.jpg",
                Price = 99.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Lone Tree at Sunset",
                Description = "A lone tree on a hill beneath a swirling, richly saturated sunset sky.",
                ImageUrl = "/images/products/vibrant-palette-knife/lone-tree-at-sunset.jpg",
                Price = 99.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Venetian Canal at Dusk",
                Description = "A Venetian canal with gondolas at dusk, shimmering with vivid impressionistic reflections.",
                ImageUrl = "/images/products/vibrant-palette-knife/venetian-canal-at-dusk.jpg",
                Price = 109.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Bull in Motion I",
                Description = "A powerful bull mid-stride, rendered in bold expressive colour and thick palette-knife texture.",
                ImageUrl = "/images/products/vibrant-palette-knife/bull-in-motion-i.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Bull in Motion II",
                Description = "A powerful bull mid-stride, rendered in bold expressive colour and thick palette-knife texture.",
                ImageUrl = "/images/products/vibrant-palette-knife/bull-in-motion-ii.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Bull in Motion III",
                Description = "A powerful bull mid-stride, rendered in bold expressive colour and thick palette-knife texture.",
                ImageUrl = "/images/products/vibrant-palette-knife/bull-in-motion-iii.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Galloping Through Grass I",
                Description = "A horse galloping through tall grass, captured in energetic, textured knife strokes.",
                ImageUrl = "/images/products/vibrant-palette-knife/galloping-through-grass-i.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Galloping Through Grass II",
                Description = "A horse galloping through tall grass, captured in energetic, textured knife strokes.",
                ImageUrl = "/images/products/vibrant-palette-knife/galloping-through-grass-ii.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Galloping Through Grass III",
                Description = "A horse galloping through tall grass, captured in energetic, textured knife strokes.",
                ImageUrl = "/images/products/vibrant-palette-knife/galloping-through-grass-iii.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Parisian Cafe After Rain I",
                Description = "A Parisian cafe street after rain, warm lamplight pooling on the wet pavement.",
                ImageUrl = "/images/products/vibrant-palette-knife/parisian-cafe-after-rain-i.jpg",
                Price = 99.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Parisian Cafe After Rain II",
                Description = "A Parisian cafe street after rain, warm lamplight pooling on the wet pavement.",
                ImageUrl = "/images/products/vibrant-palette-knife/parisian-cafe-after-rain-ii.jpg",
                Price = 99.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Wildflowers Under a Painted Sky",
                Description = "A field of wildflowers beneath a dramatic, multicoloured impressionist sky.",
                ImageUrl = "/images/products/vibrant-palette-knife/wildflowers-under-a-painted-sky.jpg",
                Price = 99.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Sailboat at Sunset I",
                Description = "A small sailboat on a glittering sea at sunset, luminous jewel-toned reflections.",
                ImageUrl = "/images/products/vibrant-palette-knife/sailboat-at-sunset-i.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Sailboat at Sunset II",
                Description = "A small sailboat on a glittering sea at sunset, luminous jewel-toned reflections.",
                ImageUrl = "/images/products/vibrant-palette-knife/sailboat-at-sunset-ii.jpg",
                Price = 104.00m,
                Category = vibrant_palette_knife,
            },
            new Product
            {
                Name = "Botanical Circles",
                Description = "Overlapping translucent circles behind tall single-line botanical stems, in clay and olive tones.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/botanical-circles.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Terracotta Arch",
                Description = "Large sage and terracotta arch shapes with delicate line-art grasses, in warm boho gouache washes.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/terracotta-arch.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Seed Pods and Stems",
                Description = "Simplified leaves and seed pods on thin arcing stems, scattered with soft gouache dots.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/seed-pods-and-stems.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Floating Pebbles I",
                Description = "A cluster of floating pebble shapes linked by fine hand-drawn lines, airy and balanced.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/floating-pebbles-i.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Floating Pebbles II",
                Description = "A cluster of floating pebble shapes linked by fine hand-drawn lines, airy and balanced.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/floating-pebbles-ii.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Floating Pebbles III",
                Description = "A cluster of floating pebble shapes linked by fine hand-drawn lines, airy and balanced.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/floating-pebbles-iii.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Sun Over Soft Hills I",
                Description = "An abstract sun disc above layered hills with a sprig of minimal foliage, warm ochre and sand palette.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/sun-over-soft-hills-i.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Sun Over Soft Hills II",
                Description = "An abstract sun disc above layered hills with a sprig of minimal foliage, warm ochre and sand palette.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/sun-over-soft-hills-ii.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Sun Over Soft Hills III",
                Description = "An abstract sun disc above layered hills with a sprig of minimal foliage, warm ochre and sand palette.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/sun-over-soft-hills-iii.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Flowers in Flat Circles I",
                Description = "Abstract flowers reduced to flat circles and single-line stems, in an airy, open composition.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/flowers-in-flat-circles-i.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Flowers in Flat Circles II",
                Description = "Abstract flowers reduced to flat circles and single-line stems, in an airy, open composition.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/flowers-in-flat-circles-ii.jpg",
                Price = 79.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Half-Circle and Branch I",
                Description = "A bold half-circle balanced against a thin leaf branch, set in wide open negative space.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/half-circle-and-branch-i.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Half-Circle and Branch II",
                Description = "A bold half-circle balanced against a thin leaf branch, set in wide open negative space.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/half-circle-and-branch-ii.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Half-Circle and Branch III",
                Description = "A bold half-circle balanced against a thin leaf branch, set in wide open negative space.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/half-circle-and-branch-iii.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Half-Circle and Branch IV",
                Description = "A bold half-circle balanced against a thin leaf branch, set in wide open negative space.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/half-circle-and-branch-iv.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            },
            new Product
            {
                Name = "Stacked Landscape Bands",
                Description = "Stacked abstract landscape bands topped with a single delicate botanical line drawing.",
                ImageUrl = "/images/products/warm-earth-tone-abstract/stacked-landscape-bands.jpg",
                Price = 84.00m,
                Category = warm_earth_tone_abstract,
            }
        );

        await db.SaveChangesAsync();
    }
}
