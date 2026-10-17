# YouCam provider integration

WristMatch integrates Perfect Corp **YouCam API** for:

1. **AI Watch Virtual Try-On** — `POST/GET /s2s/v2.0/task/2d-vto/watch`
2. **AI Skin Analysis** — `POST/GET /s2s/v2.0/task/skin-analysis` (Look Match)

Credentials stay server-side. The browser never sees `YouCam:ApiKey`.

## Configuration

| Key | Purpose |
|---|---|
| `YouCam:ApiKey` | Bearer token from [YouCam API console](https://yce.makeupar.com/api-console/en/api-keys/) |
| `YouCam:BaseUrl` | Default `https://yce-api-01.makeupar.com` |
| `YouCam:PollIntervalMs` | Task poll interval (default 2500) |
| `YouCam:PollTimeoutSeconds` | Max wait per task (default 120) |
| `TryOn:ProviderMode` | `youcam` (live) or `demo` (local blur) |

Set the key with user-secrets (recommended):

```bash
dotnet user-secrets set "YouCam:ApiKey" "YOUR_KEY" --project WristMatch.Api
```

Or environment variable: `YouCam__ApiKey`.

When the key is empty, the API registers `DemoTryOnProvider` even if `TryOn:ProviderMode` is `youcam`. Studio and Look Match show a demo banner.

## Watch VTO flow

1. Guest session + private wrist photo upload (`POST /api/v1/photos`)
2. `POST /api/v1/tryon-jobs` enqueues a job
3. `TryOnWorker` calls `YouCamTryOnProvider`:
   - File API upload for wrist + product images
   - Create watch VTO task
   - Poll until success
   - Download result into `private-media/tryon-results/`
4. Client polls job status and streams `GET /api/v1/tryon-jobs/{id}/result`

## Skin Look Match flow

1. Face selfie + consent → `POST /api/v1/look-match`
2. Photo stored privately, then uploaded to YouCam File API
3. Skin Analysis (SD actions: `wrinkle`, `pore`, `texture`, `acne`)
4. Local undertone estimate (warm / cool / neutral) ranks catalogue metals and colours
5. UI offers “Try on” → Studio with product preselected

## Source files

- `WristMatch.Infrastructure/YouCam/YouCamClient.cs`
- `WristMatch.Infrastructure/YouCam/YouCamTryOnProvider.cs`
- `WristMatch.Infrastructure/YouCam/LookMatchService.cs`
- `WristMatch.Api/Controllers/LookMatchController.cs`
- `WristMatch.Api/Controllers/TryOnJobsController.cs`
