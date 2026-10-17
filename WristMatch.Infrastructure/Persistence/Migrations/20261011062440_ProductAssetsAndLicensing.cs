using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductAssetsAndLicensing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    ByteSize = table.Column<long>(type: "bigint", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAssets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductAssetLicences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LicenceNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Attribution = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAssetLicences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAssetLicences_ProductAssets_ProductAssetId",
                        column: x => x.ProductAssetId,
                        principalTable: "ProductAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAssetLicences_ProductAssetId",
                table: "ProductAssetLicences",
                column: "ProductAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAssets_ProductId_IsApproved_IsPrimary",
                table: "ProductAssets",
                columns: new[] { "ProductId", "IsApproved", "IsPrimary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAssetLicences");

            migrationBuilder.DropTable(
                name: "ProductAssets");
        }
    }
}
