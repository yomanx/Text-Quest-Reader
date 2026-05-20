# Theme-Effects-Only — final regression check

Branch: `theme-effects-only` at `ec6e433`
Base: `f1f19f0` (= last commit of `cinematic-upgrade` before this branch).

## Counts (current quest.json)

```
locationCount  35
passageCount   81
location ids   35
passage ids    80
```

`passageCount` field declared `81`, actual array length `80` — **pre-existing
off-by-one inherited from upstream**, not introduced here. See backup
file `../_backups/SpaceStation_en.quest.json.20260520-044539` — same value.

## Files changed on this branch vs base

```
 Assets/StreamingAssets/Quests/SpaceStation_en/quest.json      |   2 +-
 Assets/_Scripts/_CinematicShell/CinematicShellBootstrap.cs    |  19 ++-
 Assets/_Scripts/_View/GamePanel.cs                            |  10 +-
 docs/COMPILE_BASELINE.md                                      |  60 ++++
 docs/PLAYMODE_BASELINE.md                                     |  70 ++++
 docs/PREPASS_SPACESTATION.md                                  |  95 ++++
 docs/THEME_EFFECTS_AUDIT.md                                   | 147 ++++
 7 files changed, 393 insertions(+), 10 deletions(-)
```

### Per-file classification

| File | Type | Risk |
|------|------|------|
| `quest.json` | Data — descriptions only | 🟢 Theme-only |
| `CinematicShellBootstrap.cs` | Comment block expansion | 🟢 No behaviour change |
| `GamePanel.cs` | `EnsureMainMenuSkin()` body neutered to early `return` | 🟢 Hardening, no behaviour change |
| 4 docs files | Documentation | 🟢 Doc-only |

## Regression assertions

| Assertion | Status |
|-----------|:------:|
| `AsteroidStation_NeonDescent/*` untouched on this branch | ✅ |
| No new quest folders introduced | ✅ |
| Shell `[RuntimeInitializeOnLoadMethod]` attribute remains absent | ✅ |
| No call to `MainMenuTerminalSkin.Attach` from any source file | ✅ |
| `LegacyUiSuppressor.AddComponent` only reachable from dormant shell | ✅ |
| No new `.cs` files added on this branch | ✅ |
| `locationCount`, `passageCount` unchanged vs backup | ✅ |
| `parameters` byte-equal vs backup | ✅ |
| `passages[*]` byte-equal vs backup | ✅ |
| Top-level metadata byte-equal vs backup | ✅ |
| Per-location non-description fields byte-equal vs backup | ✅ |
| Each `locations[i].descriptions` array length unchanged | ✅ |

(Verified after every commit by the `invariants vs backup` script in the
previous pass log.)

## Theme-only delta of `quest.json`

The 4 theme passes (commits `267c16c..ec6e433`) show `1 changed, 1 insertion(+), 1 deletion(-)` in `git diff --stat` because the entire file is one line of compact JSON. Logical impact:

- **29 locations** had `descriptions[i]` strings re-tagged (existing leading
  `<bg>/<fx>/<so>` prefix replaced by the new patch prefix; legacy
  `<im ... im>`, `<mu ... mu>` and inline `<so ... so>` media references
  preserved after the prefix).
- **6 locations** intentionally NOT touched on this branch: `9, 25, 26, 27,
  28, 33`. They retain tag prefixes from the earlier `cinematic-upgrade`
  cinematic pass.
- **0 logic / id / passage / parameter changes.**

Modified ids (sorted): `[1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 29, 30, 31, 32, 34, 35]`.

## Per-pass commit map

| Commit | Locations | Theme |
|--------|-----------|-------|
| `267c16c` | 1–8 | Wake / corridor / breach / monster-arc opener |
| `a0672c3` | 10–17 | Support rooms / pickups / rest beat |
| `0296b2b` | 18–24 | Puzzle / leak repair / fail / victory / map |
| `ec6e433` | 29, 30, 31, 32, 34, 35 | Combat climax / comms finale (suspense → hope → doom) |

## Rollback paths

```
# Full revert to pre-theme state:
git switch backup/cinematic-upgrade-pre-theme-20260520-0434

# Or quest.json alone:
cp ../_backups/SpaceStation_en.quest.json.20260520-044539 \
   Assets/StreamingAssets/Quests/SpaceStation_en/quest.json
```

## Conclusion

The branch is tightly scoped, reversible, and theme-only. No quest logic,
no UI replacement, no new content. Safe to ship as a thematic-only patch
on top of `cinematic-upgrade`.
