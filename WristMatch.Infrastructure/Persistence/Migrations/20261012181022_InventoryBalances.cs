using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InventoryBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.InventoryBalances', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.InventoryBalances
                    (
                        Id uniqueidentifier NOT NULL CONSTRAINT PK_InventoryBalances PRIMARY KEY,
                        WarehouseId uniqueidentifier NOT NULL,
                        ProductId uniqueidentifier NOT NULL,
                        OnHand decimal(18,4) NOT NULL,
                        Reserved decimal(18,4) NOT NULL,
                        AverageCost decimal(18,6) NOT NULL,
                        CreatedAt datetimeoffset NOT NULL,
                        UpdatedAt datetimeoffset NOT NULL,
                        RowVersion rowversion NOT NULL
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InventoryBalances_WarehouseId_ProductId' AND object_id = OBJECT_ID('dbo.InventoryBalances'))
                    CREATE UNIQUE INDEX IX_InventoryBalances_WarehouseId_ProductId ON dbo.InventoryBalances(WarehouseId, ProductId);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.InventoryBalances', N'U') IS NOT NULL
                    DROP TABLE dbo.InventoryBalances;
                """);
        }
    }
}
