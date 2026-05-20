# Theme-Effects-Only — Release Notes

Branch: `theme-effects-only`
Base: `cinematic-upgrade @ f1f19f0`
Backup: `backup/cinematic-upgrade-pre-theme-20260520-0434`
Build target: Unity 6000.4.7f1

---

## TL;DR

A reversible, **theme-only** polish pass on top of `cinematic-upgrade`. No
new quests. No shell reactivation. No layout rewrite. The branch adds:

1. Cinematic `<bg>` / `<fx>` tags themed for 29 of 35 locations in the
   original `SpaceStation_en` quest.
2. Tuned procedural fallback backgrounds for the 12 presets that quest
   actually reaches.
3. 29 procedural SFX mappings so every `<so X so>` key in the theme pass
   has an audible fallback when no real audio file ships.
4. Polished choice cards (hover glow, mood palette, clearer disabled state).
5. Polished HUD parameter cells (heart-beat critical pulse, outer halo,
   floating ±N delta label, text-contrast bump).
6. Polished catalog cards (cyan accent stripe on selection, lighter
   premium veil, pill-shaped badge with state-aware colour).
7. Improved typewriter pacing (punctuation-aware pauses scaled with text
   speed) + skip affordance (`▸ Space / Enter to skip` hint).
8. One-command headless demo build script.
9. Compile baseline + Play Mode checklist + demo evidence checklist.

The runtime-built `_CinematicShell/` module from the previous branch
remains compiled into the project but is **dormant**:
`[RuntimeInitializeOnLoadMethod]` is removed; `MainMenuTerminalSkin.Attach`
is no longer called from any source path. The original GamePanel-driven
UI is the canonical reader / catalog.

---

## What changed (diff summary)

```
17 files changed, 1459 insertions(+), 153 deletions(-)
```

| Lines | File | Type |
|------:|------|------|
| +303 / -5 | `_Cinematic/ProceduralAudio.cs` | New 29 SFX builders + helpers |
| +222 / -28 | `_Cinematic/ParameterAnimator.cs` | Heart-beat pulse + delta label |
| +199 / -71 | `_Cinematic/Procedural/ProceduralBackgroundPresets.cs` | 12 preset tunings |
| +141 / -10 | `_View/AliveText.cs` | Punctuation pauses + skip hint |
| +90 / -14 | `_Cinematic/ChoiceVisualState.cs` | Hover/glow + palette |
| +58 / -15 | `_Monetization/PremiumLockOverlay.cs` | Pill badge, lighter veil |
| +46 / -2 | `_View/QuestCell.cs` | Accent stripe, middle-dot |
| +29 / -3 | `_View/ModernSettingsExtension.cs` | Live `1.0x` value label |
| +21 / -3 | `_View/QuestionCell.cs` | Hover relay + cleaner disabled |
| +19 / -3 | `_CinematicShell/CinematicShellBootstrap.cs` | Hardened disable notice |
| +10 / -4 | `_View/GamePanel.cs` | Neutered `EnsureMainMenuSkin` body |
| +84 / 0  | `Assets/Editor/BuildTextQuestDemo.cs` (new) | Build script |
| +2 / -1  | `Quests/SpaceStation_en/quest.json` | Cinematic tags on 29 locations |
| docs/* | 6 new markdown files | Audit, checklists, baseline |

Full per-line: `git diff --stat f1f19f0..HEAD`.

## Commit graph (15 most recent on branch)

```
765d78a feat(audio): add procedural fallback sfx for SpaceStation
37fdbfe feat(procedural): tune SpaceStation background presets
5b93ed1 feat(reader): improve typewriter pacing and skip affordance
f2bfae4 feat(ui): refine catalog cards while preserving baseline layout
145e4b9 feat(ui): improve hud feedback and critical readability
c03d256 feat(ui): polish choice cards with clearer states
fac59ca test(spacestation): verify theme-only data diff
ec6e433 feat(spacestation): add theme tags to confrontation and comms finale
0296b2b feat(spacestation): add theme tags to puzzle leak and endings
a0672c3 feat(spacestation): add theme tags to support rooms and pickups
267c16c feat(spacestation): add theme tags to intro and monster arc part 1
1619820 chore(spacestation): snapshot quest before theme pass
f497aff test(compile): record clean compile baseline
05d30a7 chore(shell): keep cinematic shell disabled on theme branch
536acf7 docs(audit): record upstream diff and risky files
```

## What was NOT changed (explicit non-goals)

- **No new quests** — `AsteroidStation_NeonDescent/*` is byte-identical to
  the base branch. The only quest data file touched is
  `SpaceStation_en/quest.json`, and only its `descriptions[i]` strings
  (29 of 35 locations) — never `parameters`, `passages`, `locationCount`,
  `passageCount`, top-level metadata, or array lengths.
- **No shell re-enable** — `[RuntimeInitializeOnLoadMethod]` remains
  absent. `MainMenuTerminalSkin.Attach` is not called anywhere.
  `LegacyUiSuppressor` is only referenced inside the dormant
  `CinematicShellBootstrap`, which has no entry point.
- **No branching-logic edits** — `PassageResolver` / `FormulaEvaluator` /
  `Quest` / `Passage` / `Location` / `Player` / `SaveLoadManager` are not
  touched on this branch.
- **No layout / scene / prefab edits** — `MainScene.unity` is unmodified.
  Existing `[SerializeField]` references in `GamePanel`, `QuestCell`,
  `QuestionCell`, `SettingsPanel`, `PictureNode` are untouched.
- **No new C# classes outside the build script** — `BuildTextQuestDemo.cs`
  is the only new `.cs` file added on the branch. All polish edits are
  in-file.
- **No new packages** — `Packages/manifest.json` is unchanged.

## Known limitations (honest list)

1. **No Unity Editor self-review available to the agent.** All visuals
   were tuned with no in-Editor frame-by-frame inspection. The
   `docs/THEME_EFFECTS_DEMO_CHECKLIST.md` enumerates the screenshots a
   reviewer needs to capture before merging.
2. **`passageCount` declared 81 in JSON, actual array 80.** Pre-existing
   off-by-one inherited from upstream `albruevich/main`. Not touched.
3. **`<im>` tags still drive PictureNode** — when a real PNG exists in
   `Quests/SpaceStation_en/Images/`, it overrides the procedural
   background. Procedural shows through only when the file is missing.
   This is intentional.
4. **Procedural SFX are synthesised at runtime.** They are short and
   readable but obviously not real foley. Real audio files in
   `Quests/SpaceStation_en/Sounds/` take priority when present.
5. **TMP outline width is tweaked on parameter cells.** If the project's
   TMP material doesn't expose outline shader keywords, the `try/catch`
   block silently no-ops — text reads at default contrast.
6. **`Builds/` and `Logs/` are in `.gitignore`** — the build artifact
   and log are not committed. Re-run the build script if a reviewer
   needs them.

## Compile baseline

Last clean compile log: `Logs/compile-theme.log` (regenerate as documented
in `docs/COMPILE_BASELINE.md`).

```
errors    : 0
warnings  : 111 (all pre-existing CS0618 deprecation noise + 6 minor CS0219)
```

See `docs/COMPILE_BASELINE.md` for the full breakdown.

## Build artifact

Run:

```bash
mkdir -p Logs
/Applications/Unity/Hub/Editor/6000.4.7f1/Unity.app/Contents/MacOS/Unity \
  -projectPath "$PWD" -batchmode -quit -nographics \
  -executeMethod BuildTextQuestDemo.BuildCurrentPlatform \
  -logFile Logs/build-demo.log
```

Output on macOS:
```
Builds/TextQuestReaderDemo/current/TextQuestReaderDemo.app
Logs/build-demo.log
```

Verified locally: exit 0, no compile errors during build, app size ~ Unity 6
StandaloneOSX default.

## Rollback

Pre-theme safety branch:
```bash
git switch backup/cinematic-upgrade-pre-theme-20260520-0434
```

Per-commit revert:
```bash
git revert <commit-sha>
```

Quest data only:
```bash
cp ../_backups/SpaceStation_en.quest.json.20260520-044539 \
   Assets/StreamingAssets/Quests/SpaceStation_en/quest.json
```

## Where the evidence goes

After Play Mode capture, place:

- `docs/playmode-evidence/catalog.png`
- `docs/playmode-evidence/spacestation-intro.png`
- `docs/playmode-evidence/dark-section.png`
- `docs/playmode-evidence/monster-confrontation.png`
- `docs/playmode-evidence/victory.png`
- `docs/playmode-evidence/failure.png`
- `docs/playmode-evidence/playmode.mp4`

Then commit:
```bash
git add docs/playmode-evidence
git commit -m "test(playmode): capture theme-effects-only evidence"
```

When all the above are present and `Logs/compile-theme.log` shows zero
errors, this branch is reviewable.
