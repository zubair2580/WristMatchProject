# WristMatch commerce ERP design

## Confirmed scope

WristMatch includes a public storefront with guest and registered-customer checkout, cash on delivery, bank transfer, an abstract card-payment gateway, configurable tax rates, weighted-average inventory costing, supplier purchasing, invoicing, payments, double-entry accounting, operational reports, spreadsheet export, and dashboard analytics.

The card gateway remains provider-neutral until a specific provider and current contract are approved. No payment credentials, card numbers, CVV values, or raw gateway payloads are stored.

## Module boundaries

1. Identity and access
2. Customer relationship management
3. Product catalogue and categories
4. Warehouses and inventory
5. Suppliers and purchasing
6. Carts, checkout, sales orders, and fulfilment
7. Invoices and payment allocation
8. Double-entry general ledger
9. Try-on operations
10. Reporting, exports, and audit

## Shared conventions

- SQL Server identifiers use `uniqueidentifier`.
- Dates use UTC `datetimeoffset`.
- Money uses `decimal(18,2)` and a required `char(3)` currency.
- Quantities use `decimal(18,4)`.
- Unit costs use `decimal(18,6)`.
- Mutable aggregate roots use `rowversion`.
- Documents have immutable internal IDs and human-readable sequential numbers.
- Posted accounting and stock movements are never edited or deleted; corrections use reversal documents.
- Customer deletion anonymizes retained commercial records where legal retention requires them.
- All list APIs support `page`, `pageSize`, search, filters, sort field, and sort direction.
- Spreadsheet exports repeat the same authorization and filter rules as list APIs.

## Identity and access

### AspNetUsers

Supports administrators and registered customers through explicit user type and roles.

Additional fields: DisplayName, UserType, IsActive, CreatedAt, UpdatedAt, LastLoginAt, RowVersion.

Admin roles: SuperAdmin, CatalogManager, OperationsManager, PurchasingManager, Accountant, ReportViewer, SupportViewer.

Customer role: Customer.

### CustomerProfiles

Id, UserId nullable, CustomerNumber unique, FirstName, LastName, Email, Phone, IsGuest, MarketingConsent, CreatedAt, UpdatedAt, RowVersion.

A guest checkout creates a customer profile without credentials. Later account conversion can attach a user without changing commercial history.

### CustomerAddresses

Id, CustomerId, Type, RecipientName, Phone, AddressLine1, AddressLine2, City, StateProvince, PostalCode, CountryCode, IsDefault, CreatedAt, UpdatedAt.

## Catalogue

### Categories

Id, ParentCategoryId nullable, Name, Slug unique, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt, RowVersion.

Supports hierarchical navigation without hard-coded category enums.

### Products

Existing product fields plus CategoryId, SKU unique, Barcode nullable, CostPrice, TrackInventory, ReorderLevel, TaxCategoryId, IsFeatured, PublishedAt, RowVersion.

Product category remains Watch or Bracelet for try-on behavior, while CategoryId controls storefront merchandising.

### ProductVariants

Id, ProductId, SKU unique, Name, Colour, Material, SizeLabel, PriceAdjustment, CostAdjustment, IsActive, RowVersion.

Initial watches may use one default variant. The table prevents later inventory redesign.

### ProductAssets and ProductAssetLicences

Existing approved catalogue image and licensing model. Assets can attach to a product or variant. One approved asset is primary.

### ProductOccasions

Existing product-to-occasion mapping.

### TaxCategories

Id, Name, Description, IsActive.

### TaxRates

Id, TaxCategoryId, Name, Rate decimal(9,6), CountryCode, StateProvince nullable, EffectiveFrom, EffectiveTo nullable, IsActive.

Order lines snapshot the applied tax name and rate.

## Warehouses and inventory

### Warehouses

Id, Code unique, Name, Address fields, IsActive, CreatedAt, UpdatedAt, RowVersion.

### InventoryBalances

WarehouseId and ProductVariantId composite key, QuantityOnHand, QuantityReserved, QuantityAvailable computed by application, AverageUnitCost, InventoryValue, UpdatedAt, RowVersion.

### StockMovements

Id, WarehouseId, ProductVariantId, MovementType, Quantity, UnitCost, TotalCost, ReferenceType, ReferenceId, OccurredAt, CreatedByUserId, ReversalOfMovementId nullable.

Movement types: OpeningBalance, PurchaseReceipt, SaleShipment, CustomerReturn, SupplierReturn, AdjustmentIncrease, AdjustmentDecrease, TransferOut, TransferIn, Reservation, ReservationRelease.

### InventoryReservations

Id, OrderId, WarehouseId, ProductVariantId, Quantity, State, ExpiresAt, CreatedAt, ReleasedAt nullable, RowVersion.

Reservations prevent overselling between order placement and fulfilment.

### StockTransfers

Id, TransferNumber unique, FromWarehouseId, ToWarehouseId, State, RequestedAt, ShippedAt nullable, ReceivedAt nullable, CreatedByUserId, RowVersion.

### StockTransferLines

Id, StockTransferId, ProductVariantId, RequestedQuantity, ShippedQuantity, ReceivedQuantity.

### Weighted-average costing

On purchase receipt:

`new average = ((old quantity × old average) + (received quantity × receipt unit cost)) / (old quantity + received quantity)`

Sale shipment credits inventory using the current average cost captured on the stock movement. Negative stock is prohibited by default.

## Suppliers and purchasing

### Suppliers

Id, SupplierNumber unique, Name, ContactName, Email, Phone, TaxNumber, PaymentTermsDays, Currency, IsActive, CreatedAt, UpdatedAt, RowVersion.

### SupplierAddresses

Id, SupplierId, Address fields, IsDefault.

### PurchaseOrders

Id, PurchaseOrderNumber unique, SupplierId, WarehouseId, Status, OrderDate, ExpectedDate nullable, Currency, Subtotal, DiscountTotal, TaxTotal, GrandTotal, Notes, CreatedByUserId, ApprovedByUserId nullable, ApprovedAt nullable, RowVersion.

Statuses: Draft, Submitted, Approved, PartiallyReceived, Received, Cancelled.

### PurchaseOrderLines

Id, PurchaseOrderId, ProductVariantId, DescriptionSnapshot, OrderedQuantity, ReceivedQuantity, UnitCost, DiscountAmount, TaxRateSnapshot, TaxAmount, LineTotal.

### GoodsReceipts

Id, ReceiptNumber unique, PurchaseOrderId, WarehouseId, ReceivedAt, ReceivedByUserId, SupplierDocumentNumber nullable, Notes, PostedAt nullable, RowVersion.

### GoodsReceiptLines

Id, GoodsReceiptId, PurchaseOrderLineId, ProductVariantId, Quantity, UnitCost, BatchReference nullable.

Posting a goods receipt creates stock movements and updates weighted-average inventory balances in one database transaction.

### SupplierInvoices

Id, SupplierInvoiceNumber, SupplierId, PurchaseOrderId nullable, InvoiceDate, DueDate, Currency, Subtotal, TaxTotal, GrandTotal, OutstandingAmount, Status, PostedAt nullable, RowVersion.

## Cart, checkout, and sales

### ShoppingCarts

Id, SessionId nullable, CustomerId nullable, Currency, CreatedAt, UpdatedAt, ExpiresAt, RowVersion.

### ShoppingCartLines

Id, CartId, ProductVariantId, Quantity, UnitPriceSnapshot, AddedAt. Unique CartId and ProductVariantId.

Server recalculates price, tax, availability, and totals during checkout. Browser totals are never trusted.

### SalesOrders

Id, OrderNumber unique, CustomerId, Status, PaymentStatus, FulfilmentStatus, Currency, BillingAddressJson, ShippingAddressJson, Subtotal, DiscountTotal, TaxTotal, ShippingTotal, GrandTotal, CustomerNotes, InternalNotes, PlacedAt, CancelledAt nullable, RowVersion.

Order statuses: Pending, Confirmed, Processing, Completed, Cancelled, Refunded.

Payment statuses: Pending, Authorized, PartiallyPaid, Paid, PartiallyRefunded, Refunded, Failed.

Fulfilment statuses: Unfulfilled, PartiallyFulfilled, Fulfilled, Returned.

### SalesOrderLines

Id, SalesOrderId, ProductId, ProductVariantId, SKU snapshot, Name snapshot, Quantity, UnitPrice, UnitCostSnapshot, DiscountAmount, TaxRateSnapshot, TaxAmount, LineTotal.

### OrderStatusHistory

Id, SalesOrderId, PreviousStatus, NewStatus, Reason, ChangedByUserId nullable, ChangedAt.

### Shipments

Id, ShipmentNumber unique, SalesOrderId, WarehouseId, Carrier, TrackingNumber, Status, ShippedAt nullable, DeliveredAt nullable, RowVersion.

### ShipmentLines

Id, ShipmentId, SalesOrderLineId, Quantity.

Posting a shipment releases reservations, reduces stock, creates inventory and cost-of-goods journal entries, and updates order fulfilment status in one transaction.

### CustomerReturns

Id, ReturnNumber unique, SalesOrderId, Status, Reason, RequestedAt, ReceivedAt nullable, CompletedAt nullable, RowVersion.

### CustomerReturnLines

Id, CustomerReturnId, SalesOrderLineId, Quantity, Condition, Restock, RefundAmount.

## Payments and invoicing

### PaymentMethods

Id, Code unique, Name, Type, IsActive, ConfigurationReference nullable.

Types: CashOnDelivery, BankTransfer, CardGateway.

### PaymentIntents

Id, SalesOrderId, PaymentMethodId, Provider, ProviderReference nullable, IdempotencyKey unique, Amount, Currency, Status, ExpiresAt nullable, CreatedAt, UpdatedAt, RowVersion.

Card details remain with the selected PCI-compliant provider. Only safe provider references and statuses are stored.

### Payments

Id, PaymentNumber unique, CustomerId, PaymentIntentId nullable, Method, ProviderReference nullable, Amount, Currency, ReceivedAt, Status, RecordedByUserId nullable, RowVersion.

### SalesInvoices

Id, InvoiceNumber unique, SalesOrderId, CustomerId, InvoiceDate, DueDate, Currency, Subtotal, DiscountTotal, TaxTotal, GrandTotal, OutstandingAmount, Status, PostedAt nullable, RowVersion.

### SalesInvoiceLines

Id, SalesInvoiceId, SalesOrderLineId nullable, Description, Quantity, UnitPrice, DiscountAmount, TaxRate, TaxAmount, LineTotal, RevenueAccountId.

### PaymentAllocations

Id, PaymentId, SalesInvoiceId, Amount, AllocatedAt, CreatedByUserId nullable.

Unique business validation prevents allocated amounts exceeding either payment remainder or invoice outstanding amount.

### CreditNotes

Id, CreditNoteNumber unique, SalesInvoiceId, Reason, Subtotal, TaxTotal, GrandTotal, Status, PostedAt nullable, RowVersion.

## Double-entry accounting

### FiscalPeriods

Id, Name, StartDate, EndDate, Status. Statuses: Open, Closed, Locked.

### Accounts

Id, Code unique, Name, AccountType, ParentAccountId nullable, IsControlAccount, IsActive, NormalBalance, RowVersion.

Account types: Asset, Liability, Equity, Revenue, Expense.

### JournalEntries

Id, JournalNumber unique, FiscalPeriodId, EntryDate, SourceType, SourceId, Description, Status, PostedByUserId, PostedAt, ReversalOfJournalEntryId nullable, RowVersion.

Statuses: Draft, Posted, Reversed.

### JournalLines

Id, JournalEntryId, AccountId, Description, Debit, Credit, CustomerId nullable, SupplierId nullable, WarehouseId nullable.

Database and application validation require total debits to equal total credits before posting.

### AccountingSettings

One active configuration mapping AccountsReceivable, AccountsPayable, Cash, Bank, Inventory, CostOfGoodsSold, SalesRevenue, SalesTaxPayable, PurchaseTaxReceivable, Discounts, ShippingRevenue, and RetainedEarnings accounts.

### Posting rules

Sales invoice:
- Debit Accounts Receivable
- Credit Sales Revenue
- Credit Sales Tax Payable

Customer payment:
- Debit Cash or Bank
- Credit Accounts Receivable

Purchase receipt with supplier invoice:
- Debit Inventory
- Debit Purchase Tax Receivable when applicable
- Credit Accounts Payable

Shipment:
- Debit Cost of Goods Sold
- Credit Inventory

Credit notes and reversals create new opposite journal entries. Posted journals are immutable.

## Try-on and privacy

Existing GuestSessions, PhotoAssets, TryOnJobs, TryOnResults, ProviderRequestEvents, and retention rules remain isolated from customer commercial records. Customer accounts do not automatically gain access to old guest photos.

## Reporting and exports

### SavedReports

Id, Name, ReportType, OwnerUserId, FilterJson, IsShared, CreatedAt, UpdatedAt.

### ExportJobs

Id, RequestedByUserId, ExportType, FilterJson, State, StorageKey nullable, RequestedAt, CompletedAt nullable, ExpiresAt, ErrorCode nullable.

Large exports run asynchronously. Small filtered lists can stream CSV or XLSX. Export files are private, authorized, expiring assets.

## Dashboard aggregates

Dashboard endpoints return date-range-filtered data:

- Revenue, order count, average order value, outstanding receivables
- Sales line chart by day/week/month
- Order-status donut chart
- Top products horizontal bar chart
- Stock value and low-stock count
- Purchases and supplier activity
- Inventory movement chart
- Customer growth and new-versus-returning split
- Try-on jobs by state, success rate, and top previewed products

Aggregates use SQL projections and indexes, not loading transaction tables into memory.

## Pagination and filtering

Every administrative list request supports:

- page default 1
- pageSize default 20, maximum 100
- search
- sortBy allowlist
- sortDirection asc or desc
- status/date/category/warehouse/supplier/customer filters as appropriate

Responses use `{ items, page, pageSize, total, totalPages }`.

## Number sequences

### DocumentSequences

DocumentType primary key, Prefix, NextNumber, Padding, RowVersion.

Order, invoice, payment, purchase order, receipt, shipment, return, credit note, and journal numbers are allocated atomically inside the posting transaction.

## Transaction boundaries

- Checkout: validate cart, calculate totals, create customer/order/lines/reservations/payment intent.
- Goods receipt posting: receipt lines, stock movements, balances, purchase order state, and journal entry.
- Shipment posting: shipment lines, reservations, stock movements, balances, order state, and cost journal.
- Invoice posting: invoice totals, receivable balance, and sales journal.
- Payment posting: payment, allocation, invoice balance, order payment state, and receipt journal.

Each boundary uses one SQL transaction and idempotency key where an external retry is possible.

## Delivery order

1. Shared entities, categories, customers, warehouses, tax, and document sequences.
2. Inventory, suppliers, purchase orders, and goods receipts.
3. Carts, checkout, orders, reservations, and shipments.
4. Invoices, payments, allocations, and credit notes.
5. Chart of accounts, journals, posting rules, and financial reports.
6. Admin CRUD pages, filters, pagination, export, and dashboard charts.
7. Security, audit, concurrency, integration tests, and browser journeys.
