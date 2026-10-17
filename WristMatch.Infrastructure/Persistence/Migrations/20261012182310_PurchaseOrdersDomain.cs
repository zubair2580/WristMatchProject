using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrdersDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.PurchaseOrders', 'TaxRate') IS NULL
                    ALTER TABLE dbo.PurchaseOrders ADD TaxRate decimal(9,6) NOT NULL CONSTRAINT DF_PurchaseOrders_TaxRate DEFAULT(0.10);
                IF COL_LENGTH('dbo.PurchaseOrders', 'Notes') IS NULL
                    ALTER TABLE dbo.PurchaseOrders ADD Notes nvarchar(500) NULL;
                IF COL_LENGTH('dbo.PurchaseOrders', 'ReceivedAt') IS NULL
                    ALTER TABLE dbo.PurchaseOrders ADD ReceivedAt datetimeoffset NULL;
                IF COL_LENGTH('dbo.PurchaseOrders', 'UpdatedAt') IS NULL
                    ALTER TABLE dbo.PurchaseOrders ADD UpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_PurchaseOrders_UpdatedAt DEFAULT(SYSUTCDATETIME());
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.PurchaseOrderLines', 'ReceivedQuantity') IS NULL
                    ALTER TABLE dbo.PurchaseOrderLines ADD ReceivedQuantity decimal(18,4) NOT NULL CONSTRAINT DF_PurchaseOrderLines_ReceivedQuantity DEFAULT(0);
                """);

            migrationBuilder.Sql("""
                UPDATE pol
                SET ReceivedQuantity = pol.Quantity
                FROM dbo.PurchaseOrderLines pol
                INNER JOIN dbo.PurchaseOrders po ON po.Id = pol.PurchaseOrderId
                WHERE po.Status = N'Received' AND pol.ReceivedQuantity = 0;

                UPDATE dbo.PurchaseOrders
                SET ReceivedAt = COALESCE(ReceivedAt, OrderedAt),
                    UpdatedAt = COALESCE(UpdatedAt, OrderedAt)
                WHERE Status = N'Received';
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId', N'F') IS NULL
                   AND OBJECT_ID(N'dbo.PurchaseOrderLines', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.PurchaseOrderLines WITH NOCHECK
                    ADD CONSTRAINT FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId
                    FOREIGN KEY (PurchaseOrderId) REFERENCES dbo.PurchaseOrders(Id) ON DELETE CASCADE;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrderLines_PurchaseOrderId' AND object_id = OBJECT_ID(N'dbo.PurchaseOrderLines'))
                    CREATE INDEX IX_PurchaseOrderLines_PurchaseOrderId ON dbo.PurchaseOrderLines (PurchaseOrderId);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrders_PurchaseOrderNumber' AND object_id = OBJECT_ID(N'dbo.PurchaseOrders'))
                   AND OBJECT_ID(N'dbo.UQ_PurchaseOrders_Number', N'UQ') IS NULL
                    CREATE UNIQUE INDEX IX_PurchaseOrders_PurchaseOrderNumber ON dbo.PurchaseOrders (PurchaseOrderNumber);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId', N'F') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrderLines DROP CONSTRAINT FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId;
                """);
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrderLines_PurchaseOrderId' AND object_id = OBJECT_ID(N'dbo.PurchaseOrderLines'))
                    DROP INDEX IX_PurchaseOrderLines_PurchaseOrderId ON dbo.PurchaseOrderLines;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrders_PurchaseOrderNumber' AND object_id = OBJECT_ID(N'dbo.PurchaseOrders'))
                    DROP INDEX IX_PurchaseOrders_PurchaseOrderNumber ON dbo.PurchaseOrders;
                """);
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.DF_PurchaseOrderLines_ReceivedQuantity', N'D') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrderLines DROP CONSTRAINT DF_PurchaseOrderLines_ReceivedQuantity;
                IF COL_LENGTH('dbo.PurchaseOrderLines', 'ReceivedQuantity') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrderLines DROP COLUMN ReceivedQuantity;
                IF OBJECT_ID(N'dbo.DF_PurchaseOrders_TaxRate', N'D') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrders DROP CONSTRAINT DF_PurchaseOrders_TaxRate;
                IF COL_LENGTH('dbo.PurchaseOrders', 'TaxRate') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrders DROP COLUMN TaxRate;
                IF COL_LENGTH('dbo.PurchaseOrders', 'Notes') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrders DROP COLUMN Notes;
                IF COL_LENGTH('dbo.PurchaseOrders', 'ReceivedAt') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrders DROP COLUMN ReceivedAt;
                IF OBJECT_ID(N'dbo.DF_PurchaseOrders_UpdatedAt', N'D') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrders DROP CONSTRAINT DF_PurchaseOrders_UpdatedAt;
                IF COL_LENGTH('dbo.PurchaseOrders', 'UpdatedAt') IS NOT NULL
                    ALTER TABLE dbo.PurchaseOrders DROP COLUMN UpdatedAt;
                """);
        }
    }
}
