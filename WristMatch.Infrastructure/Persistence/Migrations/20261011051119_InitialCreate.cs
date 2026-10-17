using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RetailerUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CaseDiameterMm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductOccasions",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Occasion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOccasions", x => new { x.ProductId, x.Occasion });
                    table.ForeignKey(
                        name: "FK_ProductOccasions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CaseDiameterMm", "Category", "Colour", "CreatedAt", "Currency", "Description", "ImageUrl", "IsActive", "Material", "Name", "Price", "RetailerUrl", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c0b6b7f7-945b-4bd1-bf73-8c77d7b25a01"), null, 1, "Silver", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A clean everyday watch with a brushed steel bracelet and balanced proportions.", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Meridian Steel", 18500m, "https://example.com/meridian-steel", "meridian-steel", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("d7b7f7a2-0a53-4f13-a7a0-0f8a9d9b0f02"), null, 1, "Black", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A refined black dial with a leather strap for considered evening dressing.", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85", true, "Leather", "Noir Atelier", 24900m, "https://example.com/noir-atelier", "noir-atelier", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e9e5c453-3188-470f-b8b5-43f1e1f0e303"), null, 1, "Gold", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "Warm champagne tones and a slim case designed for formal occasions.", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85", true, "Stainless Steel", "Champagne Classic", 31900m, "https://example.com/champagne-classic", "champagne-classic", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("f4a1e3b8-8c1d-4c7e-9a0a-2f6d1b7c4404"), null, 1, "Green", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "A practical field-inspired watch with a durable fabric strap.", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85", true, "Nylon", "Field Olive", 14200m, "https://example.com/field-olive", "field-olive", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "ProductOccasions",
                columns: new[] { "Occasion", "ProductId" },
                values: new object[,]
                {
                    { 1, new Guid("c0b6b7f7-945b-4bd1-bf73-8c77d7b25a01") },
                    { 3, new Guid("c0b6b7f7-945b-4bd1-bf73-8c77d7b25a01") },
                    { 4, new Guid("c0b6b7f7-945b-4bd1-bf73-8c77d7b25a01") },
                    { 1, new Guid("d7b7f7a2-0a53-4f13-a7a0-0f8a9d9b0f02") },
                    { 2, new Guid("d7b7f7a2-0a53-4f13-a7a0-0f8a9d9b0f02") },
                    { 4, new Guid("d7b7f7a2-0a53-4f13-a7a0-0f8a9d9b0f02") },
                    { 2, new Guid("e9e5c453-3188-470f-b8b5-43f1e1f0e303") },
                    { 4, new Guid("e9e5c453-3188-470f-b8b5-43f1e1f0e303") },
                    { 1, new Guid("f4a1e3b8-8c1d-4c7e-9a0a-2f6d1b7c4404") },
                    { 3, new Guid("f4a1e3b8-8c1d-4c7e-9a0a-2f6d1b7c4404") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Category_Price",
                table: "Products",
                columns: new[] { "IsActive", "Category", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductOccasions");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
