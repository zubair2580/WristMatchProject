# WristMatch

WristMatch is a watch discovery, **YouCam Skin Look Match**, and **YouCam Watch Virtual Try-On** commerce experience built for the [YouCam API Skin AI & eCommerce VTO Hackathon](https://youcam-api-skin-ai-ecommerce.devpost.com/).

## YouCam APIs

| Capability | YouCam API | Product surface |
|---|---|---|
| Watch on wrist | AI Watch Virtual Try-On (`/s2s/v2.0/task/2d-vto/watch`) | `/studio` |
| Skin → metal match | AI Skin Analysis (`/s2s/v2.0/task/skin-analysis`) | `/look-match` |
| Image ingest | File API (`/s2s/v2.0/file`) | Server-side only |

See [docs/provider-integration.md](docs/provider-integration.md) and the Devpost pack in [docs/HACKATHON.md](docs/HACKATHON.md).

## Stack

- ASP.NET Core 9 Web API + EF Core 9 + SQL Server
- React 19, TypeScript, Vite, TanStack Query
- Background try-on worker with idempotent jobs and private media

## Local setup

1. Confirm SQL Server is available and update `ConnectionStrings:SqlServer` if needed.
2. `dotnet restore` and `dotnet build WristMatch.sln`
3. `dotnet ef database update --project WristMatch.Infrastructure --startup-project WristMatch.Api`
4. Set your YouCam key (required for live VTO / Skin Analysis):

```bash
dotnet user-secrets set "YouCam:ApiKey" "YOUR_KEY" --project WristMatch.Api
```

5. `dotnet run --project WristMatch.Api`
6. `cd frontend && npm install && npm run dev`

Without `YouCam:ApiKey`, Studio and Look Match run in labelled **demo** mode.

`TryOn:ProviderMode` defaults to `youcam`. Live mode activates only when the API key is present.

## Configuration

| Setting | Notes |
|---|---|
| `YouCam:ApiKey` / `YouCam__ApiKey` | Perfect Corp bearer token — never commit |
| `YouCam:BaseUrl` | Default `https://yce-api-01.makeupar.com` |
| `TryOn:ProviderMode` | `youcam` or `demo` |
| `Cors:FrontendOrigin` | Default `http://localhost:5173` |
| `Admin:BootstrapEmail` / `Admin:BootstrapPassword` | First SuperAdmin via user-secrets |
| `VITE_API_URL` | Frontend API base (default `/api/v1` via proxy) |

## Shopper flows

- **Look Match** (`/look-match`) — face selfie → Skin Analysis + undertone → ranked watches → Try on
- **Studio** (`/studio`) — private wrist photo → YouCam Watch VTO → bag
- **Catalogue / compare / favourites / checkout / account** — full commerce path
- **Admin ERP** (`/admin`) — products, stock, orders, invoices, users

## Project layout

- `WristMatch.Domain` — entities
- `WristMatch.Application` — catalogue, try-on, look-match contracts
- `WristMatch.Infrastructure` — EF, YouCam client/providers, workers
- `WristMatch.Api` — HTTP API
- `WristMatch.Tests` — backend tests
- `frontend` — shopper + admin UI
- `docs` — provider + hackathon submission notes
