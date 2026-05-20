# Theme-Effects-Only — Demo Evidence Checklist

Branch: `theme-effects-only`
Backup: `backup/cinematic-upgrade-pre-theme-20260520-0434`

This checklist captures the **visual evidence** that reviewers need to sign off
on this branch. Code-only review is not sufficient — the value of this branch
is in screenshots and a short Play Mode recording.

## Required screenshots

Save under `docs/playmode-evidence/` with the exact filenames below.

| # | File | What it shows |
|---|------|---------------|
| 1 | `catalog.png` | Original catalog list. Selected quest has the cyan left-accent stripe. Premium quest shows the new compact pill badge (`PREMIUM` chip, dark-warm fill). No fullscreen shell. |
| 2 | `spacestation-intro.png` | After **Start Quest** on `SpaceStation_en`. Location 1 — quarters_warm preset visible **inside the picture rect only** (catalog/text/choices not tinted). Vignette darkens edges of the picture. First lines typewriting. Skip hint `▸ Space / Enter to skip` breathing bottom-right of the text. |
| 3 | `dark-section.png` | Reach Location 7 (dark zone, black_void preset). Heavy red-pulse vignette in the picture rect. Glitch effect cycling briefly. |
| 4 | `monster-confrontation.png` | Location 29 (monster). alarm_state preset with red beams + red embers, heart-beat pulse on the picture rect. HUD parameter that's near-critical visibly blinking red with outer halo. |
| 5 | `victory.png` | Location 20. victory_scene preset with brighter gradient, gold radial glow, light beams, sparkles. Result screen overlay appears 1.5 s after. |
| 6 | `failure.png` | Location 19 OR 35. black_void or failure preset. Dark red slow pulse, heavy vignette, fade-out. Result screen overlay. |

Naming is fixed — the release notes link to those exact filenames.

## Required recording

| File | What it captures |
|------|------------------|
| `playmode.mp4` (or `.gif`) | 25–40 s clip showing: catalog → click ENTER QUEST → fade transition (inside picture rect only) → typewriter typing with punctuation-aware pauses → skip via Space → choice card hover (glow pulses faster) → click a choice → next location → one HUD critical-state moment if you can reach it. |

Cmd+Shift+5 on macOS lets you record a selected region.

## Required artifact paths

- **Compile log**: `Logs/compile-theme.log` (regenerate with `Unity -batchmode -quit -nographics -logFile Logs/compile-theme.log`). Confirm `grep -c "error CS" Logs/compile-theme.log` returns `0`.
- **Build artifact**: `Builds/TextQuestReaderDemo/current/TextQuestReaderDemo.app` (on macOS). See `docs/COMPILE_BASELINE.md` for the build command.
- **Build log**: `Logs/build-demo.log` produced by `Assets/Editor/BuildTextQuestDemo.cs` via `-executeMethod BuildTextQuestDemo.BuildCurrentPlatform`.

## Required diff summary

Run and paste the output into the release notes:

```bash
git diff --stat f1f19f0..HEAD
git log --oneline --decorate -n 15
```

## Explicit checks before sign-off

- [ ] No new quest folders introduced.
- [ ] `AsteroidStation_NeonDescent/*` untouched on this branch.
- [ ] Shell remains disabled (`grep -RIn "RuntimeInitializeOnLoadMethod" Assets/_Scripts/_CinematicShell` returns only the comment line).
- [ ] `MainMenuTerminalSkin.Attach` is not called anywhere (`grep -RIn "MainMenuTerminalSkin\.Attach" Assets/_Scripts` returns only the comment in GamePanel.EnsureMainMenuSkin).
- [ ] `LegacyUiSuppressor` is only referenced inside the dormant CinematicShellBootstrap.
- [ ] All cinematic FX are scoped inside `mainPictureRect`, not full canvas (verified in `GamePanel.EnsureCinematicService` — uses `BootstrapScoped(mainPictureRect)`).
- [ ] Real images (`L1.jpg`…`L35.jpg` in `SpaceStation_en/Images/`) still display when present.
- [ ] Procedural backgrounds only show through where no PNG exists (PictureNode renders on top).
- [ ] AudioManager falls back to `ProceduralAudio.GetClip` when sound file is missing; Console stays free of error-level entries for any of the 29 new SFX keys.
- [ ] Quest counts unchanged: `locationCount = 35`, `passages = 80`, `parameters = 13`.

## Rollback path

If anything in the demo evidence is wrong / blocking:

```bash
# Full revert to pre-theme state:
git switch backup/cinematic-upgrade-pre-theme-20260520-0434

# Or undo a single commit on the branch:
git revert <commit-sha>

# Or restore only the quest data:
cp ../_backups/SpaceStation_en.quest.json.20260520-044539 \
   Assets/StreamingAssets/Quests/SpaceStation_en/quest.json
```
