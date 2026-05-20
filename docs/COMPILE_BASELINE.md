# Compile Baseline — theme-effects-only

Date: 2026-05-20 04:42
Branch: `theme-effects-only` at `05d30a7`
Unity: 6000.4.7f1
Command:

```
/Applications/Unity/Hub/Editor/6000.4.7f1/Unity.app/Contents/MacOS/Unity \
  -projectPath . -batchmode -quit -nographics \
  -logFile Logs/compile-theme.log
```

## Result

**Clean compile.**

- Exit code: `0`
- Final line in log: `Exiting batchmode successfully now!`
- `error CS*` count: **0**
- `: error` (any case) count: **0**
- `warning CS*` count: **111** — all non-blocking (see breakdown below).

## TMP sanity

TMP is **fully imported**:

- `Assets/TextMesh Pro/` exists and was scanned by the importer.
- `Library/ScriptAssemblies/Unity.TextMeshPro.dll` + `Unity.TextMeshPro.Editor.dll` built without errors.
- No "TMP Essentials missing" prompt — the project was previously imported and
  the essentials package is checked into the repo at `Assets/TextMesh Pro/`.

Conclusion: **TMP Essentials are NOT a blocker.** No broad fix needed.

## Warning breakdown (all non-blocking)

| Count | Code | Meaning | Source |
|------:|------|---------|--------|
| 105 | CS0618 | Obsolete API usage | 99× from `Assets/Plugins/AssetUsageDetector/Editor/*` (third-party, deprecated TreeView API in Unity 6); 6× from our pragma-wrapped `enableWordWrapping = true` (intentional — TMP 4.x deprecation warning, kept for TMP 3.x compatibility). |
| 6 | CS0219 | Unused local variable | All in `Assets/_Scripts/_CinematicShell/CinematicHeroArt.cs:170-171` (`innerThickness`, `outerThickness` declared but never read). Trivial cleanup, not on theme-pass scope. |

None of these prevent compilation, Editor entry, or Play Mode.

## Files NOT touched

This pass only added documentation. No source changes. The branch state
matches `05d30a7`.

## What this proves

1. The Cinematic Shell, while present in `Assets/_Scripts/_CinematicShell/`,
   compiles cleanly — no dangling references.
2. The legacy GamePanel-driven UI is the active path (no auto-init for shell
   confirmed by previous audit pass).
3. TMP is not a missing-essentials blocker.
4. The branch is **safe for Play Mode smoke test**.

## Next step

Run a manual Play Mode smoke test (Editor GUI), see `docs/PLAYMODE_BASELINE.md`.
