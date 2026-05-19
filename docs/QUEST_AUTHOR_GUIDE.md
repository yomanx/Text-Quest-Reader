# Quest Author Guide

This guide is for people writing quests for Text-Quest-Reader. It covers
the legacy `quest.json` format, the legacy media tags (`<im>`, `<mu>`,
`<so>`), and the new optional cinematic tags introduced by the modernization
layer.

Everything new here is **optional**. Quests written for the original reader
still work unchanged.

---

## Top-level `quest.json` structure

```json
{
  "ownerUserId": 0,
  "id": 9001,
  "questName": "MyQuestFolderName",
  "displayName": "My Quest's Pretty Name",
  "description": "Catalog blurb shown when the quest is selected.",
  "startMusic": "intro_loop",
  "startImage": "preview",
  "locationCount": 18,
  "passageCount": 36,
  "order": 100,
  "lang": "en",
  "author": "Your Name",
  "parameters": [ ... ],
  "locations": [ ... ],
  "passages": [ ... ]
}
```

The `questName` must match the folder name (case-sensitive). The reader uses
it to resolve `Images/`, `Musics/`, `Sounds/` assets.

`startMusic`, `startImage` resolve from the quest's own subfolders. PNG/JPG
for images, OGG/MP3/WAV/AIF for audio.

---

## Parameters

Each parameter is a stat tracked through the quest. Examples: Oxygen, Gold,
Sanity, Reputation.

```json
{
  "workingName": "Oxygen",
  "paramType": "Failed",
  "index": 1,
  "value": 70,
  "startValue": 70,
  "minValue": 0,
  "maxValue": 100,
  "isActive": true,
  "isCriticMax": false,
  "criticText": "Your oxygen runs out. The lights fade.",
  "isHidden": false,
  "paramsRanges": [
    {"min": 0,  "max": 19,  "output": "<color=#FF6060>Oxygen: <></color>"},
    {"min": 20, "max": 49,  "output": "<color=#FFCC55>Oxygen: <></color>"},
    {"min": 50, "max": 100, "output": "<color=#A8E8FF>Oxygen: <></color>"}
  ]
}
```

| Field | Notes |
|-------|-------|
| `paramType` | `Usual` / `Successful` / `Failed`. `Failed` triggers a Fail ending when the boundary is crossed. `Successful` triggers a Victory ending. |
| `isCriticMax` | When `true`, the boundary that triggers the ending is the max (e.g. Signal = 10 → win). When `false`, the boundary is the min (e.g. Oxygen = 0 → die). |
| `paramsRanges[].output` | Text shown in the parameter panel. `<>` is replaced with the current value. Standard TMP rich text works. |
| `criticText` | Shown when the parameter ends the game. Can contain cinematic tags. |

The text in `output` and `criticText` can include cinematic tags — they fire
when the parameter is rendered or the game ends.

---

## Locations

Each location is a node in the quest graph.

```json
{
  "id": 1,
  "locationType": "Start",
  "descriptions": [
    "<im cryo_pod im><mu ambient_dread mu><fx fade in 1.2 fx>You wake up. The cryopod cracks open."
  ],
  "chooseWithFormula": false,
  "formula": "",
  "gridX": 0, "gridY": 0,
  "passability": 0,
  "influences": [
    {"influenceType": "Units", "value": 0, "formula": null}
  ],
  "paramsActions": ["Ignore"],
  "visitCounter": 0,
  "firstInPair": []
}
```

| Field | Notes |
|-------|-------|
| `locationType` | `Start` / `Neutral` / `Victory` / `Fail` / `Empty`. Each quest needs exactly one `Start`. `Victory` / `Fail` end the quest. |
| `descriptions` | Array. Multiple variants picked sequentially by visit count, OR by formula if `chooseWithFormula` is true. |
| `chooseWithFormula` + `formula` | When true, the result of the formula picks which `descriptions[i]` shows (`i = formula result - 1`). |
| `influences` | One entry **per parameter, in order**. Applied when location is entered. |
| `paramsActions` | One entry per parameter: `Ignore` / `Show` / `Hide`. Changes parameter visibility for the rest of the run. |

`Empty` locations chain into the next passage automatically — they're useful
for short "and then…" bridges.

---

## Passages (choices)

```json
{
  "from": 3, "to": 7, "same": 0, "id": 107,
  "question": "[story] Head straight for the Comms Array.",
  "description": "",
  "logicalCondition": "p3 >= 15",
  "priority": 1.0,
  "displayOrder": 0,
  "alwaysShow": true,
  "ignoreDemonstration": false,
  "necessaryRanges": [...],
  "takenValues": [...],
  "multipleValues": [...],
  "influences": [...],
  "paramsActions": [...]
}
```

| Field | Notes |
|-------|-------|
| `from` / `to` | Location IDs. |
| `question` | Button label. Can contain mood markers (see below). |
| `description` | Optional bridge text shown between the previous location and the destination. Can contain cinematic tags. |
| `logicalCondition` | A formula evaluated against current parameters (e.g. `p3 >= 15`). If `false`, the passage is hidden — unless `alwaysShow` is true, in which case it appears greyed-out (Locked mood). |
| `priority` | 0..1 — used for probability-gated branches. |
| `displayOrder` | Sort order within the location's choices. |
| `necessaryRanges` / `takenValues` / `multipleValues` | Legacy condition mechanisms — see the original docs. |

### Mood markers (new)

You can prefix the question with a bracket marker to colour the choice card:

| Marker | Meaning |
|--------|---------|
| `[danger]` or `[risk]` | Red accent — dangerous choice. |
| `[reward]` or `[loot]` | Gold accent — gain something. |
| `[story]` or `[lore]` | Purple accent — narrative path. |
| `[locked]` | Grey accent — explicit lock (the reader auto-greys disabled choices anyway). |

The marker is stripped before rendering. If no marker is present, the card
infers `Locked` from `alwaysShow` + `!conditions` state, otherwise it's
`Normal`.

---

## Formulas

Formulas are evaluated by `FormulaEvaluator` (`Assets/_Scripts/_QuestFormula/`).
Operators: `+ - * /`, `&& || !`, `== != < > <= >=`, ternary `? :`, parentheses.
Function: `rnd(min, max)`.

Reference parameters by their index: `p1`, `p2`, `p3`…

Examples:

```
p1 < 30                 — oxygen below 30
p3 >= 25 && p5 >= 4     — power at least 25 AND signal at least 4
rnd(1, 3) == 1          — random 1-in-3 chance
```

---

## Legacy media tags

These have been in the reader since day one and still work.

| Tag | Effect |
|-----|--------|
| `<im imagename im>` | Crossfade to image `Images/imagename.png` or `.jpg`. **If the file is missing, the reader auto-falls back to a procedural background with the same name** (see below). |
| `<mu musicname mu>` | Crossfade music to `Musics/musicname.ogg`/`mp3`/`wav`. |
| `<so soundname so>` | Play one-shot SFX `Sounds/soundname.ogg`/`wav`. Missing files fall back to procedural beeps for a known list of names (alarm, click, beep, tada, door_hiss, console_hum, heart_monitor, radio_static, paper_rustle, med_inject, explosion_small, soft_static). |

## Procedural backgrounds — `<bg name bg>` (new)

A scene background can be picked entirely procedurally — no image asset
required. Use `<bg name bg>` in any description text, or let `<im name im>`
fall through to the procedural fallback when no PNG exists.

| Preset name | Mood |
|-------------|------|
| `deep_space` | Drifting stars, cool blue gradient, light vignette |
| `cryo_pod` | Cold blue with red emergency pulse, frost sparkles |
| `neon_corridor` | Red emergency, scanlines, light beams |
| `terminal_room` | Cyan grid + radial glow + sparkles (default main-menu look) |
| `reactor_core` | Orange pulse, sparks, glow |
| `med_bay` | Clinical green, soft pulse |
| `comms_array` | White-noise stars + heavy scanlines + light beams |
| `storage_dim` | Warm dim, low scanlines |
| `hidden_lab` | Purple grid + violet glow + sparkles |
| `server_room` | Cool grid + scanlines + cyan pulse |
| `cargo_hold` | Industrial warm orange glow |
| `observation_deck` | Deep starfield + magenta pulsing asteroid |
| `quarters_warm` | Warm dim, soft slow pulse |
| `alarm_state` | Red alarm pulse + light beams + scanlines |
| `black_void` | Pure black with faint red pulse — death moments |
| `victory_scene` | Bright starfield + gold radial glow + light beams |
| `secret_signal` | Purple/violet starfield + sparkles + beams |
| `failure_scene` | Dark red, slow pulse, heavy vignette |

Alias names also resolve to the right preset:

| Quest-friendly name | Resolves to |
|---------------------|-------------|
| `preview`, `main_hub` | `terminal_room` |
| `cryo_pod` | `cryo_pod` |
| `corridor` | `neon_corridor` |
| `bridge` | `deep_space` |
| `medbay` | `med_bay` |
| `reactor`, `engine_bay` | `reactor_core` |
| `comms` | `comms_array` |
| `storage` | `storage_dim` |
| `hidden_lab` | `hidden_lab` |
| `server_room` | `server_room` |
| `cargo` | `cargo_hold` |
| `observation` | `observation_deck` |
| `quarters` | `quarters_warm` |
| `airlock` | `alarm_state` |
| `signal_sent` | `victory_scene` |
| `suffocation` | `black_void` |
| `truth` | `secret_signal` |

Unknown names fall back to `deep_space`. The system is alias-driven, so
existing quests that use `<im observation im>` get the `observation_deck`
look for free, even with no asteroid PNG on disk.

To force a procedural background regardless of image presence, use the
explicit tag:

```
<bg reactor_core bg>The reactor screams.
```

`<bg>` always wins over `<im>` if both are present.

You can put any number of these anywhere in `descriptions` or `description`.
Only the last value for each tag wins (so `<im A im>...<im B im>` ends up
showing image `B`).

Tags are stripped before the text is rendered.

---

## Cinematic tags (new, optional)

All cinematic tags follow the form `<fx ARGUMENTS fx>` (or a dedicated
tag-name shortcut). They are stripped before render, dispatched to
`CinematicEffectsService`, and ignored if the user has disabled screen
effects in Settings.

### `<fx fade in DUR fx>` / `<fx fade out DUR fx>`

Fades the screen to black or back. `DUR` in seconds, default `0.6`.

```
<fx fade in 1.2 fx>You wake. Cold light.
```

### `<fx shake DUR INTENSITY fx>` or `<shake DUR INTENSITY shake>`

Shakes the UI canvas. `DUR` in seconds (default 0.35), `INTENSITY` in pixels
(default 18). Respects the "Screen Shake" toggle in Settings.

```
The hull lurches.<fx shake 0.6 24 fx>You stumble.
```

### `<fx flash HEXCOLOR DUR PEAK fx>` or `<flash ... flash>`

Full-screen flash. `HEXCOLOR` like `FFFFFF` or `FFEEAA` (no `#`). `PEAK` is
the maximum alpha 0..1 (default 0.7).

```
<fx flash FFEEAA 0.5 0.6 fx>The reactor pulse blinds you.
```

### `<fx vignette HEXCOLOR DUR vignette>` or `<vignette ... vignette>`

Fades a vignette toward the given color/alpha. Use the alpha channel of the
color to control intensity (e.g. `000000AA` for strong dark).

```
<vignette 000020A0 0.8 vignette>The corridor darkens.
```

`<vignette off 0.5 vignette>` clears it.

### `<fx tint HEXCOLOR DUR fx>` or `<tint ... tint>`

Solid colored overlay (lighter than vignette). Useful for "red alert" or
"poisoned vision".

```
<tint FF202040 0.5 tint>Danger!
```

`<tint off 0.3 tint>` clears it.

### `<fx pulse HEXCOLOR DUR PULSES fx>` or `<pulse ... pulse>`

Heart-beat-style pulsing tint. `PULSES` is the count (default 3), `DUR` is
total time across all pulses (default 1.2).

```
<pulse FF1010CC 1.6 4 pulse>Alarms scream.
```

### `<fx glitch DUR INTENSITY fx>` or `<glitch ... glitch>`

Short jittery distortion. Use for hallucination / interference.

```
<glitch 0.6 14 glitch>The voices win for a second.
```

### `<fx overlay NAME on/off fx>` or `<overlay NAME on/off overlay>`

Spawn or remove an atmospheric overlay. Persists across location changes
until you turn it off (or `<fx stop fx>` clears everything).

| Name | Effect |
|------|--------|
| `fog` | Drifting fog layers. |
| `rain` | Falling vertical rain streaks. |
| `snow` | Drifting snowflakes. |
| `sparks` | Rising sparks. |
| `dust` | Slow dust motes. |
| `stars` | Twinkling stars. |
| `shimmer` / `magic` | Magic shimmer. |
| `blood` / `danger` | Red pulsing veil. |
| `scanlines` | CRT-style horizontal lines. |

```
<overlay rain on overlay>...
later: <overlay rain off overlay>
```

### `<fx stop fx>`

Clears ALL active overlays + tints + flashes + shake. Useful as the first
tag in a scene that should feel like a hard reset.

---

## A worked example

```json
{
  "id": 12,
  "locationType": "Neutral",
  "descriptions": [
    "<im observation im><mu ambient_deep mu><fx overlay stars on fx><fx vignette 000020A0 0.7 vignette>The observation deck. The asteroid pulses in slow magenta below.\n\nIt should not pulse. You watch anyway."
  ],
  ...
}
```

Combined effect:
- Image swap to `Images/observation.png` with crossfade.
- Music change to `Musics/ambient_deep.ogg` with crossfade.
- Spawn a twinkling stars overlay (persists until cleared).
- Fade vignette to dark blue.
- Typewrite the text.

---

## Tips

- Don't fire too many overlays at once. They are cheap but additive.
- Use `<fx stop fx>` at the start of a new arc to clear lingering effects
  from earlier choices.
- `<fx shake>` and `<fx glitch>` are visible only when their respective
  Settings toggles are on — assume the user might have turned them off for
  motion-sensitivity reasons. Build the scene so the tag *enhances* but is
  never load-bearing.
- Test your quest with **all cinematic settings off** as well as on. Text
  alone should still tell the story.
