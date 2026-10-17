using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WristMatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DemoFoundationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IsActive", "Name", "ParentCategoryId", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-4000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Practical watches for daily wear.", 1, true, "Everyday Watches", null, "everyday-watches", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Refined watches for formal occasions.", 2, true, "Dress Watches", null, "dress-watches", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Durable field-inspired watches.", 3, true, "Field Watches", null, "field-watches", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Stainless steel bracelet watches.", 4, true, "Steel Collection", null, "steel-collection", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Leather strap watches.", 5, true, "Leather Collection", null, "leather-collection", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Clean, understated dial designs.", 6, true, "Minimal Watches", null, "minimal-watches", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000007"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Distinctive watches with a strong presence.", 7, true, "Statement Watches", null, "statement-watches", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000008"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Considered watches for gifting.", 8, true, "Gift Watches", null, "gift-watches", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000009"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Professional watches for the working week.", 9, true, "Office Edit", null, "office-edit", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000010"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Formal watches for celebrations.", 10, true, "Wedding Edit", null, "wedding-edit", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000011"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Relaxed watches for weekends.", 11, true, "Weekend Edit", null, "weekend-edit", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000012"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Black dial watches.", 12, true, "Black Dial", null, "black-dial", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000013"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Blue dial watches.", 13, true, "Blue Dial", null, "blue-dial", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000014"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Warm gold and champagne finishes.", 14, true, "Gold Tone", null, "gold-tone", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000015"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Ivory, beige, and sandstone tones.", 15, true, "Neutral Tones", null, "neutral-tones", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("20000000-0000-4000-8000-000000000016"), new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Inactive testing category.", 16, false, "Sample Archive", null, "sample-archive", new DateTimeOffset(new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "CustomerProfiles",
                columns: new[] { "Id", "CreatedAt", "CustomerNumber", "Email", "FirstName", "IsGuest", "LastName", "MarketingConsent", "Phone", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("21000000-0000-4000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0001", "demo.customer1@wristmatch.local", "Demo1", false, "Customer", false, "+92 300 555 0001", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0002", "demo.customer2@wristmatch.local", "Demo2", true, "Customer", false, "+92 300 555 0002", new DateTimeOffset(new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0003", "demo.customer3@wristmatch.local", "Demo3", false, "Customer", true, "+92 300 555 0003", new DateTimeOffset(new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0004", "demo.customer4@wristmatch.local", "Demo4", true, "Customer", false, "+92 300 555 0004", new DateTimeOffset(new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0005", "demo.customer5@wristmatch.local", "Demo5", false, "Customer", false, "+92 300 555 0005", new DateTimeOffset(new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0006", "demo.customer6@wristmatch.local", "Demo6", true, "Customer", true, "+92 300 555 0006", new DateTimeOffset(new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000007"), new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0007", "demo.customer7@wristmatch.local", "Demo7", false, "Customer", false, "+92 300 555 0007", new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000008"), new DateTimeOffset(new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0008", "demo.customer8@wristmatch.local", "Demo8", true, "Customer", false, "+92 300 555 0008", new DateTimeOffset(new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000009"), new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0009", "demo.customer9@wristmatch.local", "Demo9", false, "Customer", true, "+92 300 555 0009", new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000010"), new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0010", "demo.customer10@wristmatch.local", "Demo10", true, "Customer", false, "+92 300 555 0010", new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000011"), new DateTimeOffset(new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0011", "demo.customer11@wristmatch.local", "Demo11", false, "Customer", false, "+92 300 555 0011", new DateTimeOffset(new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000012"), new DateTimeOffset(new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0012", "demo.customer12@wristmatch.local", "Demo12", true, "Customer", true, "+92 300 555 0012", new DateTimeOffset(new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000013"), new DateTimeOffset(new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0013", "demo.customer13@wristmatch.local", "Demo13", false, "Customer", false, "+92 300 555 0013", new DateTimeOffset(new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000014"), new DateTimeOffset(new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0014", "demo.customer14@wristmatch.local", "Demo14", true, "Customer", false, "+92 300 555 0014", new DateTimeOffset(new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000015"), new DateTimeOffset(new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0015", "demo.customer15@wristmatch.local", "Demo15", false, "Customer", true, "+92 300 555 0015", new DateTimeOffset(new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("21000000-0000-4000-8000-000000000016"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "CUS-DEMO-0016", "demo.customer16@wristmatch.local", "Demo16", true, "Customer", false, "+92 300 555 0016", new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "ContactName", "CreatedAt", "Currency", "Email", "IsActive", "Name", "PaymentTermsDays", "Phone", "SupplierNumber", "TaxNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("22000000-0000-4000-8000-000000000001"), "Contact 1", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier1@demo.wristmatch.local", true, "Demo Supply Partner 1", 30, "+92 301 666 0001", "SUP-DEMO-0001", "DEMO-TAX-0001", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000002"), "Contact 2", new DateTimeOffset(new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier2@demo.wristmatch.local", true, "Demo Supply Partner 2", 45, "+92 301 666 0002", "SUP-DEMO-0002", "DEMO-TAX-0002", new DateTimeOffset(new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000003"), "Contact 3", new DateTimeOffset(new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier3@demo.wristmatch.local", true, "Demo Supply Partner 3", 60, "+92 301 666 0003", "SUP-DEMO-0003", "DEMO-TAX-0003", new DateTimeOffset(new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000004"), "Contact 4", new DateTimeOffset(new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier4@demo.wristmatch.local", true, "Demo Supply Partner 4", 15, "+92 301 666 0004", "SUP-DEMO-0004", "DEMO-TAX-0004", new DateTimeOffset(new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000005"), "Contact 5", new DateTimeOffset(new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier5@demo.wristmatch.local", true, "Demo Supply Partner 5", 30, "+92 301 666 0005", "SUP-DEMO-0005", "DEMO-TAX-0005", new DateTimeOffset(new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000006"), "Contact 6", new DateTimeOffset(new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier6@demo.wristmatch.local", true, "Demo Supply Partner 6", 45, "+92 301 666 0006", "SUP-DEMO-0006", "DEMO-TAX-0006", new DateTimeOffset(new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000007"), "Contact 7", new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier7@demo.wristmatch.local", true, "Demo Supply Partner 7", 60, "+92 301 666 0007", "SUP-DEMO-0007", "DEMO-TAX-0007", new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000008"), "Contact 8", new DateTimeOffset(new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier8@demo.wristmatch.local", true, "Demo Supply Partner 8", 15, "+92 301 666 0008", "SUP-DEMO-0008", "DEMO-TAX-0008", new DateTimeOffset(new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000009"), "Contact 9", new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier9@demo.wristmatch.local", true, "Demo Supply Partner 9", 30, "+92 301 666 0009", "SUP-DEMO-0009", "DEMO-TAX-0009", new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000010"), "Contact 10", new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier10@demo.wristmatch.local", true, "Demo Supply Partner 10", 45, "+92 301 666 0010", "SUP-DEMO-0010", "DEMO-TAX-0010", new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000011"), "Contact 11", new DateTimeOffset(new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier11@demo.wristmatch.local", true, "Demo Supply Partner 11", 60, "+92 301 666 0011", "SUP-DEMO-0011", "DEMO-TAX-0011", new DateTimeOffset(new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000012"), "Contact 12", new DateTimeOffset(new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier12@demo.wristmatch.local", true, "Demo Supply Partner 12", 15, "+92 301 666 0012", "SUP-DEMO-0012", "DEMO-TAX-0012", new DateTimeOffset(new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000013"), "Contact 13", new DateTimeOffset(new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier13@demo.wristmatch.local", true, "Demo Supply Partner 13", 30, "+92 301 666 0013", "SUP-DEMO-0013", "DEMO-TAX-0013", new DateTimeOffset(new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000014"), "Contact 14", new DateTimeOffset(new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier14@demo.wristmatch.local", true, "Demo Supply Partner 14", 45, "+92 301 666 0014", "SUP-DEMO-0014", "DEMO-TAX-0014", new DateTimeOffset(new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000015"), "Contact 15", new DateTimeOffset(new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier15@demo.wristmatch.local", true, "Demo Supply Partner 15", 60, "+92 301 666 0015", "SUP-DEMO-0015", "DEMO-TAX-0015", new DateTimeOffset(new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22000000-0000-4000-8000-000000000016"), "Contact 16", new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PKR", "supplier16@demo.wristmatch.local", false, "Demo Supply Partner 16", 15, "+92 301 666 0016", "SUP-DEMO-0016", "DEMO-TAX-0016", new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "TaxCategories",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("24000000-0000-4000-8000-000000000001"), "Demo configurable standard rate.", true, "Standard Demo Tax" },
                    { new Guid("24000000-0000-4000-8000-000000000002"), "Demo configurable reduced rate.", true, "Reduced Demo Tax" },
                    { new Guid("24000000-0000-4000-8000-000000000003"), "Demo zero rate.", true, "Zero Demo Tax" }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Code", "CreatedAt", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("23000000-0000-4000-8000-000000000001"), "WH-0001", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 1", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000002"), "WH-0002", new DateTimeOffset(new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 2", new DateTimeOffset(new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000003"), "WH-0003", new DateTimeOffset(new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 3", new DateTimeOffset(new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000004"), "WH-0004", new DateTimeOffset(new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 4", new DateTimeOffset(new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000005"), "WH-0005", new DateTimeOffset(new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 5", new DateTimeOffset(new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000006"), "WH-0006", new DateTimeOffset(new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 6", new DateTimeOffset(new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000007"), "WH-0007", new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 7", new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000008"), "WH-0008", new DateTimeOffset(new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 8", new DateTimeOffset(new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000009"), "WH-0009", new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 9", new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000010"), "WH-0010", new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 10", new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000011"), "WH-0011", new DateTimeOffset(new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 11", new DateTimeOffset(new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000012"), "WH-0012", new DateTimeOffset(new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 12", new DateTimeOffset(new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000013"), "WH-0013", new DateTimeOffset(new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 13", new DateTimeOffset(new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000014"), "WH-0014", new DateTimeOffset(new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 14", new DateTimeOffset(new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000015"), "WH-0015", new DateTimeOffset(new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Demo Warehouse 15", new DateTimeOffset(new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("23000000-0000-4000-8000-000000000016"), "WH-0016", new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "Demo Warehouse 16", new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000015"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CustomerProfiles",
                keyColumn: "Id",
                keyValue: new Guid("21000000-0000-4000-8000-000000000016"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000015"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("22000000-0000-4000-8000-000000000016"));

            migrationBuilder.DeleteData(
                table: "TaxCategories",
                keyColumn: "Id",
                keyValue: new Guid("24000000-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TaxCategories",
                keyColumn: "Id",
                keyValue: new Guid("24000000-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TaxCategories",
                keyColumn: "Id",
                keyValue: new Guid("24000000-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000015"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("23000000-0000-4000-8000-000000000016"));
        }
    }
}
