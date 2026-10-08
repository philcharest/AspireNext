using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireNext.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGelatoFulfillmentAndMultiCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CanvasHeightCm",
                table: "Products",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CanvasWidthCm",
                table: "Products",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GelatoProductUid",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceUsd",
                table: "Products",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Orders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentStatus",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GelatoOrderId",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_AddressLine1",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_AddressLine2",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_City",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Country",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Email",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_FirstName",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_LastName",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Phone",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_PostCode",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_State",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GelatoOrderId",
                table: "Orders",
                column: "GelatoOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_GelatoOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CanvasHeightCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CanvasWidthCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GelatoProductUid",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceUsd",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GelatoOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_AddressLine1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_AddressLine2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Country",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Email",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_FirstName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_LastName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Phone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_PostCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_State",
                table: "Orders");
        }
    }
}
