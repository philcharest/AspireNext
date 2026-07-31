using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        var japandi = new Category { Name = "Japandi Ink Wash" };
        var earthTone = new Category { Name = "Warm Earth-Tone Abstract" };
        var abstractPattern = new Category { Name = "Abstract Pattern" };

        db.Categories.AddRange(japandi, earthTone, abstractPattern);

        db.Products.AddRange(
            new Product
            {
                Name = "Plum Branch in Mist",
                Description = "A gnarled plum branch with sweeping cherry blossoms, rendered in loose sumi-e ink wash on rice-paper texture.",
                ImageUrl = "https://picsum.photos/seed/plum-branch-mist/800/1000",
                Price = 89.00m,
                Category = japandi,
            },
            new Product
            {
                Name = "Lone Pine Ridge",
                Description = "One windswept pine on a misty ridge beneath vast open sky, in muted sumi ink and soft grey.",
                ImageUrl = "https://picsum.photos/seed/lone-pine-ridge/800/1000",
                Price = 89.00m,
                Category = japandi,
            },
            new Product
            {
                Name = "Crane Among Reeds",
                Description = "A solitary crane wading through still shallow water among sparse reeds, hand-painted with generous negative space.",
                ImageUrl = "https://picsum.photos/seed/crane-reeds/800/1000",
                Price = 94.00m,
                Category = japandi,
            },
            new Product
            {
                Name = "Terracotta Arch",
                Description = "Large sage and terracotta arch shapes with delicate line-art grasses, in warm boho gouache washes.",
                ImageUrl = "https://picsum.photos/seed/terracotta-arch/800/1000",
                Price = 79.00m,
                Category = earthTone,
            },
            new Product
            {
                Name = "Botanical Circles",
                Description = "Overlapping translucent circles behind tall single-line botanical stems, in clay and olive tones.",
                ImageUrl = "https://picsum.photos/seed/botanical-circles/800/1000",
                Price = 79.00m,
                Category = earthTone,
            },
            new Product
            {
                Name = "Sun Over Soft Hills",
                Description = "An abstract sun disc above layered hills with a sprig of minimal foliage, warm ochre and sand palette.",
                ImageUrl = "https://picsum.photos/seed/sun-soft-hills/800/1000",
                Price = 84.00m,
                Category = earthTone,
            },
            new Product
            {
                Name = "Gestural Crossing",
                Description = "A bold sweeping calligraphic gesture crossed by clusters of fine parallel lines, mid-century abstraction.",
                ImageUrl = "https://picsum.photos/seed/gestural-crossing/800/1000",
                Price = 99.00m,
                Category = abstractPattern,
            },
            new Product
            {
                Name = "Torn Paper Fields",
                Description = "Layered torn-paper shapes in warm earth tones threaded with dark linework, rich hand-painted texture.",
                ImageUrl = "https://picsum.photos/seed/torn-paper-fields/800/1000",
                Price = 99.00m,
                Category = abstractPattern,
            },
            new Product
            {
                Name = "Off-Center Grid",
                Description = "Thick and thin arcs, dots and hand-drawn grids arranged in off-center balance over soft terracotta blocks.",
                ImageUrl = "https://picsum.photos/seed/off-center-grid/800/1000",
                Price = 89.00m,
                Category = abstractPattern,
            }
        );

        await db.SaveChangesAsync();
    }
}
