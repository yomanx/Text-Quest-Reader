# Text-Quest-Reader — Cinematic Modernization

This document describes the cinematic upgrade layered on top of the original
Text-Quest-Reader. Everything in this layer is **opt-in and additive**: old
quests (including `SomeTestQuest` and `SpaceStation_en`) still load exactly
as before. Any new features activate automatically when a quest uses the new
optional tags or when the user enables the related toggle in Settings.

---

## What was added

### Procedural graphics layer (no external assets required)
- **Generated runtime textures** for every visual primitive — vertical
  gradients, radial glows, scanlines, grids, beams, vignettes, noise, rounded
  rectangles, corner brackets, glowing dots. All built by
  `ProceduralTextureFactory` in `_Cinematic/Procedural/`. No PNG/JPG bundled
  for the layer.
- **`ProceduralBackgroundPresets`** — 18 named scene presets
  (`cryo_pod`, `neon_corridor`, `terminal_room`, `reactor_core`, `med_bay`,
  `comms_array`, `storage_dim`, `hidden_lab`, `server_room`, `cargo_hold`,
  `observation_deck`, `quarters_warm`, `alarm_state`, `black_void`,
  `deep_space`, `victory_scene`, `secret_signal`, `failure_scene`). Quests
  reference these by name via the new `<bg name bg>` tag, OR through the
  existing `<im name im>` tag — when the PNG isn't found, the matching
  preset is resolved automatically.
- **`ProceduralSceneRenderer`** — mounts inside any RectTransform and
  crossfades between presets. Used behind the picture rect, behind
  ResultScreen, and behind UnlockModal.
- **`MainMenuTerminalSkin`** — sci-fi terminal frame for the entire main
  menu: corner brackets, animated header label, faint grid, scanlines.
  All code-built, sits at sibling-index 0 under the canvas.
- **`ChoiceVisualState` glow** — pulsing fringe around hover/active choice
  cards, color-keyed to mood (danger/reward/story/normal). Code-built
  rounded-rect sprite, no shaders.
- **HUD-style parameter cells** — `ParameterAnimator` adds a subtle
  rounded frame behind every parameter row; when a parameter is near its
  critical threshold the frame turns red and blinks. Pure code, no assets.
- **`ProceduralAudio`** — synthesizes short procedural AudioClips at
  runtime (alarm, click, beep, hiss, sweep, explosion, tada, etc.). Used
  as a fallback when a quest references an SFX name that's not on disk,
  so silence isn't dead silence — alarms still beep, ticks still tick.

### Cinematic core
- **Typewriter improvements** (`Assets/_Scripts/_View/AliveText.cs`)
  - Skip-to-end on `Enter`, `Space`, or left mouse click.
  - User-configurable text speed (`GameSettings.TextSpeed`, x0.25–x4).
  - Optional typewriter SFX (toggle in Settings).
- **Fade transitions** between quest start / end / abandon
  (`CinematicEffectsService.FadeOut/FadeIn`, called from `GamePanel.StartQuest`
  and `StartLocalQuest`).
- **Screen shake / flash / vignette / pulse / glitch / tint** — runtime
  overlays drawn on the existing canvas. No new prefabs needed.
- **Choice cards** — vanilla `QuestionCell` is decorated at runtime by
  `ChoiceVisualState`. Moods inferred from text markers (`[danger]`,
  `[reward]`, `[story]`, `[locked]`), color-coded accent bar + icon.
- **Animated parameter panel** — `ParameterAnimator` is attached to every
  parameter cell on spawn. It pulses on value change, pulses harder near
  critical thresholds, and fades in on first appearance.

### Audio
- **Master / Music / SFX volume sliders** (`GameSettings.MasterVolume`,
  `MusicVolume`, `SfxVolume`) — `AudioManager.ApplyVolumeSettings()` is wired
  to `GameSettings.AudioVolumesChanged`.
- **One-shot SFX with volume scaling** — `AudioManager.PlaySfxClip(clip, volume)`.

### Visual FX layer + cinematic tags
- New optional in-text tags (backward-compatible, ignored if absent):
  - `<fx shake DUR INTENSITY fx>` — camera shake.
  - `<fx flash HEXCOLOR DUR PEAK fx>` — full-screen flash.
  - `<fx vignette HEXCOLOR DUR vignette>` — dark vignette.
  - `<fx tint HEXCOLOR DUR fx>` / `<fx tint off DUR fx>` — colored tint.
  - `<fx pulse HEXCOLOR DUR PULSES fx>` — heartbeat tint.
  - `<fx glitch DUR INTENSITY fx>` — UI shake + jitter.
  - `<fx overlay NAME on/off fx>` — atmospheric overlays: `fog`, `rain`,
    `snow`, `sparks`, `dust`, `stars`, `shimmer`, `blood/danger`, `scanlines`.
  - `<fx fade in/out DUR fx>` — full-screen fade.
  - `<fx stop fx>` — clear all effects.
- Existing media tags (`<im X im>`, `<mu X mu>`, `<so X so>`) still work.

### Monetization architecture
- New classes under `Assets/_Scripts/_Monetization/`:
  - `MonetizationService` (façade, singleton).
  - `EntitlementService` (PlayerPrefs-persisted ownership).
  - `QuestCatalogService` (combines local + remote + premium status).
  - `ProductDefinition`, `MonetizationConfig` (JSON model).
  - `IPurchaseProvider`, `MockPurchaseProvider` (replace later with Unity IAP / Steam).
  - `PremiumLockOverlay` (visual lock on each premium QuestCell).
  - `UnlockModal` (code-built purchase confirmation modal).
- Config file: `Assets/StreamingAssets/monetization.json` — if absent, every
  quest is treated as free.
- A locked quest still appears in the catalog with PREMIUM badge + Unlock
  button; clicking it spawns the mock purchase flow.

### Modern UI
- **Result screen** — cinematic VICTORY / FAILURE overlay spawns 1.5 s after
  a quest reaches `Victory` or `Fail`, listing final parameter values.
- **Settings panel extension** — sliders for Master/Music/SFX volume, text
  speed, plus toggles for Screen Effects / Screen Shake / Scene Fade /
  Typewriter SFX. Runtime-attached to the existing prefab in `GamePanel.ActionSettings()`.

### Showcase quest
- `Assets/StreamingAssets/Quests/AsteroidStation_NeonDescent/` —
  18 locations, 36 passages, 5 parameters (Oxygen, Sanity, Power, Trust,
  Signal), three endings (signal-sent victory, suffocation fail, secret
  truth-revealed victory). Heavy use of `<fx ...>` tags. Premium-locked
  via `monetization.json`.

---

## How to run

The project is a Unity project (Unity 2022+, see `ProjectSettings/`). To run:

1. Open the folder `Text-Quest-Reader/` in **Unity Hub**.
2. Open scene `Assets/_Scenes/MainScene.unity`.
3. Press Play.

The free quests (`SomeTestQuest`, `SpaceStation_en`) load directly.
`AsteroidStation_NeonDescent` appears in the catalog with a PREMIUM badge —
click the orange "Unlock · $2.99" button on the card to run the mock purchase
and then start the quest.

To wipe mock ownership during testing, call from the Unity console:

```csharp
TextQuestReader.Monetization.MonetizationService.Instance.DebugLockAll();
```

To unlock everything for QA:

```csharp
TextQuestReader.Monetization.MonetizationService.Instance.DebugUnlockAll();
```

---

## Adding placeholder assets to the showcase quest

The showcase quest references image / music / sfx files that are not bundled
in the repo (to keep it small and license-clean). Drop your own files into:

- `Assets/StreamingAssets/Quests/AsteroidStation_NeonDescent/Images/`
- `Assets/StreamingAssets/Quests/AsteroidStation_NeonDescent/Musics/`
- `Assets/StreamingAssets/Quests/AsteroidStation_NeonDescent/Sounds/`

See `ASSETS_README.md` inside that folder for the expected filenames. The
quest plays through even without these files — missing assets log a warning
and skip the audio/image; cinematic FX still fire.

---

## Adding new cinematic tags to your own quest

Open any `quest.json` location or passage. Inside the `descriptions` or
`description` field, add tags as plain text:

```
"You step into the reactor room.<fx shake 0.4 12 fx><fx overlay sparks on fx><so reactor_alarm so>The core groans."
```

You can chain tags. Order doesn't matter. They are stripped from the text
before rendering and dispatched to `CinematicEffectsService`. See
`QUEST_AUTHOR_GUIDE.md` for a full reference.

---

## Adding a new premium quest

1. Drop the quest folder into `Assets/StreamingAssets/Quests/MyNewQuest/`
   with a `quest.json` inside.
2. Edit `Assets/StreamingAssets/monetization.json`:
   - Add the quest name to `premiumQuestNames`.
   - Add a `ProductDefinition` with `grantedQuestNames: ["MyNewQuest"]`.
3. Play. The quest now shows up as PREMIUM in the catalog with the price
   you set.

To make a quest appear "Featured" (cyan badge at top of catalog), add its
name to `featuredQuestNames` in the same JSON.

---

## Recommended next-step Unity Editor tweaks

The cinematic layer works out of the box, but the visual polish is best
combined with a few small Editor adjustments. None are required.

- **Reference a TMP font asset** to `ResultScreen`, `UnlockModal`, and
  `ModernSettingsExtension` if you want non-default fonts (the runtime code
  uses TMP's default).
- **Drag-assign the `mainPictureRect` or canvas RectTransform** to
  `CinematicEffectsService.shakeRoot` in the inspector. The bootstrap
  currently uses the canvas as the shake root, which is fine but you may
  want a tighter region.
- **Replace the runtime-built color palette** in `ChoiceVisualState`,
  `PremiumLockOverlay`, and `ResultScreen` with your brand colors if you
  want to customize the look.

---

## Known limitations

- Cinematic shake is applied to the canvas RectTransform. On scenes where
  the canvas is a child of an overlay, you may want to point `shakeRoot`
  somewhere more specific.
- The `MockPurchaseProvider` simulates a 0.6 s delay and always succeeds —
  for production, swap in `UnityIAPPurchaseProvider` (not included in this
  layer; the interface is `IPurchaseProvider`).
- No analytics layer is wired (the monetization events surface as
  `MonetizationService.PurchaseCompleted` — easy to forward).

---

## File map (new files)

```
Assets/_Scripts/_Settings/
├── GameSettings.cs                — PlayerPrefs façade
└── SettingsKeys.cs                — String constants

Assets/_Scripts/_Cinematic/
├── ChoiceVisualState.cs           — Choice card moods + hover glow
├── CinematicEffectsService.cs     — Central FX coordinator
├── CinematicTagParser.cs          — Parses <fx ...> tags
├── FogLayerMover.cs               — Fog overlay layer
├── OverlayFactory.cs              — Atmospheric overlay generators
├── ParameterAnimator.cs           — Parameter cell pulse + HUD frame
├── ParticleField.cs               — UI particle field (rain/snow/sparks/stars)
├── ProceduralAudio.cs             — Generated AudioClips (alarm/beep/hiss/etc.)
├── PulsingTint.cs                 — Pulsing tint helper
└── Procedural/
    ├── BackgroundLayers.cs        — Gradient/Stars/Grid/Glow/Beams/Sparkle/Scanlines layers
    ├── BackgroundPreset.cs        — Preset struct
    ├── ProceduralBackgroundPresets.cs — 18 named scene presets + aliases
    ├── ProceduralSceneRenderer.cs — Crossfade applicator
    └── ProceduralTextureFactory.cs — Runtime Texture2D / Sprite builders

Assets/_Scripts/_Monetization/
├── EntitlementService.cs          — Ownership store
├── IPurchaseProvider.cs           — Provider interface
├── MockPurchaseProvider.cs        — Editor / local mock
├── MonetizationService.cs         — Top-level façade
├── PremiumLockOverlay.cs          — Catalog lock badge
├── ProductDefinition.cs           — Product + config models
├── PurchaseResult.cs              — Result + access state enums
├── QuestCatalogService.cs         — Combined catalog
└── UnlockModal.cs                 — Code-built purchase modal

Assets/_Scripts/_View/
├── MainMenuTerminalSkin.cs        — Sci-fi terminal frame for main canvas
├── ModernSettingsExtension.cs     — Volume + speed + FX toggles
└── ResultScreen.cs                — Cinematic VICTORY / FAILURE overlay

Assets/StreamingAssets/
├── monetization.json              — Sample monetization config
└── Quests/AsteroidStation_NeonDescent/
    ├── quest.json                 — 18 locations, 36 passages, 5 params
    └── ASSETS_README.md           — Placeholder asset list

docs/
├── README_MODERNIZATION.md        — This file
├── QUEST_AUTHOR_GUIDE.md          — Cinematic-tag reference
└── MONETIZATION_ARCHITECTURE.md   — Money-layer reference
```

Modified existing files:
- `Assets/_Scripts/_View/AliveText.cs` — typewriter skip + speed + SFX.
- `Assets/_Scripts/_Core/TextParser.cs` — `ExtractAllTagValues()`.
- `Assets/_Scripts/_Core/AudioManager.cs` — volume settings hook.
- `Assets/_Scripts/_Core/ParameterService.cs` — animator hook.
- `Assets/_Scripts/_View/GamePanel.cs` — service bootstrap, event hooks,
  cinematic-tag dispatch, premium gating, fade transitions, skip-to-end.
