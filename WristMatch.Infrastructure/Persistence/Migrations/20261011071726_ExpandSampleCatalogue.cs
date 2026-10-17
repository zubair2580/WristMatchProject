using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandSampleCatalogue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CaseDiameterMm", "Category", "CategoryId", "Colour", "CostPrice", "CreatedAt", "Currency", "Description", "ImageUrl", "IsActive", "Material", "Name", "Price", "ReorderLevel", "RetailerUrl", "SKU", "Slug", "TaxCategoryId", "TrackInventory", "TryOnReady", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-4000-8000-000000000005"), null, 1, null, "Blue", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A deep blue dial with a polished steel profile for versatile daily wear.", "https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Harbour Blue", 22500m, 0m, "https://example.com/harbour-blue", "HARBOUR-BLUE", "harbour-blue", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000006"), null, 1, null, "Rose Gold", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A warm rose-tone watch with a minimal face and refined proportions.", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Rose Hour", 27800m, 0m, "https://example.com/rose-hour", "ROSE-HOUR", "rose-hour", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000007"), null, 1, null, "Charcoal", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A charcoal dial and clean markers created for understated office dressing.", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Slate Office", 19800m, 0m, "https://example.com/slate-office", "SLATE-OFFICE", "slate-office", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000008"), null, 1, null, "Brown", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A classic brown leather strap paired with a warm, traditional dial.", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85", true, "Leather", "Heritage Brown", 21400m, 0m, "https://example.com/heritage-brown", "HERITAGE-BROWN", "heritage-brown", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000009"), null, 1, null, "Silver", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A bright silver bracelet and quiet dial with contemporary detailing.", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Lunar Silver", 26400m, 0m, "https://example.com/lunar-silver", "LUNAR-SILVER", "lunar-silver", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000010"), null, 1, null, "Black", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A monochrome watch with a flexible mesh bracelet for evening wear.", "https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Midnight Mesh", 28900m, 0m, "https://example.com/midnight-mesh", "MIDNIGHT-MESH", "midnight-mesh", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000011"), null, 1, null, "Beige", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A calm neutral dial and slim case designed for effortless styling.", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", true, "Leather", "Sandstone Minimal", 17600m, 0m, "https://example.com/sandstone-minimal", "SANDSTONE-MINIMAL", "sandstone-minimal", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000012"), null, 1, null, "Copper", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A copper-tone statement watch with balanced contemporary geometry.", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Copper Atelier", 33600m, 0m, "https://example.com/copper-atelier", "COPPER-ATELIER", "copper-atelier", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000013"), null, 1, null, "Navy", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A relaxed watch with a durable canvas strap for casual weekends.", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85", true, "Canvas", "Weekend Canvas", 12900m, 0m, "https://example.com/weekend-canvas", "WEEKEND-CANVAS", "weekend-canvas", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000014"), null, 1, null, "Ivory", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "An ivory dial with polished accents for formal celebrations.", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Ivory Ceremony", 35200m, 0m, "https://example.com/ivory-ceremony", "IVORY-CEREMONY", "ivory-ceremony", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000015"), null, 1, null, "Graphite", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A graphite sports-inspired profile with practical everyday character.", "https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85", true, "Silicone", "Graphite Motion", 23800m, 0m, "https://example.com/graphite-motion", "GRAPHITE-MOTION", "graphite-motion", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-4000-8000-000000000016"), null, 1, null, "Gold", 0m, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A timeless polished watch presented as a versatile gift choice.", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Classic Gift", 30500m, 0m, "https://example.com/classic-gift", "CLASSIC-GIFT", "classic-gift", null, true, false, new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "ProductOccasions",
                columns: new[] { "Occasion", "ProductId" },
                values: new object[,]
                {
                    { 1, new Guid("10000000-0000-4000-8000-000000000005") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000005") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000006") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000006") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000007") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000007") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000008") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000008") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000009") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000009") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000010") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000010") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000011") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000011") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000012") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000012") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000013") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000013") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000014") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000014") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000015") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000015") },
                    { 1, new Guid("10000000-0000-4000-8000-000000000016") },
                    { 3, new Guid("10000000-0000-4000-8000-000000000016") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000005") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000005") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000006") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000006") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000007") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000007") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000008") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000008") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000009") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000009") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000010") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000010") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000011") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000011") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000012") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000012") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000013") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000013") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000014") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000014") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000015") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000015") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 1, new Guid("10000000-0000-4000-8000-000000000016") });

            migrationBuilder.DeleteData(
                table: "ProductOccasions",
                keyColumns: new[] { "Occasion", "ProductId" },
                keyValues: new object[] { 3, new Guid("10000000-0000-4000-8000-000000000016") });

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000015"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000016"));
        }
    }
}
