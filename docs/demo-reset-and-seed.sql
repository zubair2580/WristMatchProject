SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.TryOnJobs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TryOnJobs
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_TryOnJobs PRIMARY KEY,
        SessionId uniqueidentifier NOT NULL,
        PhotoId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL,
        IdempotencyKey nvarchar(120) NOT NULL,
        RequestHash nvarchar(64) NOT NULL,
        State nvarchar(30) NOT NULL,
        ProviderMode nvarchar(40) NOT NULL CONSTRAINT DF_TryOnJobs_ProviderMode_Seed DEFAULT(N'demo'),
        ProviderTaskId nvarchar(120) NULL,
        ResultStorageKey nvarchar(300) NULL,
        SafeMessage nvarchar(500) NULL,
        WorkerLeaseUntil datetimeoffset NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL,
        CompletedAt datetimeoffset NULL,
        ExpiresAt datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_TryOnJobs_SessionId_IdempotencyKey ON dbo.TryOnJobs(SessionId, IdempotencyKey);
    CREATE INDEX IX_TryOnJobs_State_CreatedAt ON dbo.TryOnJobs(State, CreatedAt);
END;

IF OBJECT_ID(N'dbo.StockMovements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockMovements
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_StockMovements PRIMARY KEY,
        WarehouseId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL,
        MovementType nvarchar(40) NOT NULL,
        Quantity decimal(18,4) NOT NULL,
        UnitCost decimal(18,6) NOT NULL,
        TotalCost decimal(18,2) NOT NULL,
        ReferenceNumber nvarchar(60) NOT NULL,
        OccurredAt datetimeoffset NOT NULL,
        Notes nvarchar(500) NULL
    );
END;

IF OBJECT_ID(N'dbo.PurchaseOrders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOrders
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_PurchaseOrders PRIMARY KEY,
        PurchaseOrderNumber nvarchar(40) NOT NULL CONSTRAINT UQ_PurchaseOrders_Number UNIQUE,
        SupplierId uniqueidentifier NOT NULL,
        WarehouseId uniqueidentifier NOT NULL,
        Status nvarchar(30) NOT NULL,
        Currency char(3) NOT NULL,
        TaxRate decimal(9,6) NOT NULL CONSTRAINT DF_PurchaseOrders_TaxRate DEFAULT(0.10),
        Subtotal decimal(18,2) NOT NULL,
        TaxTotal decimal(18,2) NOT NULL,
        GrandTotal decimal(18,2) NOT NULL,
        Notes nvarchar(500) NULL,
        OrderedAt datetimeoffset NOT NULL,
        ExpectedAt datetimeoffset NULL,
        ReceivedAt datetimeoffset NULL,
        UpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_PurchaseOrders_UpdatedAt DEFAULT(SYSUTCDATETIME())
    );
END;

IF OBJECT_ID(N'dbo.PurchaseOrderLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOrderLines
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_PurchaseOrderLines PRIMARY KEY,
        PurchaseOrderId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL,
        Quantity decimal(18,4) NOT NULL,
        ReceivedQuantity decimal(18,4) NOT NULL CONSTRAINT DF_PurchaseOrderLines_ReceivedQuantity DEFAULT(0),
        UnitCost decimal(18,6) NOT NULL,
        LineTotal decimal(18,2) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.SalesOrders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesOrders
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_SalesOrders PRIMARY KEY,
        OrderNumber nvarchar(40) NOT NULL CONSTRAINT UQ_SalesOrders_Number UNIQUE,
        CustomerId uniqueidentifier NOT NULL,
        Status nvarchar(30) NOT NULL,
        PaymentStatus nvarchar(30) NOT NULL,
        FulfilmentStatus nvarchar(30) NOT NULL,
        Currency char(3) NOT NULL,
        Subtotal decimal(18,2) NOT NULL,
        TaxTotal decimal(18,2) NOT NULL,
        GrandTotal decimal(18,2) NOT NULL,
        PlacedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.SalesOrderLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesOrderLines
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_SalesOrderLines PRIMARY KEY,
        SalesOrderId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL,
        Quantity decimal(18,4) NOT NULL,
        UnitPrice decimal(18,2) NOT NULL,
        TaxAmount decimal(18,2) NOT NULL,
        LineTotal decimal(18,2) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.SalesInvoices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesInvoices
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_SalesInvoices PRIMARY KEY,
        InvoiceNumber nvarchar(40) NOT NULL CONSTRAINT UQ_SalesInvoices_Number UNIQUE,
        SalesOrderId uniqueidentifier NOT NULL,
        CustomerId uniqueidentifier NOT NULL,
        Status nvarchar(30) NOT NULL,
        Currency char(3) NOT NULL,
        InvoiceDate datetimeoffset NOT NULL,
        DueDate datetimeoffset NOT NULL,
        Subtotal decimal(18,2) NOT NULL,
        TaxTotal decimal(18,2) NOT NULL,
        GrandTotal decimal(18,2) NOT NULL,
        OutstandingAmount decimal(18,2) NOT NULL,
        Notes nvarchar(500) NULL,
        UpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_SalesInvoices_UpdatedAt DEFAULT(SYSUTCDATETIME())
    );
END;

IF OBJECT_ID(N'dbo.SalesInvoiceLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesInvoiceLines
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_SalesInvoiceLines PRIMARY KEY,
        SalesInvoiceId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL,
        Quantity decimal(18,4) NOT NULL,
        UnitPrice decimal(18,2) NOT NULL,
        TaxAmount decimal(18,2) NOT NULL,
        LineTotal decimal(18,2) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        PaymentNumber nvarchar(40) NOT NULL CONSTRAINT UQ_Payments_Number UNIQUE,
        CustomerId uniqueidentifier NOT NULL,
        SalesInvoiceId uniqueidentifier NOT NULL,
        Method nvarchar(30) NOT NULL,
        Status nvarchar(30) NOT NULL,
        Currency char(3) NOT NULL,
        Amount decimal(18,2) NOT NULL,
        ReceivedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Accounts PRIMARY KEY,
        Code nvarchar(30) NOT NULL CONSTRAINT UQ_Accounts_Code UNIQUE,
        Name nvarchar(160) NOT NULL,
        AccountType nvarchar(30) NOT NULL,
        NormalBalance nvarchar(10) NOT NULL,
        CurrentBalance decimal(18,2) NOT NULL,
        IsActive bit NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.JournalEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalEntries
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_JournalEntries PRIMARY KEY,
        JournalNumber nvarchar(40) NOT NULL CONSTRAINT UQ_JournalEntries_Number UNIQUE,
        EntryDate datetimeoffset NOT NULL,
        SourceType nvarchar(40) NOT NULL,
        SourceId uniqueidentifier NULL,
        Description nvarchar(300) NOT NULL,
        Status nvarchar(20) NOT NULL,
        TotalDebit decimal(18,2) NOT NULL,
        TotalCredit decimal(18,2) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.JournalLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalLines
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_JournalLines PRIMARY KEY,
        JournalEntryId uniqueidentifier NOT NULL,
        AccountId uniqueidentifier NOT NULL,
        Description nvarchar(300) NOT NULL,
        Debit decimal(18,2) NOT NULL,
        Credit decimal(18,2) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.ReportDefinitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReportDefinitions
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_ReportDefinitions PRIMARY KEY,
        Name nvarchar(160) NOT NULL,
        Module nvarchar(50) NOT NULL,
        OwnerEmail nvarchar(256) NOT NULL,
        Format nvarchar(10) NOT NULL,
        Status nvarchar(30) NOT NULL,
        LastRunAt datetimeoffset NULL,
        CreatedAt datetimeoffset NOT NULL
    );
END;

DELETE FROM dbo.JournalLines;
DELETE FROM dbo.JournalEntries;
DELETE FROM dbo.Payments;
DELETE FROM dbo.SalesInvoiceLines;
DELETE FROM dbo.SalesInvoices;
DELETE FROM dbo.SalesOrderLines;
DELETE FROM dbo.SalesOrders;
DELETE FROM dbo.PurchaseOrderLines;
DELETE FROM dbo.PurchaseOrders;
DELETE FROM dbo.StockMovements;
DELETE FROM dbo.TryOnJobs;
DELETE FROM dbo.ReportDefinitions;
DELETE FROM dbo.Accounts;
DELETE FROM dbo.PhotoAssets;
DELETE FROM dbo.GuestSessions;
DELETE FROM dbo.ProductAssetLicences;
DELETE FROM dbo.ProductAssets;
DELETE FROM dbo.ProductOccasions;
DELETE FROM dbo.Products;
DELETE FROM dbo.Categories;
DELETE FROM dbo.CustomerAddresses;
DELETE FROM dbo.CustomerProfiles;
DELETE FROM dbo.Suppliers;
DELETE FROM dbo.Warehouses;
DELETE FROM dbo.TaxRates;
DELETE FROM dbo.TaxCategories;

INSERT INTO dbo.Categories (Id, Name, Slug, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), CONCAT(N'Demo Category ', v.Number), CONCAT(N'demo-category-', v.Number), N'Local testing category', v.Number, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16)) v(Number);

INSERT INTO dbo.Products
(
    Id, Slug, SKU, Name, Description, Price, Currency, Colour, Material, ImageUrl, RetailerUrl,
    Category, CategoryId, CostPrice, ReorderLevel, TaxCategoryId, TrackInventory, TryOnReady,
    IsActive, CreatedAt, UpdatedAt
)
VALUES
('c0b6b7f7-945b-4bd1-bf73-8c77d7b25a01',N'meridian-steel',N'MERIDIAN-STEEL',N'Meridian Steel',N'A clean everyday watch with a brushed steel bracelet.',18500,N'PKR',N'Silver',N'Stainless Steel',N'https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85',N'https://example.com/meridian-steel',1,NULL,12000,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('d7b7f7a2-0a53-4f13-a7a0-0f8a9d9b0f02',N'noir-atelier',N'NOIR-ATELIER',N'Noir Atelier',N'A refined black dial with a leather strap.',24900,N'PKR',N'Black',N'Leather',N'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85',N'https://example.com/noir-atelier',1,NULL,16500,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('e9e5c453-3188-470f-b8b5-43f1e1f0e303',N'champagne-classic',N'CHAMPAGNE-CLASSIC',N'Champagne Classic',N'Warm champagne tones for formal occasions.',31900,N'PKR',N'Gold',N'Stainless Steel',N'https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85',N'https://example.com/champagne-classic',1,NULL,21000,4,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('f4a1e3b8-8c1d-4c7e-9a0a-2f6d1b7c4404',N'field-olive',N'FIELD-OLIVE',N'Field Olive',N'A practical field-inspired watch with a durable fabric strap.',14200,N'PKR',N'Green',N'Nylon',N'https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85',N'https://example.com/field-olive',1,NULL,9000,6,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000005',N'harbour-blue',N'HARBOUR-BLUE',N'Harbour Blue',N'A deep blue dial with a polished steel profile.',22500,N'PKR',N'Blue',N'Stainless Steel',N'https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85',N'https://example.com/harbour-blue',1,NULL,14500,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000006',N'rose-hour',N'ROSE-HOUR',N'Rose Hour',N'A warm rose-tone watch with a minimal face.',27800,N'PKR',N'Rose Gold',N'Stainless Steel',N'https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85',N'https://example.com/rose-hour',1,NULL,18000,4,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000007',N'slate-office',N'SLATE-OFFICE',N'Slate Office',N'A charcoal dial for understated office dressing.',19800,N'PKR',N'Charcoal',N'Stainless Steel',N'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85',N'https://example.com/slate-office',1,NULL,12500,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000008',N'heritage-brown',N'HERITAGE-BROWN',N'Heritage Brown',N'A classic brown leather strap and warm dial.',21400,N'PKR',N'Brown',N'Leather',N'https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85',N'https://example.com/heritage-brown',1,NULL,14000,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000009',N'lunar-silver',N'LUNAR-SILVER',N'Lunar Silver',N'A bright silver bracelet with contemporary detailing.',26400,N'PKR',N'Silver',N'Stainless Steel',N'https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85',N'https://example.com/lunar-silver',1,NULL,17000,4,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000010',N'midnight-mesh',N'MIDNIGHT-MESH',N'Midnight Mesh',N'A monochrome watch with a flexible mesh bracelet.',28900,N'PKR',N'Black',N'Stainless Steel',N'https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85',N'https://example.com/midnight-mesh',1,NULL,19000,4,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000011',N'sandstone-minimal',N'SANDSTONE-MINIMAL',N'Sandstone Minimal',N'A calm neutral dial and slim case.',17600,N'PKR',N'Beige',N'Leather',N'https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85',N'https://example.com/sandstone-minimal',1,NULL,11000,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000012',N'copper-atelier',N'COPPER-ATELIER',N'Copper Atelier',N'A copper-tone statement watch with contemporary geometry.',33600,N'PKR',N'Copper',N'Stainless Steel',N'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85',N'https://example.com/copper-atelier',1,NULL,22500,3,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000013',N'weekend-canvas',N'WEEKEND-CANVAS',N'Weekend Canvas',N'A relaxed watch with a durable canvas strap.',12900,N'PKR',N'Navy',N'Canvas',N'https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85',N'https://example.com/weekend-canvas',1,NULL,8000,7,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000014',N'ivory-ceremony',N'IVORY-CEREMONY',N'Ivory Ceremony',N'An ivory dial with polished accents.',35200,N'PKR',N'Ivory',N'Stainless Steel',N'https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85',N'https://example.com/ivory-ceremony',1,NULL,23500,3,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000015',N'graphite-motion',N'GRAPHITE-MOTION',N'Graphite Motion',N'A graphite sports-inspired everyday profile.',23800,N'PKR',N'Graphite',N'Silicone',N'https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85',N'https://example.com/graphite-motion',1,NULL,15000,5,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME()),
('10000000-0000-4000-8000-000000000016',N'classic-gift',N'CLASSIC-GIFT',N'Classic Gift',N'A timeless polished watch for gifting.',30500,N'PKR',N'Gold',N'Stainless Steel',N'https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85',N'https://example.com/classic-gift',1,NULL,20000,4,NULL,1,0,1,SYSUTCDATETIME(),SYSUTCDATETIME());

INSERT INTO dbo.ProductOccasions (ProductId, Occasion)
SELECT p.Id, o.Occasion
FROM dbo.Products p
CROSS APPLY (VALUES (1),(3)) o(Occasion);

INSERT INTO dbo.CustomerProfiles (Id, CustomerNumber, FirstName, LastName, Email, Phone, IsGuest, MarketingConsent, CreatedAt, UpdatedAt)
SELECT NEWID(), CONCAT(N'CUS-SQL-', FORMAT(v.Number, '0000')), CONCAT(N'Test', v.Number), N'Customer', CONCAT(N'test.customer', v.Number, N'@local.wristmatch'), CONCAT(N'+92 300 700 ', FORMAT(v.Number, '0000')), IIF(v.Number % 2 = 0, 1, 0), IIF(v.Number % 3 = 0, 1, 0), DATEADD(day, -v.Number, SYSUTCDATETIME()), DATEADD(day, -v.Number, SYSUTCDATETIME())
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16)) v(Number);

INSERT INTO dbo.Suppliers (Id, SupplierNumber, Name, ContactName, Email, Phone, TaxNumber, PaymentTermsDays, Currency, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), CONCAT(N'SUP-SQL-', FORMAT(v.Number, '0000')), CONCAT(N'Test Supplier ', v.Number), CONCAT(N'Purchasing Contact ', v.Number), CONCAT(N'supplier', v.Number, N'@local.wristmatch'), CONCAT(N'+92 301 800 ', FORMAT(v.Number, '0000')), CONCAT(N'TAX-SQL-', FORMAT(v.Number, '0000')), 30, N'PKR', IIF(v.Number = 16, 0, 1), DATEADD(day, -v.Number, SYSUTCDATETIME()), DATEADD(day, -v.Number, SYSUTCDATETIME())
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16)) v(Number);

INSERT INTO dbo.Warehouses (Id, Code, Name, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), CONCAT(N'SQL-WH-', FORMAT(v.Number, '0000')), CONCAT(N'Test Warehouse ', v.Number), IIF(v.Number = 16, 0, 1), DATEADD(day, -v.Number, SYSUTCDATETIME()), DATEADD(day, -v.Number, SYSUTCDATETIME())
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16)) v(Number);

INSERT INTO dbo.TaxCategories (Id, Name, Description, IsActive)
VALUES
(NEWID(), N'Standard Test Tax', N'Local testing rate', 1),
(NEWID(), N'Reduced Test Tax', N'Local testing rate', 1),
(NEWID(), N'Zero Test Tax', N'Local testing rate', 1);

INSERT INTO dbo.Accounts (Id, Code, Name, AccountType, NormalBalance, CurrentBalance, IsActive)
VALUES
(NEWID(), N'1000', N'Cash', N'Asset', N'Debit', 250000, 1),
(NEWID(), N'1010', N'Bank', N'Asset', N'Debit', 450000, 1),
(NEWID(), N'1100', N'Accounts Receivable', N'Asset', N'Debit', 125000, 1),
(NEWID(), N'1200', N'Inventory', N'Asset', N'Debit', 780000, 1),
(NEWID(), N'2000', N'Accounts Payable', N'Liability', N'Credit', 210000, 1),
(NEWID(), N'2100', N'Sales Tax Payable', N'Liability', N'Credit', 32500, 1),
(NEWID(), N'3000', N'Owner Equity', N'Equity', N'Credit', 1000000, 1),
(NEWID(), N'4000', N'Watch Sales', N'Revenue', N'Credit', 640000, 1),
(NEWID(), N'4010', N'Accessory Sales', N'Revenue', N'Credit', 95000, 1),
(NEWID(), N'5000', N'Cost of Goods Sold', N'Expense', N'Debit', 385000, 1),
(NEWID(), N'5100', N'Purchasing Expense', N'Expense', N'Debit', 110000, 1),
(NEWID(), N'5200', N'Warehouse Expense', N'Expense', N'Debit', 42000, 1),
(NEWID(), N'5300', N'Marketing Expense', N'Expense', N'Debit', 27000, 1),
(NEWID(), N'5400', N'Payment Fees', N'Expense', N'Debit', 8500, 1),
(NEWID(), N'5500', N'General Expense', N'Expense', N'Debit', 19000, 1);

INSERT INTO dbo.ReportDefinitions (Id, Name, Module, OwnerEmail, Format, Status, LastRunAt, CreatedAt)
SELECT NEWID(), CONCAT(N'Demo ', v.Name, N' Report'), v.Module, N'admin@wristmatch.local', IIF(v.Number % 2 = 0, N'XLSX', N'CSV'), N'Completed', DATEADD(day, -v.Number, SYSUTCDATETIME()), DATEADD(day, -v.Number, SYSUTCDATETIME())
FROM (VALUES
(1,N'Sales',N'Orders'),(2,N'Sales',N'Invoices'),(3,N'Inventory',N'Stock'),(4,N'Inventory',N'Purchases'),
(5,N'Finance',N'Accounts'),(6,N'Finance',N'Journals'),(7,N'Customers',N'Customers'),(8,N'Products',N'Catalogue'),
(9,N'Sales',N'Orders'),(10,N'Inventory',N'Stock'),(11,N'Finance',N'Accounts'),(12,N'Customers',N'Customers'),
(13,N'TryOn',N'Try-on Jobs'),(14,N'Products',N'Catalogue'),(15,N'Sales',N'Invoices'),(16,N'Inventory',N'Purchases')) v(Number, Name, Module);

INSERT INTO dbo.StockMovements (Id, WarehouseId, ProductId, MovementType, Quantity, UnitCost, TotalCost, ReferenceNumber, OccurredAt, Notes)
SELECT NEWID(), w.Id, p.Id, IIF(v.Number % 3 = 0, N'AdjustmentIncrease', N'PurchaseReceipt'), 10 + v.Number, p.CostPrice, (10 + v.Number) * p.CostPrice, CONCAT(N'SM-DEMO-', FORMAT(v.Number, '0000')), DATEADD(day, -v.Number, SYSUTCDATETIME()), N'Local demo stock movement'
FROM (SELECT TOP 15 ROW_NUMBER() OVER (ORDER BY Id) Number, Id FROM dbo.Warehouses WHERE IsActive = 1) w
JOIN (SELECT TOP 15 ROW_NUMBER() OVER (ORDER BY Id) Number, Id, CostPrice FROM dbo.Products WHERE IsActive = 1) p ON p.Number = w.Number
JOIN (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15)) v(Number) ON v.Number = w.Number;

DECLARE @DemoOrders TABLE (Id uniqueidentifier NOT NULL, CustomerId uniqueidentifier NOT NULL, Number int NOT NULL, OrderNumber nvarchar(40) NOT NULL);
INSERT INTO dbo.SalesOrders (Id, OrderNumber, CustomerId, Status, PaymentStatus, FulfilmentStatus, Currency, Subtotal, TaxTotal, GrandTotal, PlacedAt)
OUTPUT inserted.Id, inserted.CustomerId, 0, inserted.OrderNumber INTO @DemoOrders
SELECT NEWID(), CONCAT(N'ORD-DEMO-', FORMAT(v.Number, '0000')), c.Id, IIF(v.Number % 5 = 0, N'Completed', N'Processing'), IIF(v.Number % 3 = 0, N'Paid', N'Pending'), IIF(v.Number % 4 = 0, N'Fulfilled', N'Unfulfilled'), N'PKR', 18000 + v.Number * 350, 1800 + v.Number * 35, 19800 + v.Number * 385, DATEADD(day, -v.Number, SYSUTCDATETIME())
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15)) v(Number)
JOIN (SELECT TOP 15 Id, ROW_NUMBER() OVER (ORDER BY CustomerNumber) Number FROM dbo.CustomerProfiles ORDER BY CustomerNumber) c ON c.Number = v.Number;
UPDATE @DemoOrders SET Number = CONVERT(int, RIGHT(OrderNumber, 4));

INSERT INTO dbo.SalesOrderLines (Id, SalesOrderId, ProductId, Quantity, UnitPrice, TaxAmount, LineTotal)
SELECT NEWID(), o.Id, p.Id, 1, p.Price, p.Price * 0.1, p.Price * 1.1
FROM @DemoOrders o
JOIN (SELECT TOP 15 Id, Price, ROW_NUMBER() OVER (ORDER BY SKU) Number FROM dbo.Products ORDER BY SKU) p ON p.Number = o.Number;

DECLARE @DemoInvoices TABLE (Id uniqueidentifier NOT NULL, SalesOrderId uniqueidentifier NOT NULL, CustomerId uniqueidentifier NOT NULL, Number int NOT NULL, InvoiceNumber nvarchar(40) NOT NULL);
INSERT INTO dbo.SalesInvoices (Id, InvoiceNumber, SalesOrderId, CustomerId, Status, Currency, InvoiceDate, DueDate, Subtotal, TaxTotal, GrandTotal, OutstandingAmount, UpdatedAt)
OUTPUT inserted.Id, inserted.SalesOrderId, inserted.CustomerId, 0, inserted.InvoiceNumber INTO @DemoInvoices
SELECT NEWID(), CONCAT(N'INV-DEMO-', FORMAT(o.Number, '0000')), o.Id, o.CustomerId, IIF(o.Number % 3 = 0, N'Paid', N'Issued'), N'PKR', DATEADD(day, -o.Number, SYSUTCDATETIME()), DATEADD(day, 30 - o.Number, SYSUTCDATETIME()), 18000 + o.Number * 350, 1800 + o.Number * 35, 19800 + o.Number * 385, IIF(o.Number % 3 = 0, 0, 19800 + o.Number * 385), SYSUTCDATETIME()
FROM @DemoOrders o;
UPDATE @DemoInvoices SET Number = CONVERT(int, RIGHT(InvoiceNumber, 4));

INSERT INTO dbo.SalesInvoiceLines (Id, SalesInvoiceId, ProductId, Quantity, UnitPrice, TaxAmount, LineTotal)
SELECT NEWID(), i.Id, p.Id, 1, p.Price, p.Price * 0.1, p.Price * 1.1
FROM @DemoInvoices i
JOIN (SELECT TOP 15 Id, Price, ROW_NUMBER() OVER (ORDER BY SKU) Number FROM dbo.Products ORDER BY SKU) p ON p.Number = i.Number;

INSERT INTO dbo.Payments (Id, PaymentNumber, CustomerId, SalesInvoiceId, Method, Status, Currency, Amount, ReceivedAt)
SELECT NEWID(), CONCAT(N'PAY-DEMO-', FORMAT(i.Number, '0000')), i.CustomerId, i.Id, IIF(i.Number % 2 = 0, N'BankTransfer', N'CashOnDelivery'), N'Confirmed', N'PKR', 19800 + i.Number * 385, DATEADD(day, -i.Number, SYSUTCDATETIME())
FROM @DemoInvoices i
WHERE i.Number % 3 = 0;

DECLARE @DemoPurchases TABLE (Id uniqueidentifier NOT NULL, Number int NOT NULL, PurchaseOrderNumber nvarchar(40) NOT NULL);
INSERT INTO dbo.PurchaseOrders (Id, PurchaseOrderNumber, SupplierId, WarehouseId, Status, Currency, TaxRate, Subtotal, TaxTotal, GrandTotal, OrderedAt, ExpectedAt, ReceivedAt, UpdatedAt)
OUTPUT inserted.Id, 0, inserted.PurchaseOrderNumber INTO @DemoPurchases
SELECT NEWID(), CONCAT(N'PO-DEMO-', FORMAT(v.Number, '0000')), s.Id, w.Id, IIF(v.Number % 4 = 0, N'Received', N'Approved'), N'PKR', 0.10, 80000 + v.Number * 1000, 8000 + v.Number * 100, 88000 + v.Number * 1100, DATEADD(day, -v.Number, SYSUTCDATETIME()), DATEADD(day, 7 - v.Number, SYSUTCDATETIME()), IIF(v.Number % 4 = 0, DATEADD(day, -v.Number, SYSUTCDATETIME()), NULL), SYSUTCDATETIME()
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15)) v(Number)
JOIN (SELECT TOP 15 Id, ROW_NUMBER() OVER (ORDER BY SupplierNumber) Number FROM dbo.Suppliers WHERE IsActive = 1 ORDER BY SupplierNumber) s ON s.Number = v.Number
JOIN (SELECT TOP 15 Id, ROW_NUMBER() OVER (ORDER BY Code) Number FROM dbo.Warehouses WHERE IsActive = 1 ORDER BY Code) w ON w.Number = v.Number;
UPDATE @DemoPurchases SET Number = CONVERT(int, RIGHT(PurchaseOrderNumber, 4));

INSERT INTO dbo.PurchaseOrderLines (Id, PurchaseOrderId, ProductId, Quantity, ReceivedQuantity, UnitCost, LineTotal)
SELECT NEWID(), po.Id, p.Id, 10 + po.Number, IIF(po.Number % 4 = 0, 10 + po.Number, 0), p.CostPrice, (10 + po.Number) * p.CostPrice
FROM @DemoPurchases po
JOIN (SELECT TOP 15 Id, CostPrice, ROW_NUMBER() OVER (ORDER BY SKU) Number FROM dbo.Products ORDER BY SKU) p ON p.Number = po.Number;

-- Try-on jobs are created by the Studio / API worker path (not SQL demo inserts).

INSERT INTO dbo.JournalEntries (Id, JournalNumber, EntryDate, SourceType, SourceId, Description, Status, TotalDebit, TotalCredit)
SELECT NEWID(), CONCAT(N'JE-DEMO-', FORMAT(v.Number, '0000')), DATEADD(day, -v.Number, SYSUTCDATETIME()), N'DemoSeed', NULL, CONCAT(N'Demo journal entry ', v.Number), N'Posted', 10000 + v.Number * 100, 10000 + v.Number * 100
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15)) v(Number);

INSERT INTO dbo.JournalLines (Id, JournalEntryId, AccountId, Description, Debit, Credit)
SELECT NEWID(), entry.Id, account.Id, N'Demo debit line', entry.TotalDebit, 0
FROM dbo.JournalEntries entry
JOIN dbo.Accounts account ON account.Code = N'1000';

INSERT INTO dbo.JournalLines (Id, JournalEntryId, AccountId, Description, Debit, Credit)
SELECT NEWID(), entry.Id, account.Id, N'Demo credit line', 0, entry.TotalCredit
FROM dbo.JournalEntries entry
JOIN dbo.Accounts account ON account.Code = N'4000';

COMMIT TRANSACTION;
