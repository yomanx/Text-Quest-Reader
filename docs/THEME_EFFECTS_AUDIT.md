# Theme-Effects-Only Audit

Branch: `theme-effects-only` (forked from `cinematic-upgrade` at `f1f19f0`).
Backup: `backup/cinematic-upgrade-pre-theme-20260520-0434`.
Upstream: `origin/main` (= `albruevich/Text-Quest-Reader`). No separate
`upstream` remote configured.

## Diff summary vs `origin/main`

```
102 files changed, 10336 insertions(+), 20 deletions(-)
```

That's a lot of additive surface area. This branch must **not** add more.
The theme-only pass is reversible micro-tweaks to existing files, nothing
else.

### Files that changed the most (top 10 by lines added)

| Lines | File |
|------:|------|
| 1082 | `Assets/StreamingAssets/Quests/AsteroidStation_NeonDescent/quest.json` |
| 560 | `Assets/_Scripts/_CinematicShell/CinematicCatalogScreen.cs` |
| 489 | `Assets/_Scripts/_Cinematic/CinematicEffectsService.cs` |
| 485 | `Assets/_Scripts/_CinematicShell/CinematicReaderScreen.cs` |
| 461 | `Assets/_Scripts/_Cinematic/Procedural/BackgroundLayers.cs` |
| 396 | `docs/QUEST_AUTHOR_GUIDE.md` |
| 317 | `Assets/_Scripts/_Cinematic/Procedural/ProceduralTextureFactory.cs` |
| 312 | `Assets/_Scripts/_Cinematic/Procedural/ProceduralBackgroundPresets.cs` |
| 296 | `docs/MONETIZATION_ARCHITECTURE.md` |
| 294 | `Assets/_Scripts/_View/GamePanel.cs` (M, big delta) |

### Files **modified** (M, not A) — only 6 in the source

- `Assets/_Scripts/_View/GamePanel.cs` — central hub; integration points
  for cinematic FX, monetization, procedural bg.
- `Assets/_Scripts/_View/AliveText.cs` — typewriter skip/speed.
- `Assets/_Scripts/_View/PictureNode.cs` — warning level downgrade only.
- `Assets/_Scripts/_Core/AudioManager.cs` — volume settings + procedural
  audio fallback.
- `Assets/_Scripts/_Core/ParameterService.cs` — parameter animator hook.
- `Assets/_Scripts/_Core/TextParser.cs` — `ExtractAllTagValues` helper.
- `Assets/StreamingAssets/Quests/SpaceStation_en/quest.json` — cinematic
  tag pass (29 of 35 locations).

---

## What NOT to touch on `theme-effects-only`

### 🔴 INVASIVE — keep frozen

These were either user-rejected paths or layer-replacement code that the
user explicitly said felt worse than the source. Do not call from any new
code. Do not auto-attach. Do not extend.

- `Assets/_Scripts/_CinematicShell/CinematicShellBootstrap.cs` — auto-init
  disabled; keep that way.
- `Assets/_Scripts/_CinematicShell/LegacyUiSuppressor.cs` — hides original
  GamePanel UI fields via reflection; must remain dormant.
- `Assets/_Scripts/_CinematicShell/CinematicCatalogScreen.cs`
- `Assets/_Scripts/_CinematicShell/CinematicReaderScreen.cs`
- `Assets/_Scripts/_CinematicShell/CinematicChoiceCard.cs`
- `Assets/_Scripts/_CinematicShell/CinematicQuestCard.cs`
- `Assets/_Scripts/_CinematicShell/CinematicHud.cs`
- `Assets/_Scripts/_CinematicShell/CinematicGlassPanel.cs`
- `Assets/_Scripts/_CinematicShell/CinematicHeroArt.cs`
- `Assets/_Scripts/_CinematicShell/CinematicBackgroundLayer.cs`
- `Assets/_Scripts/_View/MainMenuTerminalSkin.cs` — entire file is
  dormant. `Attach()` method exists, but never called. Do not call.

### 🟡 SENSITIVE — do not rewrite, ok to read

Existing GamePanel-driven UI hub. Theme pass may add small comment-labelled
tweaks; no layout rewrite, no new components, no new events.

- `Assets/_Scripts/_View/GamePanel.cs` — only allowed edit is comment
  or theme-related guard. No new event, no new method that adds runtime
  UI. The `EnsureMainMenuSkin` method body should stay neutered.

### 🟢 SAFE — theme tweaks allowed

These are pure presentation knobs. Color tuning, easing tweaks, opacity
adjustments and intensity values are acceptable; structural rewrites are
not.

- `Assets/_Scripts/_Cinematic/CinematicEffectsService.cs` — tweak effect
  intensities / durations / colors. Do not change the scoping (already
  bound to `mainPictureRect` via `BootstrapScoped`).
- `Assets/_Scripts/_Cinematic/Procedural/ProceduralBackgroundPresets.cs`
  — adjust colors / pulse periods / star counts of existing presets.
  Do not add new preset names without a green-lit reason.
- `Assets/_Scripts/_Cinematic/OverlayFactory.cs` — overlay color palettes.
- `Assets/_Scripts/_Cinematic/ChoiceVisualState.cs` — mood colours.
- `Assets/_Scripts/_Cinematic/ParameterAnimator.cs` — HUD frame palette.
- `Assets/StreamingAssets/Quests/SpaceStation_en/quest.json` — refine the
  `<bg>`/`<fx>` tags on existing locations only. **Do not** rewrite
  descriptions, choices, parameters, passages.

### 🟢 ALSO SAFE — passive infrastructure

These are services the original UI depends on; the theme pass uses them
but does not modify their public API.

- `Assets/_Scripts/_Settings/GameSettings.cs`
- `Assets/_Scripts/_Settings/SettingsKeys.cs`
- `Assets/_Scripts/_Monetization/*` (services + UI)
- `Assets/_Scripts/_Cinematic/CinematicTagParser.cs`

---

## Hard rules for the theme pass

1. **No new quests.** `AsteroidStation_NeonDescent` stays as-is; no new
   showcase content; no rename. The single quest under work is
   `SpaceStation_en` (displayName "Asteroid Station").
2. **No shell re-enable.** Do not restore `[RuntimeInitializeOnLoadMethod]`
   on `CinematicShellBootstrap`. Do not call `MainMenuTerminalSkin.Attach`
   from anywhere. Do not instantiate `LegacyUiSuppressor`.
3. **No layout rewrite.** No new RectTransforms in `GamePanel`, no
   re-anchoring of `mainPictureRect`/`paramsRect`/`mainTextRect`/
   `questionsRect`, no new prefabs.
4. **No new C# classes** unless absolutely required for a theme tweak,
   and only with explicit user OK. Default: edit colours / numbers /
   ordering inside existing files.
5. **No new dependencies** in `Packages/manifest.json`.

---

## Reset / rollback

If anything goes wrong on `theme-effects-only`:

```
git switch backup/cinematic-upgrade-pre-theme-20260520-0434
```

This restores exact `f1f19f0` state. Or:

```
git switch cinematic-upgrade
```

to land on the working pre-theme branch.

---

_File generated by the theme-effects-only kickoff audit pass._
