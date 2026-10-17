# YouCam API Skin AI & eCommerce VTO Hackathon — WristMatch

**Track:** eCommerce VTO (primary) + Skin AI Look Match  
**Deadline:** 2 Nov 2026 (confirm on [Devpost](https://youcam-api-skin-ai-ecommerce.devpost.com/))  
**Organizer contact for private repos:** contact_event@PerfectCorp.com

## What WristMatch is

WristMatch is a watch discovery and commerce experience. Shoppers get **metal and colour recommendations from YouCam Skin Analysis**, then **see the watch on their own wrist via YouCam AI Watch Virtual Try-On**, then checkout through a full bag → payment → order flow with admin ERP behind it.

This is not an API wrapper: try-on sits inside catalogue, shortlist, compare, favourites, inventory reservation, and order management.

## YouCam APIs used

| API | Endpoint | Where in product |
|---|---|---|
| AI Watch Virtual Try-On | `/s2s/v2.0/task/2d-vto/watch` | Studio (`/studio`) |
| File API | `/s2s/v2.0/file` | Server-side upload for wrist, SKU, selfie |
| AI Skin Analysis | `/s2s/v2.0/task/skin-analysis` | Look Match (`/look-match`) |

## Devpost project description (draft)

**Title:** WristMatch — Skin-matched watch shopping with YouCam Watch VTO

**Tagline:** Match metal to your skin, try the watch on your wrist, buy with confidence.

**Description:**

WristMatch solves a real eCommerce pain: watch shoppers cannot tell if a metal tone or case size will suit them until after purchase. We combine Perfect Corp YouCam **Skin Analysis** with **AI Watch Virtual Try-On** in one continuous flow.

1. Upload a face selfie on Look Match. YouCam Skin Analysis scores skin conditions while WristMatch estimates undertone and ranks catalogue metals and strap colours.
2. Open Studio with a recommended watch. Upload a private wrist photo (back of wrist, five fingers). YouCam Watch VTO composites the SKU onto the wrist.
3. Add to bag and checkout. Orders reserve warehouse stock and appear in the WristMatch Commerce ERP.

Privacy: guest session cookies, consent gates, EXIF stripping, private media storage, automatic expiry. API keys never leave the server.

**Built with:** ASP.NET Core 9, SQL Server, React + Vite, YouCam API (Watch VTO + Skin Analysis).

## Screenshot checklist

Capture these for Devpost:

1. Home / collection
2. Look Match selfie + undertone profile + ranked watches
3. Studio wrist upload + watch picker
4. YouCam Watch VTO result (label YOUCAM in badge)
5. Bag / checkout
6. Admin products (optional ERP depth)

## Demo video script (≤ 3 minutes)

1. **0:00–0:20** Problem: buying a watch online without seeing metal on your skin/wrist.  
2. **0:20–0:50** Look Match: selfie → consent → Skin Analysis / undertone → recommendations. Say “YouCam Skin Analysis API”.  
3. **0:50–1:50** Studio: wrist photo tips → Secure photo → Generate YouCam preview. Say “YouCam AI Watch Virtual Try-On”. Show result.  
4. **1:50–2:20** Add to bag → checkout path.  
5. **2:20–2:50** Architecture note: server-side File API, job queue, private results.  
6. **2:50–3:00** Closing: consumer value + APIs used.

Upload to YouTube (preferred), Vimeo, or Youku. No copyrighted music / third-party trademarks without permission.

## Setup for judges

```bash
# API
dotnet restore
dotnet user-secrets set "YouCam:ApiKey" "YOUR_KEY" --project WristMatch.Api
dotnet run --project WristMatch.Api

# Frontend
cd frontend && npm install && npm run dev
```

- Storefront: http://localhost:5173  
- Look Match: http://localhost:5173/look-match  
- Studio: http://localhost:5173/studio  
- Health: http://localhost:5000/api/v1/health/live (port may vary)

Without an API key the app runs in labelled **demo** mode (local preview + undertone ranking).

## Redeem YouCam units

1. Register on Devpost → receive redeem code by email  
2. Sign up at https://yce.makeupar.com/ai-api  
3. Account → Redeem Code → enter code (1,000 units)  
4. Create API key → set `YouCam__ApiKey`

## Submission checklist

- [x] Product integrates YouCam Watch VTO + Skin Analysis (code complete; needs your API key for live demos)
- [x] README with run instructions
- [ ] Set `YouCam__ApiKey` via user-secrets (redeem Devpost units first)
- [ ] Public git remote (or private shared with contact_event@PerfectCorp.com)
- [ ] This description pasted into Devpost
- [ ] Screenshots uploaded (see checklist above)
- [ ] Demo video link (≤ 3 min, shows YouCam usage on device)
- [ ] Agree to exit interview / blog if selected as winner

## Local run (after clone)

```bash
dotnet user-secrets set "YouCam:ApiKey" "YOUR_KEY" --project WristMatch.Api
dotnet user-secrets set "Admin:BootstrapEmail" "admin@wristmatch.local" --project WristMatch.Api
dotnet user-secrets set "Admin:BootstrapPassword" "Admin@WristMatch2026!" --project WristMatch.Api
dotnet run --project WristMatch.Api
# other terminal
cd frontend && npm install && npm run dev
```

Admin: `admin@wristmatch.local` / `Admin@WristMatch2026!`  
Storefront: http://localhost:5173 · API: http://localhost:5118
