using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesInvoicesDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.SalesInvoices', 'Notes') IS NULL
                    ALTER TABLE dbo.SalesInvoices ADD Notes nvarchar(500) NULL;
                IF COL_LENGTH('dbo.SalesInvoices', 'UpdatedAt') IS NULL
                    ALTER TABLE dbo.SalesInvoices ADD UpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_SalesInvoices_UpdatedAt DEFAULT(SYSUTCDATETIME());
                """);

            migrationBuilder.Sql("""
                UPDATE dbo.SalesInvoices
                SET UpdatedAt = COALESCE(UpdatedAt, InvoiceDate);
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FK_SalesInvoiceLines_SalesInvoices_SalesInvoiceId', N'F') IS NULL
                   AND OBJECT_ID(N'dbo.SalesInvoiceLines', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.SalesInvoiceLines WITH NOCHECK
                    ADD CONSTRAINT FK_SalesInvoiceLines_SalesInvoices_SalesInvoiceId
                    FOREIGN KEY (SalesInvoiceId) REFERENCES dbo.SalesInvoices(Id) ON DELETE CASCADE;
                END

                IF OBJECT_ID(N'dbo.FK_Payments_SalesInvoices_SalesInvoiceId', N'F') IS NULL
                   AND OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.Payments WITH NOCHECK
                    ADD CONSTRAINT FK_Payments_SalesInvoices_SalesInvoiceId
                    FOREIGN KEY (SalesInvoiceId) REFERENCES dbo.SalesInvoices(Id);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesInvoiceLines_SalesInvoiceId' AND object_id = OBJECT_ID(N'dbo.SalesInvoiceLines'))
                    CREATE INDEX IX_SalesInvoiceLines_SalesInvoiceId ON dbo.SalesInvoiceLines (SalesInvoiceId);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_SalesInvoiceId' AND object_id = OBJECT_ID(N'dbo.Payments'))
                    CREATE INDEX IX_Payments_SalesInvoiceId ON dbo.Payments (SalesInvoiceId);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesInvoices_InvoiceNumber' AND object_id = OBJECT_ID(N'dbo.SalesInvoices'))
                   AND OBJECT_ID(N'dbo.UQ_SalesInvoices_Number', N'UQ') IS NULL
                    CREATE UNIQUE INDEX IX_SalesInvoices_InvoiceNumber ON dbo.SalesInvoices (InvoiceNumber);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_PaymentNumber' AND object_id = OBJECT_ID(N'dbo.Payments'))
                   AND OBJECT_ID(N'dbo.UQ_Payments_Number', N'UQ') IS NULL
                    CREATE UNIQUE INDEX IX_Payments_PaymentNumber ON dbo.Payments (PaymentNumber);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FK_Payments_SalesInvoices_SalesInvoiceId', N'F') IS NOT NULL
                    ALTER TABLE dbo.Payments DROP CONSTRAINT FK_Payments_SalesInvoices_SalesInvoiceId;
                IF OBJECT_ID(N'dbo.FK_SalesInvoiceLines_SalesInvoices_SalesInvoiceId', N'F') IS NOT NULL
                    ALTER TABLE dbo.SalesInvoiceLines DROP CONSTRAINT FK_SalesInvoiceLines_SalesInvoices_SalesInvoiceId;
                """);
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesInvoiceLines_SalesInvoiceId' AND object_id = OBJECT_ID(N'dbo.SalesInvoiceLines'))
                    DROP INDEX IX_SalesInvoiceLines_SalesInvoiceId ON dbo.SalesInvoiceLines;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_SalesInvoiceId' AND object_id = OBJECT_ID(N'dbo.Payments'))
                    DROP INDEX IX_Payments_SalesInvoiceId ON dbo.Payments;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesInvoices_InvoiceNumber' AND object_id = OBJECT_ID(N'dbo.SalesInvoices'))
                    DROP INDEX IX_SalesInvoices_InvoiceNumber ON dbo.SalesInvoices;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_PaymentNumber' AND object_id = OBJECT_ID(N'dbo.Payments'))
                    DROP INDEX IX_Payments_PaymentNumber ON dbo.Payments;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.SalesInvoices', 'Notes') IS NOT NULL
                    ALTER TABLE dbo.SalesInvoices DROP COLUMN Notes;
                IF OBJECT_ID(N'dbo.DF_SalesInvoices_UpdatedAt', N'D') IS NOT NULL
                    ALTER TABLE dbo.SalesInvoices DROP CONSTRAINT DF_SalesInvoices_UpdatedAt;
                IF COL_LENGTH('dbo.SalesInvoices', 'UpdatedAt') IS NOT NULL
                    ALTER TABLE dbo.SalesInvoices DROP COLUMN UpdatedAt;
                """);
        }
    }
}
