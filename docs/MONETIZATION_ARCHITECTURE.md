# Monetization Architecture

This document describes the money layer added by the modernization. It is
opt-in: with no `monetization.json` present, every quest is treated as free
and the layer is invisible.

The architecture is deliberately decoupled from any specific store so the
same code runs against a mock in editor, Unity IAP on mobile, or a Steamworks
bridge on PC.

---

## Components

### `MonetizationService` (singleton)

`Assets/_Scripts/_Monetization/MonetizationService.cs`

The façade. UI code talks only to this. It:

- Loads `Assets/StreamingAssets/monetization.json` (or treats the catalog as
  empty if absent).
- Holds the active `IPurchaseProvider` (defaults to `MockPurchaseProvider`).
- Holds the `EntitlementService`.
- Exposes:
  - `IsQuestUnlocked(string questName) → bool`
  - `GetAccessState(string questName) → QuestAccessState`
  - `FindProductForQuest(string questName) → ProductDefinition`
  - `BeginPurchase(ProductDefinition, callback)`
  - `RestorePurchases(callback)`
  - `DebugUnlockAll()` / `DebugLockAll()` — handy for QA from the editor.
- Fires events:
  - `PurchaseCompleted(PurchaseResult)`
  - `ProductsChanged`

Bootstrapped automatically by `GamePanel.Awake()` (creates a new GameObject
with the component if no instance exists).

### `EntitlementService`

`Assets/_Scripts/_Monetization/EntitlementService.cs`

Stores ownership in PlayerPrefs under the key prefix `tqr_owned_<productId>`.
Single source of truth for "does the player currently own X".

Methods:

- `OwnsProduct(productId)` / `OwnsQuest(questName)`
- `IsPremiumQuest(questName)` / `IsFeaturedQuest(questName)`
- `GetAccessState(questName)` → `FreeOpen` / `OwnedPremium` / `LockedPremium`
- `GrantProduct(productId)` / `RevokeProduct(productId)`
- `ResetAllForDebug()`

It does **not** call out to any store; it is purely the local cache. The
`IPurchaseProvider` is responsible for telling it what to grant.

### `IPurchaseProvider`

`Assets/_Scripts/_Monetization/IPurchaseProvider.cs`

Interface for swap-in store integration. Methods:

- `Initialize(config, onReady)`
- `Purchase(product, callback)`
- `RestorePurchases(callback)`
- `IsAvailable` / `ProviderId`

### `MockPurchaseProvider`

`Assets/_Scripts/_Monetization/MockPurchaseProvider.cs`

The default provider. Always available, simulates a 0.6 s delay, always
returns `PurchaseStatus.Success`. Useful for editor / demo builds.

### `QuestCatalogService`

`Assets/_Scripts/_Monetization/QuestCatalogService.cs`

Combines a list of `QuestShort` with `MonetizationService` data to produce
`CatalogEntry` records carrying:

- `QuestShort QuestShort`
- `QuestSourceKind Source` — `Local` / `Remote` / `PremiumLocked`
- `QuestAccessState AccessState`
- `bool IsFeatured`
- `ProductDefinition Product`

Used by `GamePanel.ShowQuestShortList` to drive the modern catalog UI.

### `PremiumLockOverlay`

`Assets/_Scripts/_Monetization/PremiumLockOverlay.cs`

Runtime-built UI overlay attached to a `QuestCell`. Draws:

- A dimming veil (when `LockedPremium`).
- A corner badge (`PREMIUM` / `OWNED` / `FEATURED`).
- An orange "Unlock · $price" CTA button (when locked + product known).

The button calls back into `GamePanel.OnUnlockButtonClicked` which spawns
the `UnlockModal`.

### `UnlockModal`

`Assets/_Scripts/_Monetization/UnlockModal.cs`

Code-built confirm dialog. Spawned via `UnlockModal.Show(canvas, product,
callback)`. On confirm, calls `MonetizationService.BeginPurchase` and routes
the result back through the callback.

---

## Data model

### `MonetizationConfig`

Loaded from `Assets/StreamingAssets/monetization.json`. Schema:

```json
{
  "products": [
    {
      "id": "pack.neon_descent",
      "displayName": "Neon Descent Quest Pack",
      "description": "Unlocks the cinematic showcase quest...",
      "price": "$2.99",
      "kind": "QuestPack",
      "grantedQuestNames": ["AsteroidStation_NeonDescent"],
      "grantedThemeIds": [],
      "isMockOnly": true,
      "previewImage": "preview"
    }
  ],
  "premiumQuestNames": ["AsteroidStation_NeonDescent"],
  "featuredQuestNames": ["AsteroidStation_NeonDescent", "SpaceStation_en"]
}
```

`ProductKind` enum (see `ProductDefinition.cs`):

| Kind | Purpose |
|------|---------|
| `QuestPack` | Bundle of quests unlocked together. |
| `SingleQuest` | One quest. |
| `CosmeticTheme` | UI theme — granted theme IDs listed in `grantedThemeIds`. |
| `HintBundle` | Pack of optional hints. |
| `CheckpointToken` | Consumable rewind. |
| `SupporterPass` | One-time pass granting current + future packs. |
| `SubscriptionMonth` | Recurring (not implemented in mock; provider must handle). |

`premiumQuestNames` — quests in this list are locked unless a product
grants them.

`featuredQuestNames` — quests shown with a cyan FEATURED badge at the top
of the catalog.

---

## Flows

### Catalog rendering

```
GamePanel.ShowQuestShortList
    → QuestCatalogService.BuildEntries(list, source)
        → for each quest:
            access = MonetizationService.GetAccessState(name)
            featured = MonetizationService.IsFeaturedQuest(name)
            product = MonetizationService.FindProductForQuest(name)
        → sort: featured-first, then by Order, then by Name
    → for each entry:
        Instantiate QuestCell
        if locked or featured:
            attach PremiumLockOverlay
            overlay.Setup(...) draws veil/badge/CTA
```

### Purchase flow (mock)

```
User clicks Unlock CTA on locked card
    → PremiumLockOverlay.OnCtaClicked → callback to GamePanel
    → GamePanel spawns UnlockModal
User clicks "Mock Purchase"
    → UnlockModal calls back with confirmed=true
    → MonetizationService.BeginPurchase(product, ...)
        → MockPurchaseProvider.Purchase
            → waits 0.6 s, returns Success
        → EntitlementService.GrantProduct(product.id)
        → fires PurchaseCompleted + ProductsChanged
    → GamePanel re-renders the catalog
        → quest now shows OWNED badge, no veil, no CTA
User clicks the quest's Start button
    → Quest loads normally
```

### Start blocked by lock

```
User clicks Start on a locked card
    → GamePanel.StartQuest
    → IsQuestAccessAllowed → false
    → PromptPremiumUnlock → UnlockModal flow
```

---

## Swapping in a real store

Replace the mock provider at any time (typically in a bootstrap script):

```csharp
MonetizationService.Instance.SetPurchaseProvider(new UnityIAPPurchaseProvider());
```

Your `UnityIAPPurchaseProvider` would:

1. In `Initialize(config, onReady)` — call `UnityPurchasing.Initialize(...)`
   with a `ProductCatalog` built from `config.products`. When the Unity IAP
   `OnInitialized` callback fires, call `onReady(true)`.
2. In `Purchase(product, callback)` — call `controller.InitiatePurchase(productId)`.
   In the `ProcessPurchase` callback, call back with `PurchaseResult.Ok(productId)`
   and let `MonetizationService` handle entitlement.
3. In `RestorePurchases(callback)` — call the platform's restore API, then
   for each restored product invoke `MonetizationService.Instance.Entitlement.GrantProduct(id)`.

For Steam, swap to a `SteamworksPurchaseProvider` that talks to the
Steam Microtransactions API. The interface is the same.

The `MonetizationService` itself does not change.

---

## Recommended monetization models

The architecture supports several models. Honest, non-pay-to-win patterns:

1. **Quest packs** — group thematic quests into a `QuestPack` product
   (`pack.neon_descent`, `pack.dark_fantasy`, etc.). One purchase unlocks
   ~3-5 quests. **Current implementation default.**
2. **Single-quest unlocks** — `SingleQuest` products at lower price, useful
   for one-shots or specials.
3. **Supporter pass** — `SupporterPass` granting all current and future
   premium content. Best aimed at fans who already played the free arc.
4. **Cosmetic themes** — `CosmeticTheme` products unlocking UI themes.
   Pure aesthetic, no game effect — safe to monetize aggressively.
5. **Hints / checkpoints** — `HintBundle` / `CheckpointToken` consumables
   sold in packs. Optional. Should never gate completion — only convenience.
6. **Subscription library** — `SubscriptionMonth` — only viable on
   platforms that handle recurring billing (mobile, Steam). The current
   `MonetizationService` doesn't expire entitlements, so a subscription
   implementation must layer expiry checks on top.

**Strongly recommended not to ship:**
- Gating a critical narrative reveal behind a hint pack.
- Random "quest crate" lootboxes.
- Time-limited unlocks for narrative content.
- Pay-to-win on parameter outcomes.

---

## Debug helpers

From any C# context with `using TextQuestReader.Monetization;`:

```csharp
MonetizationService.Instance.DebugUnlockAll();   // grant every product
MonetizationService.Instance.DebugLockAll();     // revoke every product
MonetizationService.Instance.Entitlement.OwnedProductIds; // list owned
```

Or wipe PlayerPrefs entirely:

```csharp
PlayerPrefs.DeleteAll();
```

---

## File map

```
Assets/_Scripts/_Monetization/
├── EntitlementService.cs
├── IPurchaseProvider.cs
├── MockPurchaseProvider.cs
├── MonetizationService.cs
├── PremiumLockOverlay.cs
├── ProductDefinition.cs        — ProductDefinition + MonetizationConfig
├── PurchaseResult.cs           — PurchaseResult + QuestAccessState enums
├── QuestCatalogService.cs      — CatalogEntry, QuestSourceKind
└── UnlockModal.cs

Assets/StreamingAssets/
└── monetization.json           — Catalog config (optional)
```
