using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireNext.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillOrderCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Orders created before the Currency column existed were backfilled with '' by the
            // migration that added it (AddGelatoFulfillmentAndMultiCurrency) - treat those as CAD,
            // since that's what every price in the catalog was denominated in before this feature.
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"Currency\" = 'CAD' WHERE \"Currency\" = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible - we can't tell which rows were genuinely blank vs. genuinely CAD.
        }
    }
}
