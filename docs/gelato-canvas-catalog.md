# Gelato canvas catalog

Pulled from the live Product API (`https://product.gelatoapis.com/v3`) on 2026-10-18. It's read-only reference data and can be re-pulled at any time:

- `GET /catalogs/canvas`
- `POST /catalogs/canvas/products:search`

## Catalog

- **Catalog UID:** `canvas`. It's set in `GelatoOptions.CanvasCatalogUid`.
- **Size attribute:** `CanvasFormat`, with 55 values, e.g. `500x700-mm` titled "500x700 mm" or `24x30-inch` titled "24x30 inches". It's set in `GelatoOptions.CanvasSizeAttributeUid`.
- **Orientation:** `ver` (portrait) or `hor` (landscape). Each size exists in both.
- **Material / colour:** `canvas` / `4-0`. These are the only options.

## Two product families (704 products in total)

Both families come in the same metric and imperial sizes.

| Family | Frame options | `productUid` pattern |
|---|---|---|
| Slim / thick | `wood-fsc-slim` (32 mm), `wood-fsc-thick` (38 mm) | `canvas_500x700-mm-20x28-inch_canvas_wood-fsc-slim_4-0_ver` |
| Frame depth | `wood-fsc-2-cm`, `wood-fsc-3-cm`, `wood-fsc-4-cm` | `canvas_product_cf_500x700-mm_cm_canvas_cfrm_wood-fsc-2-cm_cl_4-0_ver` |

**Open decision:** which family and frame depth to sell. Both are activated and published, and both ship to CA and US.

## Metric sizes, grouped by shape

The masters are about 4992×7296 px, with a width-to-height ratio of 0.684, which is SDXL's 832×1216 shape. "Crop" below is how much of the master is cut away to reach each shape. Wrap is ignored here.

| Shape (w/h) | Sizes (cm, portrait) | Crop from master | Lowest PPI on the face |
|---|---|---|---|
| 2:3 (0.667) | 20×30, 30×45, 40×60, 50×75, 60×90 | 2.5% of width | ~206 (60×90) |
| 0.700 | 70×100 | 2.3% of height | ~181 |
| 5:7 (0.714) | 50×70 | 4.2% of height | ~254 |
| 3:4 (0.750) | 30×40, 45×60, 60×80 | 8.8% of height | ~211 (60×80) |
| 0.771 | 27×35 | 11% of height | — |
| 4:5 (0.800) | 20×25, 40×50, 60×75 | 14.5% of height | ~211 (60×75) |
| 5:6 (0.833) | 50×60 | 18% of height | — |
| 1:1 | 20, 30, 40, 50, 60 square | 32% of height | — |
| Panoramic | 20×60, 30×60, 30×90, 30×100, 40×80, 50×100 | not suitable | — |

All 16 sizes in `AspireNext.ImageGenerator/CanvasFormats.cs` exist in this catalog. Landscape sizes use the same products with `hor` orientation.

## Sizes sold and pricing

Every product is sold in the 7 sizes that need at most 5% cropped from the master. All use the **2 cm frame** (`GelatoOptions.CanvasFrameUid`). The size and price list lives in the `CanvasSizes` table, seeded by `CatalogSeeder`.

Delivered cost is product plus the cheapest shipping, from order quotes run on 2026-10-19: Montréal for CAD and New York for USD. US orders are printed in the US. Retail prices are cost ÷ 0.67, which leaves about 30% margin after Stripe's ~3% fee, rounded up to the next x4.99 or x9.99.

| Size (cm) | CAD cost | CAD price | USD cost | USD price |
|---|---|---|---|---|
| 20×30 | 41.38 | 64.99 | 27.28 | 44.99 |
| 30×45 | 63.18 | 94.99 | 38.88 | 59.99 |
| 40×60 | 72.08 | 109.99 | 43.88 | 69.99 |
| 50×70 | 102.78 | 154.99 | 69.08 | 104.99 |
| 50×75 | 104.78 | 159.99 | 70.08 | 109.99 |
| 60×90 | 141.78 | 214.99 | 83.78 | 129.99 |
| 70×100 | 151.58 | 229.99 | 85.58 | 134.99 |

Checkout doesn't charge shipping separately, so these prices include it. Gelato+ (up to 25–33% off products) would add to the margin without any price change.

## Print file spec (wrap and bleed)

From [How do I upload my design for a canvas](https://support.gelato.com/en/articles/8996347-how-do-i-upload-my-design-for-a-canvas). The API doesn't return this.

- **2 cm frame:** each side needs 34 mm of wrap plus 15 mm of bleed. That's 49 mm per side, so 98 mm is added to each dimension.
- **4 cm frame:** each side needs 40 mm of wrap plus 15 mm of bleed. That's 55 mm per side, so 110 mm is added to each dimension.

**Wrap style chosen: image wrap.** The artwork continues around the sides, which suits the nature and pattern art; mirrored edges looked wrong on some pieces. Each print file is made like this:

1. Scale the master so its width equals the full file width.
2. Centre-crop the height to the file height. The trimmed strips are on the back fold, not the front.

Pixel figures below are for the 4992×7296 masters.

| Front (cm) | File (mm) | Master height trimmed | Print PPI | Share of artwork on the front (width) |
|---|---|---|---|---|
| 20×30 | 298×398 | 8.6% | ~425 | 67% |
| 30×45 | 398×548 | 5.8% | ~319 | 75% |
| 40×60 | 498×698 | 4.1% | ~255 | 80% |
| 50×70 | 598×798 | 8.7% | ~212 | 84% |
| 50×75 | 598×848 | 3.0% | ~212 | 84% |
| 60×90 | 698×998 | 2.2% | ~182 | 86% |
| 70×100 | 798×1098 | 5.9% | ~159 | 88% |

All sizes stay above about 150 PPI. The 4848 px-wide masters come out about 3% lower, which is about 154 PPI at 70×100. Product images on the site should show the front-face crop, not the whole master.
