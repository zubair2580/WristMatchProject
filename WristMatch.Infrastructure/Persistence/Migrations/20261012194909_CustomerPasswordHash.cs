using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomerPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "SalesInvoices",
                type: "char(3)",
                fixedLength: true,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "char(3)",
                fixedLength: true,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "CustomerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000001"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000002"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000003"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000004"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000005"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000006"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000007"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000008"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000009"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000010"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000011"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000012"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000013"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000014"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000015"),
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000016"),
                column: "PasswordHash",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "CustomerProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "SalesInvoices",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldFixedLength: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldFixedLength: true);
        }
    }
}
