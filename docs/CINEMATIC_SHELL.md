# Cinematic Shell — full sci-fi UI rebuild

This document describes the new `_CinematicShell/` module. Unlike the earlier
runtime layers (which decorated the existing GamePanel UI), this shell
completely **replaces** the visible interface with a fullscreen sci-fi
launcher / reader.

The existing `GamePanel` continues to run as the logical hub (quest data,
navigation, save/load). The legacy UI fields it references (mainPictureRect,
paramsRect, questionsContent, sourcesNode, blockerNode) are hidden via
`CanvasGroup.alpha=0`. The cinematic shell renders its own surfaces in their
place and routes user clicks back to GamePanel via `StartQuest`,
`SelectQuest`, `TriggerPassageById`, `TriggerNextSinglePassage`,
`AbandonQuest`.

## How it bootstraps

`CinematicShellBootstrap` is auto-attached via
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`.
No scene edits required. On Play:

1. Waits for `GamePanel.Instance` to appear.
2. Creates a root `CinematicShellRoot` GameObject under the host canvas.
3. Suppresses the legacy UI via `LegacyUiSuppressor` (reflection over
   GamePanel's serialized RectTransform/GameObject fields).
4. Raises `CinematicOverlays` (fade/flash/vignette/tint) ABOVE the shell so
   cinematic transitions still cover everything.
5. Builds three layers: `CinematicBackgroundLayer`, `CinematicCatalogScreen`,
   `CinematicReaderScreen`.
6. Hooks 8 GamePanel events.
7. Manually re-pumps the initial state (calls `UpdateLocalQuests` or
   `OnLocationShown` so we never miss the first event fired before our
   coroutine resumed).

## The three layers

### `CinematicBackgroundLayer`

Fullscreen procedural background — runs a `ProceduralSceneRenderer` that
crossfades between named presets (`deep_space`, `terminal_room`, `cryo_pod`,
`reactor_core`, `med_bay`, `comms_array`, `hidden_lab`, `server_room`,
`observation_deck`, `alarm_state`, `black_void`, `victory_scene`,
`secret_signal`, `failure_scene`, etc.). Plus a dark veil image that adjusts
strength depending on whether catalog or reader is active.

The background is **fullscreen**, not constrained to the legacy
`mainPictureRect`.

### `CinematicCatalogScreen`

The launcher. Layout:

```
┌─────────────────────────────────────────────────────────────┐
│ TEXT-QUEST READER · LIBRARY              OPERATOR · YOU    │
│ // LINK STABLE · SIGNAL OK · 2026 //     STATUS · NOMINAL  │
│                                                             │
│ ┌──── List ─────┐  ┌─────── Hero ──────────────────────┐  │
│ │ ▌Card A      │  │  ┌────────────────────────────┐   │  │
│ │  PREMIUM     │  │  │   Procedural Hero Art       │   │  │
│ │              │  │  │   (station, ring, stars)    │   │  │
│ │ ▌Card B      │  │  └────────────────────────────┘   │  │
│ │  FREE        │  │  ASTEROID STATION: NEON DESCENT   │  │
│ │              │  │  BY Showcase Studio  [EN]          │  │
│ │ ▌Card C      │  │                                    │  │
│ │  FEATURED    │  │  description text...              │  │
│ │              │  │                                    │  │
│ │              │  │       ┌──────────────────┐         │  │
│ │              │  │       │  ▶  ENTER QUEST  │         │  │
│ │              │  │       └──────────────────┘         │  │
│ └───────────────┘  └────────────────────────────────────┘  │
│ ← → SOURCE   ↑ ↓ SELECT   ENTER START   ESC EXIT          │
└─────────────────────────────────────────────────────────────┘
```

- **`CinematicQuestCard`** renders each card with mood accent bar, hover
  glow pulse, status badge (FREE/PREMIUM/OWNED/FEATURED).
- **Hero panel** uses `CinematicHeroArt.BuildPreviewFor(quest, w, h)` to
  produce a unique procedural Texture2D per quest:
  - AsteroidStation: deep magenta nebula, asteroid ring, station silhouette,
    purple-violet stars.
  - SpaceStation: deep blue, station silhouette, white starfield.
  - Generic: cyan grid + nebula + sparse stars.
- Big orange **ENTER QUEST** button with pulsing glow. For locked premium
  quests the label becomes `★ UNLOCK · $2.99`; clicking still routes through
  `GamePanel.StartQuest` → `PromptPremiumUnlock` → `UnlockModal`.

### `CinematicReaderScreen`

The in-quest UI. Layout:

```
┌─────────────────────────────────────────────────────────────┐
│ ┌── HUD STRIP ───────────────────────────────────────────┐ │
│ │ OXY  ████████░░░  SAN ███████░░  PWR ██░░░░  ...      │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌── Glass Text Panel ────────┐  ┌── Holo Monitor ──┐ ✕    │
│ │ // LOCATION 03 //          │  │                  │      │
│ │                            │  │  // SECTOR FEED  │      │
│ │ You wake. Frost on the     │  │  │ glow + scan   │      │
│ │ inside of the cryopod.     │  │  └───────────────┘      │
│ │ Red emergency light...     │                            │
│ │                            │                            │
│ └────────────────────────────┘                            │
│                                                             │
│ ┌── Choices Strip ──────────────────────────────────────┐ │
│ │ ▌Choice  ▌Danger Choice  ▌Reward Choice  ▌Story    │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

- **`CinematicHud`** (top bar) — gradient progress bars per parameter, big
  numerical value, red warning blink when near critical threshold (last 20%).
- **Glass text panel** — semi-transparent rounded panel with outlined border,
  inner glow gradient, scanlines, location code label. Typewriter renders
  inside.
- **Holo monitor** — rightmost glass panel with radial cyan glow + heavy
  scanlines (placeholder for future sector-feed art).
- **Choice strip** — horizontal scroll of large `CinematicChoiceCard`
  components. Each card has mood label, accent bar, pulse glow, mood icon.
  Pulse speeds up on hover.
- **Exit button** — top-right circle that calls `AbandonQuest`.

## Procedural hero-art system

`CinematicHeroArt.BuildPreviewFor(quest)`:

- Vertical gradient (style-dependent palette).
- Perlin-noise nebula tinted to style.
- 80–220 procedural stars with per-pixel size/brightness variation.
- Optional tilted ring (asteroid scene).
- Optional station silhouette (rectangles + antenna).
- Optional cyan/violet grid overlay.
- Optional scanline darkening every 2 rows.
- Radial vignette.

Each quest gets a distinct mood without any imported PNG.

## Files added

```
Assets/_Scripts/_CinematicShell/
├── CinematicShellBootstrap.cs       — entry, [RuntimeInitializeOnLoadMethod]
├── LegacyUiSuppressor.cs            — hides old GamePanel UI via reflection
├── CinematicBackgroundLayer.cs      — fullscreen procedural BG host
├── CinematicHeroArt.cs              — procedural hero-art generator
├── CinematicGlassPanel.cs           — reusable glass panel builder
├── CinematicCatalogScreen.cs        — premium catalog with hero + list
├── CinematicQuestCard.cs            — one quest card
├── CinematicReaderScreen.cs         — fullscreen reader
├── CinematicHud.cs                  — top parameter HUD strip
└── CinematicChoiceCard.cs           — large mood-tagged choice card
```

## Modified existing files

- `Assets/_Scripts/_View/GamePanel.cs` — added events
  (`MainTextRendered`, `ChoicesReady`, `SinglePassageReady`, `CatalogReady`,
  `QuestPreviewSelected`), public properties (`PassageResolver`,
  `LocationDescriptionResolver`, `ParameterService`, `HasActivePlayer`),
  helper methods (`TriggerPassageById`, `TriggerNextSinglePassage`).
- `MainMenuTerminalSkin` hidden by `LegacyUiSuppressor` to avoid double-header.

## What it looks like (intended)

Per the design intent, the shell delivers the 6 visual states the task
called for:

1. **Main menu / catalog** — terminal launcher with hero + cards.
2. **Featured quest selected** — AsteroidStation hero art, orange ENTER
   QUEST CTA, premium badge.
3. **Cryo pod opening** — `cryo_pod` BG preset (cold blue + red pulse +
   sparkles), glass text panel typewriting the wake-up.
4. **Reactor danger** — `reactor_core` BG preset (orange pulse + sparks +
   glow) combined with `<fx shake>` and `<fx flash>` from the existing
   cinematic-tag pipeline.
5. **Terminal / secret signal** — `hidden_lab` or `secret_signal` BG preset
   (violet grid + sparkles + beams + glitch).
6. **Victory / failure** — `victory_scene` or `failure_scene` BG preset
   combined with the existing `ResultScreen` overlay on top.

## Required visual self-review (only the user can do this)

I have no Unity Editor on this machine — the only way to verify the new
visuals is to:

1. Pull branch `cinematic-upgrade` and open the project in Unity 2022.3+.
2. Open scene `Assets/_Scenes/MainScene.unity`.
3. Press Play.
4. Take screenshots of:
   - Initial catalog view (should be unlike the original list).
   - `AsteroidStation_NeonDescent` selected (hero panel with procedural
     ring + station + nebula).
   - After clicking Unlock and Mock Purchase, then ENTER QUEST.
   - Inside cryo pod (location 1 of the showcase quest).
   - After picking the Reactor → Vent coolant (shake + flash + orange BG).
   - Hidden lab / decrypt log (violet shimmer scene).
   - Victory ending and Failure ending.

If any screen still looks like the original UI, capture the screenshot
and the Console output — I'll iterate.
