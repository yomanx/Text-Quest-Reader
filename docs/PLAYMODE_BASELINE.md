# Play Mode Baseline — theme-effects-only

Branch: `theme-effects-only` at `05d30a7` (after compile baseline).
Prerequisite: see `docs/COMPILE_BASELINE.md` — **clean compile, no errors**.

## Why this doc exists

Visual Play Mode smoke test must be performed by a human with GUI access
to the Unity Editor. The assistant agent driving this branch has no GUI
session — it can only run Unity in `-batchmode -nographics` (used for the
compile baseline). Capture the evidence below and either attach it to this
file as a follow-up commit or paste screenshots into the PR.

## Pre-flight

1. Close any other Unity instances running on this project.
2. Make sure `git status` is clean (untracked Unity folders `My project/` and
   `Text-Quest-Reader/` aside).
3. Confirm current branch:
   ```
   git branch --show-current
   # expected: theme-effects-only
   ```

## Steps

1. Open the project in Unity Hub → 6000.4.7f1.
2. Open scene `Assets/_Scenes/MainScene.unity`.
3. Open the Console window (`Window → General → Console`). Clear it.
4. Press **Play**.
5. Observe and capture the items in the checklist below.
6. Press **Stop** within 30-40 seconds.

## Checklist

| # | What to verify | Expected | Capture |
|---|----------------|----------|---------|
| 1 | Catalog appears as the original UI (Sources Local/Remote toggle, list of three quests, preview picture on the right, description). | No fullscreen procedural shell. No PREMIUM card overlay on `SpaceStation_en`. Hero panel and ENTER QUEST button from the shell are **absent**. | `playmode-01-catalog.png` |
| 2 | Console window is mostly empty. Some `[PictureNode] Image not on disk…` / `[AudioManager] Asset missing…` log entries are acceptable (they're for the placeholder showcase quest `AsteroidStation_NeonDescent`). Anything with red icon (NullReferenceException, ArgumentNullException, "MissingReferenceException", "Type or namespace name could not be found") is **blocking**. | Zero red errors. Yellow warnings tolerated only if they're the known CS0618 noise from `Plugins/AssetUsageDetector`. | `playmode-02-console.png` |
| 3 | Click `Asteroid Station` (SpaceStation_en quest). | Description loads, preview image L1 / start image appears on the right side picture rect. | `playmode-03-spacestation-selected.png` |
| 4 | Press **Start Quest** (or arrow keys + Enter). | Fade-out → fade-in transition **inside the picture window only** (catalog text/buttons should NOT shake or flash). The first location (L1) text typewrites in the original text panel. Vignette darkens edges of the picture only. | `playmode-04-l1-intro.png` |
| 5 | Make a few choices that advance through L2 → L4 (command center / breach). | At L4 (with the gas-hiss sound trigger): shake + flash + alarm pulse — all **inside mainPictureRect**, not on the whole window. | `playmode-05-l4-alarm.png` |
| 6 | If feasible: a 20–40s screen recording of the run (Cmd+Shift+5 on macOS → record selected area). | Demonstrates baseline reader path is in control. | `playmode-rec.mp4` (or .gif) |

## Where to put the captures

Save under `docs/playmode-evidence/` (folder gets created on first save).
Filenames as suggested. Then commit:

```
git add docs/playmode-evidence
git commit -m "test(playmode): capture baseline smoke evidence"
```

If something is broken, instead of committing screenshots, paste them into a
GitHub issue / PR comment so the assistant can iterate.

## Issue classification

| Severity | Examples | Action |
|----------|----------|--------|
| 🔴 Blocking | NullRef on Start, Console flooded with errors, scene fails to load, GamePanel.Instance null. | Revert to `backup/cinematic-upgrade-pre-theme-20260520-0434`. Report. |
| 🟡 Non-blocking | "Image not on disk" or "Asset missing" log lines for AsteroidStation_NeonDescent quest. Procedural background visible. CS0618 noise. | Note but ignore for this pass. |
| 🟢 Pass | Original UI visible, SpaceStation_en plays, cinematic FX scoped to picture window, vanilla console. | Continue with theme tweaks. |

## Rollback (if blocking issue found)

```
git switch backup/cinematic-upgrade-pre-theme-20260520-0434
```
