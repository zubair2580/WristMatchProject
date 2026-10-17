using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrdersDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.SalesOrders', 'WarehouseId') IS NULL
                    ALTER TABLE dbo.SalesOrders ADD WarehouseId uniqueidentifier NULL;
                IF COL_LENGTH('dbo.SalesOrders', 'TaxRate') IS NULL
                    ALTER TABLE dbo.SalesOrders ADD TaxRate decimal(9,6) NOT NULL CONSTRAINT DF_SalesOrders_TaxRate DEFAULT(0.10);
                IF COL_LENGTH('dbo.SalesOrders', 'Notes') IS NULL
                    ALTER TABLE dbo.SalesOrders ADD Notes nvarchar(500) NULL;
                IF COL_LENGTH('dbo.SalesOrders', 'UpdatedAt') IS NULL
                    ALTER TABLE dbo.SalesOrders ADD UpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_SalesOrders_UpdatedAt DEFAULT(SYSUTCDATETIME());
                """);

            migrationBuilder.Sql("""
                UPDATE dbo.SalesOrders
                SET WarehouseId = COALESCE(WarehouseId, (SELECT TOP 1 Id FROM dbo.Warehouses WHERE IsActive = 1 ORDER BY Code))
                WHERE WarehouseId IS NULL;
                """);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SalesOrders') AND name = 'WarehouseId' AND is_nullable = 1)
                    ALTER TABLE dbo.SalesOrders ALTER COLUMN WarehouseId uniqueidentifier NOT NULL;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FK_SalesOrderLines_SalesOrders_SalesOrderId', N'F') IS NULL
                   AND OBJECT_ID(N'dbo.SalesOrderLines', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.SalesOrderLines WITH NOCHECK
                    ADD CONSTRAINT FK_SalesOrderLines_SalesOrders_SalesOrderId
                    FOREIGN KEY (SalesOrderId) REFERENCES dbo.SalesOrders(Id) ON DELETE CASCADE;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FK_SalesOrderLines_SalesOrders_SalesOrderId', N'F') IS NOT NULL
                    ALTER TABLE dbo.SalesOrderLines DROP CONSTRAINT FK_SalesOrderLines_SalesOrders_SalesOrderId;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.SalesOrders', 'WarehouseId') IS NOT NULL ALTER TABLE dbo.SalesOrders DROP COLUMN WarehouseId;
                IF OBJECT_ID(N'dbo.DF_SalesOrders_TaxRate', N'D') IS NOT NULL ALTER TABLE dbo.SalesOrders DROP CONSTRAINT DF_SalesOrders_TaxRate;
                IF COL_LENGTH('dbo.SalesOrders', 'TaxRate') IS NOT NULL ALTER TABLE dbo.SalesOrders DROP COLUMN TaxRate;
                IF COL_LENGTH('dbo.SalesOrders', 'Notes') IS NOT NULL ALTER TABLE dbo.SalesOrders DROP COLUMN Notes;
                IF OBJECT_ID(N'dbo.DF_SalesOrders_UpdatedAt', N'D') IS NOT NULL ALTER TABLE dbo.SalesOrders DROP CONSTRAINT DF_SalesOrders_UpdatedAt;
                IF COL_LENGTH('dbo.SalesOrders', 'UpdatedAt') IS NOT NULL ALTER TABLE dbo.SalesOrders DROP COLUMN UpdatedAt;
                """);
        }
    }
}
