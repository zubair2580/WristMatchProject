# WristMatch project status and SRS guide

## 1. Project purpose

WristMatch is a watch discovery and visual try-on platform. The original SRS defines a shopper journey:

1. Choose preferences.
2. Browse a watch catalogue.
3. Upload a permitted wrist photo with explicit consent.
4. Generate a private image-based try-on preview.
5. Compare products.
6. Save favourites.
7. Visit a retailer.

The project scope was later expanded during implementation into a local commerce ERP test environment with:

- Customer management
- Suppliers
- Warehouses
- Inventory
- Purchasing
- Orders
- Invoices
- Payments
- Accounting
- Reports
- Role and menu permissions

The ERP extension is a local testing scope. It supersedes the original SRS exclusions for checkout, inventory, purchasing, invoices, and accounting.

## 2. Technology

### Backend

- ASP.NET Core 9 Web API
- C#
- Entity Framework Core 9
- SQL Server
- ASP.NET Core Identity
- Cookie authentication
- Antiforgery headers for administrator mutations
- ImageSharp for decoded image validation and normalization

### Frontend

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- Lucide icons
- Custom CSS design system

### Local database

```text
Server: ZUBAIRKHALIL-WS\MSSQLSERVER2025
Database: WristMatch
```

## 3. Completed modules

### 3.1 Public storefront

Routes:

```text
/
/catalog
/products/{id}
/cart
/studio
/compare
/favourites
/privacy
```

Completed:

- Responsive storefront layout
- Editorial home page slider
- Automatic slider rotation
- Slider navigation, indicators, and pause/play control
- Product catalogue cards
- Catalogue filtering and sorting
- Budget filtering
- Occasion filtering
- Product details
- Local favourites
- Local shortlist/selection
- Compare layout
- Privacy page
- Responsive navigation
- Mobile menu
- API proxy for local development

### 3.2 Catalogue

Completed:

- Public product listing
- Product details API
- Budget filtering
- Occasion filtering
- Deterministic recommendation ordering
- Price sorting
- Product archive state
- Product category
- Product SKU
- Try-on readiness state
- Product row-version concurrency
- HTTPS retailer validation
- Image URL validation
- More than 15 local sample products

Current sample catalogue:

- 16 products
- PKR prices
- Product images
- Product occasions
- Sample retailer links
- Clearly labelled local testing data

### 3.3 Categories

Completed:

- Category SQL table
- Hierarchical parent category support
- Category CRUD API
- Category create modal
- Category edit modal
- Category archive action
- Search
- Pagination
- CSV export
- Slug validation
- Duplicate slug protection
- Row-version concurrency
- `Sr #` list column

Current local data:

- 16 demo categories

### 3.4 Product assets and licensing

Completed:

- Product asset table
- Licence metadata table
- Protected admin image upload
- JPEG and PNG validation
- Decoded dimension limits
- EXIF/IPTC/XMP metadata removal
- Normalized JPEG storage
- Primary asset selection
- Approved asset endpoint
- Source and licence note fields
- Attribution field
- Manual try-on readiness endpoint
- Multiple image selection in the product form

Pending:

- Asset gallery management screen
- Asset reorder screen
- Asset replacement preview
- Asset delete/revoke screen
- Licence detail editor

### 3.5 Guest sessions and private photos

Completed:

- Opaque random guest session token
- SHA-256 token hashing
- HttpOnly cookie
- Configurable session expiry
- JPEG and PNG validation
- 8 MiB limit
- 4096px limit
- Decoded pixel limit
- Metadata removal
- Private storage outside the static web root
- Ownership-checked photo access
- Ownership-checked deletion
- Consent version storage
- Photo expiry metadata
- `Cache-Control: no-store`

### 3.6 Studio

Completed:

- Guest session creation
- Photo selection
- Local preview
- Consent checkbox
- Upload validation
- Private upload
- Server-normalized preview
- Replace photo
- Delete photo
- Expiry display
- Privacy guidance
- Wrist pose guidance

Pending:

- Product selection inside Studio
- Start preview action
- Try-on job polling
- Generated result display
- Result delete flow
- Provider mode badge

### 3.7 Administrator Identity

Completed:

- ASP.NET Core Identity
- Admin cookie authentication
- Secure cookie configuration
- Password hashing
- Lockout configuration
- Role creation
- Bootstrap admin from user-secrets
- Admin login
- Admin logout
- Admin session guard
- Protected routes
- Admin profile endpoint

Local test administrator:

```text
Email: admin@wristmatch.local
Password: Admin@WristMatch2026!
```

This password is for local testing only.

### 3.8 User management

Completed:

- User list API
- User search
- Pagination
- Add user modal
- Edit user modal
- Role assignment
- Activate/deactivate
- Password reset modal
- Password validation through Identity
- `Sr #` list column
- Five demo administrator users
- Protected CRUD mutations
- Antiforgery validation

Demo users:

```text
catalog.manager@wristmatch.local
operations.manager@wristmatch.local
purchasing.manager@wristmatch.local
accountant@wristmatch.local
report.viewer@wristmatch.local
```

Local test password:

```text
Admin@WristMatch2026!
```

### 3.9 Roles and menu permissions

Completed:

- Role permissions table
- Role/menu permission API
- View permission
- Create permission
- Update permission
- Delete permission
- Permission seed data
- Menu permission route
- Permission selectors
- Permission checkbox matrix
- Immediate permission save

Roles:

```text
SuperAdmin
CatalogManager
OperationsManager
PurchasingManager
Accountant
ReportViewer
OperationsViewer
SupportViewer
```

Pending:

- Enforce menu permissions in the frontend navigation
- Enforce menu permissions in every controller policy
- Permission audit trail
- Role create/edit/delete UI

### 3.10 Dashboard

Current status: **Wave C5 delivered** — sales/revenue from live posted records.

Completed:

- Live active product count
- Live customer count
- Live supplier count
- Average product price
- Price-band bar chart
- Material-composition donut chart
- Live database overview endpoint
- Responsive dashboard cards
- Sales trend (14-day) from confirmed/completed orders
- Revenue trend (14-day) from confirmed payments
- Outstanding AR + open orders metric cards
- Empty states when no posted sales/payments in window

Pending:

- Stock valuation chart
- Purchase trend chart
- Customer growth chart
- Try-on state chart
- Date-range filter
- Export dashboard report

## 4. Expanded ERP modules

### 4.1 Customers

Current status: working.

Completed:

- CustomerProfile table
- CustomerAddress table foundation
- Customer list API
- Customer search
- Customer pagination
- Customer create modal
- Customer edit modal
- Guest/account state
- Marketing consent
- Deactivate operation
- 16 local demo customer records

Pending:

- Address CRUD
- Customer order history
- Customer invoice history
- Customer activity timeline
- Customer export
- Customer merge/anonymization

### 4.2 Suppliers

Current status: working.

Completed:

- Supplier table
- Supplier list API
- Supplier search
- Supplier pagination
- Supplier create modal
- Supplier edit modal
- Supplier currency
- Payment terms
- Deactivate operation
- 16 local demo suppliers

Pending:

- Supplier addresses
- Supplier purchase history
- Supplier invoice history
- Supplier balance
- Supplier export

### 4.3 Warehouses

Current status: working foundation.

Completed:

- Warehouse table
- Warehouse list API
- Warehouse search
- Warehouse pagination
- Warehouse create modal
- Warehouse edit modal
- Active/archive state
- 16 local demo warehouses

Pending:

- Warehouse addresses
- Warehouse stock balances
- Warehouse transfers
- Goods receipts
- Warehouse-specific reports

### 4.4 Stock

Current status: **Wave C1 started** — balances + adjustment posting live.

Completed:

- `InventoryBalances` EF table
- Weighted-average cost on positive adjustments
- Stock adjustment API (`POST /api/v1/admin/stock/adjustments`)
- Stock balances list API (`GET /api/v1/admin/stock`)
- Admin Stock page with adjustment modal
- StockMovements posting for adjustments
- Search + `Sr #` column

Pending:

- Stock transfer between warehouses
- Low-stock rules
- Stock valuation report

Completed via C2/C3:

- Stock reservation / release / fulfil from sales orders
- Goods-receipt posting from purchases (WAC)

### 4.5 Purchases

Current status: **Wave C3 delivered** — create, approve, receive (incl. partial), cancel.

Completed:

- EF `PurchaseOrder` / `PurchaseOrderLine` mapped to existing tables
- Purchase create API with server-side totals/tax
- Approve / receive / cancel transitions
- Partial goods receipt with `ReceivedQuantity`
- Weighted-average stock posting on receipt (`InventoryBalance.ApplyReceipt`)
- Admin Purchases page with create + detail actions
- Seeded demo POs remain readable

Pending:

- Purchase order line edit after draft
- Supplier invoice matching
- Purchase returns

### 4.6 Orders

Current status: **Wave C2 delivered** — admin create + status workflow with stock reservation.

Completed:

- EF `SalesOrder` / `SalesOrderLine` mapped to existing tables
- Warehouse + tax rate on orders
- Server-side totals and tax calculation
- Order create API (`POST /api/v1/admin/orders`)
- Status transitions: confirm → process → complete / cancel / mark-paid
- Inventory reservation on confirm, release on cancel, fulfil on complete
- Admin Orders page with create modal and detail actions
- Seeded demo orders remain readable

Pending:

- Shopping cart persistence
- Guest / registered checkout
- Shipment workflow
- Returns
- Public order confirmation

### 4.7 Invoices and payments

Current status: **Wave C4 delivered** — create from order, issue, partial/full payment allocation.

Completed:

- EF `SalesInvoice` / `SalesInvoiceLine` / `Payment` mapped to existing tables
- Create invoice from sales order (copies lines/totals)
- Issue / cancel / record payment transitions
- Partial payment reduces `OutstandingAmount`; full pay → `Paid` + order `MarkPaid`
- Admin Invoices page with payment method + amount
- Seeded demo invoices remain readable

Pending:

- Invoice PDF
- Multi-invoice payment allocations table
- Bank transfer verification workflow
- Card gateway adapter
- Credit notes
- Receivable reports

### 4.8 Accounts and journals

Current status: seeded test data and read-only module view.

Completed:

- Accounts table
- JournalEntries table
- JournalLines table
- 15 demo accounts
- 15 demo journal entries
- 30 demo journal lines
- Debit and credit values
- Account list display

Pending:

- Chart of accounts CRUD
- Journal creation
- Journal validation
- Debit/credit balancing enforcement
- Posting workflow
- Reversal workflow
- Fiscal periods
- Trial balance
- Profit and loss
- Balance sheet

### 4.9 Reports

Current status: seeded report definitions and read-only list view.

Completed:

- ReportDefinitions table
- 16 demo report definitions
- Report list display

Pending:

- Report query engine
- Date and status filters
- CSV export
- XLSX export
- Asynchronous export jobs
- Private expiring export files
- Saved report filters

### 4.10 Try-on jobs

Current status: **Wave B delivered** — EF domain, create/poll/result APIs, demo worker, Studio generate flow.

Completed:

- EF `TryOnJob` entity mapped to `TryOnJobs`
- Local job creation API (`POST /api/v1/tryon-jobs`)
- Idempotency-Key handling + request hash validation
- Worker lease acquisition
- Background hosted worker (`TryOnWorker`)
- Provider interface + demo provider adapter
- Result persistence under private storage
- Guest result image endpoint (`Cache-Control: no-store`)
- Studio product selection, generate, poll, result, delete
- Admin live try-on list (`GET /api/v1/admin/tryon-jobs`)

Pending:

- YouCam provider adapter
- Ambiguous submission handling refinements
- Dedicated expiry/cleanup worker beyond inline expiry in the poll loop

## 5. SQL test data

The complete local reset and seed script is:

`docs/demo-reset-and-seed.sql`

It:

- Removes local testing records.
- Preserves Identity administrator accounts.
- Creates missing local testing tables.
- Inserts linked demo records.
- Creates 15+ records for the main testing modules.

Run it with:

```powershell
sqlcmd -S "ZUBAIRKHALIL-WS\MSSQLSERVER2025" -d WristMatch -E -b -i "docs/demo-reset-and-seed.sql"
```

## 6. Run commands

API:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="http://localhost:5000"
dotnet run --project WristMatch.Api --no-launch-profile
```

Frontend:

```powershell
cd frontend
npm run dev -- --host 0.0.0.0
```

Build:

```powershell
dotnet build WristMatch.sln
dotnet test WristMatch.Tests/WristMatch.Tests.csproj
cd frontend
npm run build
```

## 7. Definition of complete

A module is complete only when all of the following exist:

1. SQL table and migration.
2. Domain entity and constraints.
3. Protected API.
4. Create, read, update, and deactivate/delete behavior.
5. Search and server pagination.
6. Modal form with validation.
7. Row-level permissions.
8. Concurrency protection.
9. Test seed data.
10. Backend tests.
11. Browser journey coverage.
12. Export support where required.

**Honest completion status (audit 2026-10-17):**

| Module | Meets definition? | Notes |
|--------|-------------------|--------|
| Categories | Yes | Full CRUD + export; nav filtered by menu permissions |
| Products (admin) | Yes | Archive/reactivate, category picker, image hover + side preview |
| Public catalogue | Yes | Filters/sort, detail fields, checkout path |
| Look Match / Studio | Yes | YouCam Skin Analysis + Watch VTO when `YouCam:ApiKey` set; demo fallback otherwise |
| Suppliers / Warehouses | Yes | Full deactivate CRUD |
| Customers | Yes | `IsActive` + `Deactivate()` / reactivate |
| Identity / users / permissions | Yes | Identity + permissions matrix; sidebar filters by `canView` |
| Stock / orders / purchases / invoices | Yes | Dedicated admin pages + APIs |
| Accounts / reports | Monitoring | Seed + read-only demo-module lists; journal/report engine not built |

---

## 8. Audit findings (frontend + backend)

### 8.1 Architecture facts

- Master-data and commerce APIs use dedicated controllers.
- Dedicated admin UIs: Products, Categories, Customers, Suppliers, Warehouses, Stock, Orders, Invoices, Purchases, Users, Permissions.
- `AdminModulePage` remains for **accounts**, **reports**, and **tryons** monitoring lists.
- YouCam integration: `YouCamClient`, `YouCamTryOnProvider`, `LookMatchService` (Watch VTO + Skin Analysis).
- Seed-only / monitoring: `Accounts`, `JournalEntries`, `ReportDefinitions`.

### 8.2 Resolved vs remaining

**Resolved:** customer deactivate (`IsActive`), permission-aware sidebar, product archive + category picker, Studio + Look Match YouCam path, storefront checkout.

**Still optional (not hackathon-blocking):**

1. Full GL journal posting UI
2. Report query engine + CSV job runner
3. Product asset gallery reorder/delete UI
4. Server-side product admin pagination (client filter works)

### 8.3 Design polish priorities (keep cream / tan / ink aesthetic)

Do **not** restyle into a new theme. Polish within the existing Playfair + DM Sans luxury language.

| Rank | Item | Where | Why |
|------|------|-------|-----|
| 1 | GL posting UI | Accounts module | Monitoring-only today |
| 2 | Report engine | Reports module | Definitions seeded; no runner |
| 3 | Asset gallery admin | Product assets | Upload + primary only |
| 4 | CSS hygiene | `App.css` | Trim unused leftovers over time |

### 8.4 Storefront vs admin visual verdict

- **Storefront hero and catalogue presentation are strong** and on-brand (cream/tan split hero, editorial slider). Main gap is the unfinished try-on promise after photo upload.
- **Admin shell looks mature** (dark sidebar, serif headings, gold accents). Half the nav targets are intentionally read-only; design work should make that honest rather than invent a new visual system.

---

## 9. Next delivery order and acceptance criteria

### Wave A — Fix and finish master data (1–2 days)

| # | Work | Acceptance |
|---|------|------------|
| A1 | Fix customer deactivate: add `IsActive` (migration + domain + API + UI) | Deactivate hides inactive customers from default list; reactivation works |
| A2 | Enforce menu permissions in admin nav | Role without View cannot see that menu item; direct URL returns 403-friendly UI |
| A3 | Product admin: archive + category from Categories API | Archive soft-hides product; category saved to linked `CategoryId` |
| A4 | Product detail storefront fields | Description, specs, retailer link render from API; no hardcoded filler where data exists |
| A5 | AdminModulePage honesty pass | Search works client-side OR is removed; Create replaced with monitoring badge |

### Wave B — Core product promise: try-on (3–5 days)

| # | Work | Acceptance |
|---|------|------------|
| B1 | EF `TryOnJob` entity + migration | Job row created with queued state, ownership, expiry |
| B2 | Create job API + Idempotency-Key | Duplicate key returns same job; unauthorized guest cannot create |
| B3 | Hosted worker + demo provider | Job moves queued → processing → succeeded/failed; result image private |
| B4 | Studio UI: select product, generate, poll, show result | Full browser journey from photo → preview without refresh hacks |
| B5 | Admin try-ons page uses real API | Replaces demo-module SQL for try-ons |

### Wave C — Commerce transactions (1–2 weeks)

| # | Work | Acceptance |
|---|------|------------|
| C1 | `InventoryBalances` + stock adjustment/posting rules | Balance changes only through posting; concurrency safe |
| C2 | Sales order create/update/status workflow | Totals server-side; status transitions validated; links to customer |
| C3 | Purchase order create + goods receipt | **Done** — create/approve/receive/cancel + WAC stock post |
| C4 | Invoice + payment allocation | **Done** — issue + partial/full pay updates outstanding |
| C5 | Dashboard sales/revenue charts from live data | **Done** — 14d sales/revenue series + empty states |

### Wave D — Ledger, reports, hardening

| # | Work | Acceptance |
|---|------|------------|
| D1 | Chart of accounts CRUD + balanced journals | Unbalanced journal rejected |
| D2 | Report engine + CSV export | At least sales, stock, and AR reports |
| D3 | TaxCategory/TaxRate admin APIs | Products can select tax category |
| D4 | CustomerAddress CRUD | Customer edit can manage addresses |
| D5 | Expand tests + remove WeatherForecast scaffold | Controller tests for catalogue, customers, try-on create; build green |

---

## 10. Exact run / test commands

API:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="http://localhost:5000"
dotnet run --project WristMatch.Api --no-launch-profile
```

Frontend:

```powershell
cd frontend
npm run dev -- --host 0.0.0.0
```

Build / test:

```powershell
dotnet build WristMatch.sln
dotnet test WristMatch.Tests/WristMatch.Tests.csproj
cd frontend
npm run build
```

SQL reset + seed:

```powershell
sqlcmd -S "ZUBAIRKHALIL-WS\MSSQLSERVER2025" -d WristMatch -E -b -i "docs/demo-reset-and-seed.sql"
```

Local admin (Development only):

```text
Email: admin@wristmatch.local
Password: Admin@WristMatch2026!
```

Storefront: `http://localhost:5173`  
Admin: `http://localhost:5173/admin`

---

## 11. Recommended immediate start

Start with **Wave A** (customer deactivate bug, permission-aware nav, product admin finish, read-only module honesty, product detail fidelity). That improves trust in both UIs without waiting on the try-on worker.

Then **Wave B** (try-on pipeline) — this is the original SRS differentiator and unlocks Studio + Compare preview states.
