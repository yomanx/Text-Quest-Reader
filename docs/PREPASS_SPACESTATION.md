# SpaceStation_en — pre-pass snapshot

Date: 2026-05-20 04:45
Branch: `theme-effects-only` at `28ed7a5`
Quest file: `Assets/StreamingAssets/Quests/SpaceStation_en/quest.json`

## Out-of-repo backup

```
/Users/demismezirov/.openclaw/workspaces/_backups/SpaceStation_en.quest.json.20260520-044539
size: 261,736 bytes
```

The backup sits outside the git working tree (`../_backups/`), so even a
`git clean -fdx` or branch reset can't lose it. Path is timestamped so
subsequent backups don't overwrite this one.

## Counts (baseline — must remain unchanged after the tag pass)

| Field | Declared | Actual |
|------|---------:|-------:|
| `locationCount` | 35 | 35 |
| `passageCount` | 81 | 80 _(pre-existing off-by-one — present in upstream, not introduced here)_ |
| `parameters` (array length) | — | 13 |

Location ids sweep `1..35` contiguously.

## Top-level keys (must remain identical)

```
ownerUserId, id, questName, displayName, description,
startMusic, startImage,
locationCount, passageCount, order, lang,
parameters, locations, passages
```

No new top-level keys are added by the tag-only pass.

## Scope of the tag-only pass

This pass will **only** modify the string content of:

- `locations[*].descriptions[i]` — prepend `<bg ... bg>` and/or `<fx ... fx>`
  tags. The existing `<im ... im>`, `<mu ... mu>`, `<so ... so>` legacy media
  tags stay in place; new tags are inserted before them so the original
  media references keep working.

This pass will **not** modify:

- `parameters` — neither the array nor any field.
- `locations[*].id`, `.locationType`, `.passability`, `.influences`,
  `.paramsActions`, `.visitCounter`, `.chooseWithFormula`, `.formula`,
  `.gridX`, `.gridY`, `.firstInPair`.
- `locations[*].descriptions` — array length stays the same; only string
  content of each entry is touched.
- `passages[*]` — no field, including `question`, `description`,
  `logicalCondition`, `priority`, `necessaryRanges`, etc.
- Top-level metadata (`questName`, `displayName`, `locationCount`,
  `passageCount`, `order`, `lang`, `description`, `startMusic`,
  `startImage`, etc.) — frozen.

## Post-pass invariants to re-verify

After each tag edit, the following must still be true. Add a verification
script to `tools/verify_spacestation.py` if needed:

1. `json.loads(...)` succeeds.
2. `len(d["locations"]) == 35`.
3. `sorted([loc["id"] for loc in d["locations"]]) == list(range(1, 36))`.
4. `len(d["passages"]) == 80`.
5. `len(d["parameters"]) == 13`.
6. For every `locations[i]`: `len(d["locations"][i]["descriptions"])` is unchanged
   (compare against the backup file).
7. For every `passages[i]`: every field matches the backup byte-for-byte.

## Rollback

If the tag pass produces an unexpected diff, copy the backup back:

```
cp ../_backups/SpaceStation_en.quest.json.20260520-044539 \
   Assets/StreamingAssets/Quests/SpaceStation_en/quest.json
```

Or revert via git:

```
git checkout Assets/StreamingAssets/Quests/SpaceStation_en/quest.json
```

Or switch to the safety branch:

```
git switch backup/cinematic-upgrade-pre-theme-20260520-0434
```
