using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TryOnJobsDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.TryOnJobs', 'IdempotencyKey') IS NULL
                BEGIN
                    ALTER TABLE dbo.TryOnJobs ADD
                        IdempotencyKey nvarchar(120) NOT NULL CONSTRAINT DF_TryOnJobs_IdempotencyKey DEFAULT(''),
                        RequestHash nvarchar(64) NOT NULL CONSTRAINT DF_TryOnJobs_RequestHash DEFAULT(''),
                        ProviderMode nvarchar(40) NOT NULL CONSTRAINT DF_TryOnJobs_ProviderMode DEFAULT('demo'),
                        ResultStorageKey nvarchar(300) NULL,
                        WorkerLeaseUntil datetimeoffset NULL,
                        ExpiresAt datetimeoffset NOT NULL CONSTRAINT DF_TryOnJobs_ExpiresAt DEFAULT(SYSUTCDATETIME());
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TryOnJobs') AND name = 'SessionId' AND is_nullable = 1)
                    ALTER TABLE dbo.TryOnJobs ALTER COLUMN SessionId uniqueidentifier NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TryOnJobs') AND name = 'PhotoId' AND is_nullable = 1)
                    ALTER TABLE dbo.TryOnJobs ALTER COLUMN PhotoId uniqueidentifier NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TryOnJobs') AND name = 'ProductId' AND is_nullable = 1)
                    ALTER TABLE dbo.TryOnJobs ALTER COLUMN ProductId uniqueidentifier NOT NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TryOnJobs_SessionId_IdempotencyKey' AND object_id = OBJECT_ID('dbo.TryOnJobs'))
                    CREATE UNIQUE INDEX IX_TryOnJobs_SessionId_IdempotencyKey ON dbo.TryOnJobs(SessionId, IdempotencyKey);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TryOnJobs_State_CreatedAt' AND object_id = OBJECT_ID('dbo.TryOnJobs'))
                    CREATE INDEX IX_TryOnJobs_State_CreatedAt ON dbo.TryOnJobs(State, CreatedAt);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TryOnJobs_SessionId_IdempotencyKey' AND object_id = OBJECT_ID('dbo.TryOnJobs'))
                    DROP INDEX IX_TryOnJobs_SessionId_IdempotencyKey ON dbo.TryOnJobs;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TryOnJobs_State_CreatedAt' AND object_id = OBJECT_ID('dbo.TryOnJobs'))
                    DROP INDEX IX_TryOnJobs_State_CreatedAt ON dbo.TryOnJobs;

                IF COL_LENGTH('dbo.TryOnJobs', 'IdempotencyKey') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP CONSTRAINT DF_TryOnJobs_IdempotencyKey;
                IF COL_LENGTH('dbo.TryOnJobs', 'RequestHash') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP CONSTRAINT DF_TryOnJobs_RequestHash;
                IF COL_LENGTH('dbo.TryOnJobs', 'ProviderMode') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP CONSTRAINT DF_TryOnJobs_ProviderMode;
                IF COL_LENGTH('dbo.TryOnJobs', 'ExpiresAt') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP CONSTRAINT DF_TryOnJobs_ExpiresAt;

                IF COL_LENGTH('dbo.TryOnJobs', 'IdempotencyKey') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP COLUMN IdempotencyKey;
                IF COL_LENGTH('dbo.TryOnJobs', 'RequestHash') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP COLUMN RequestHash;
                IF COL_LENGTH('dbo.TryOnJobs', 'ProviderMode') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP COLUMN ProviderMode;
                IF COL_LENGTH('dbo.TryOnJobs', 'ResultStorageKey') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP COLUMN ResultStorageKey;
                IF COL_LENGTH('dbo.TryOnJobs', 'WorkerLeaseUntil') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP COLUMN WorkerLeaseUntil;
                IF COL_LENGTH('dbo.TryOnJobs', 'ExpiresAt') IS NOT NULL ALTER TABLE dbo.TryOnJobs DROP COLUMN ExpiresAt;
                """);
        }
    }
}
