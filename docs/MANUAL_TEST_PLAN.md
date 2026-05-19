# Manual Test Plan — Cinematic Modernization

Use this checklist after opening the project in Unity Editor (Unity 2022.3+
or Unity 6). The list is ordered from "smoke test the build" → "verify each
feature" → "confirm monetization flow".

---

## Smoke test (boot)

1. Open scene `Assets/_Scenes/MainScene.unity` and press Play.
2. Watch the Console — there should be no errors and at most one warning
   (`No monetization.json found — defaulting to empty catalog` will NOT
   appear because the file IS present; if you renamed it for testing, that
   warning is expected).
3. The main menu should show the catalog with three quests: SomeTestQuest,
   SpaceStation_en (with FEATURED badge), AsteroidStation_NeonDescent (with
   PREMIUM badge + orange "Unlock · $2.99" CTA).

If anything errors at this stage, search the Console for the offending file
and check the audit findings in `MANUAL_TEST_PLAN_TROUBLESHOOTING` below.

---

## Free quest still works (backward compatibility)

4. Click `SomeTestQuest`, press the Start button.
5. The screen should fade to black briefly, then fade in on the test quest.
6. The choices should appear with a colored accent bar on the left edge
   and a small mood dot on the right. Default mood (no marker in question
   text) shows blue.
7. Click any choice. Parameter cell should pulse on change. Text should
   typewrite. Press Enter or click to skip typewriter.
8. Reach the Victory cell. ~1.5 seconds later, a cinematic VICTORY overlay
   should appear with final stats and a "Return to library" button.
9. Click Return — catalog reappears, no errors.

---

## Cinematic tags

10. Click `AsteroidStation_NeonDescent`. It's locked — proceed to the
    Monetization section first to unlock it. Then return here.
11. Once unlocked, start the quest. The opening scene should:
    - Fade in from black.
    - Show drifting fog overlay (the `<fx overlay fog on fx>` tag).
    - Play `ambient_dread` if you've supplied the asset (it's safe to play
      without — you'll only see a Console warning).
12. Pick "Step into the corridor." The next scene should:
    - Apply a dark vignette (`<fx vignette ... vignette>`).
13. Navigate to **Main Hub** → **Reactor Room** → **Vent coolant**. The
    passage should:
    - Trigger screen shake (~0.55s, intensity 22).
    - Flash orange/red.
14. Open Settings (gear icon). Toggle "Screen Shake" OFF and "Screen Effects"
    OFF. Replay the same path — shake / flash should be skipped. Toggle
    them back ON.

---

## Typewriter + settings

15. In Settings, drag the **Text Speed** slider to the left (slow).
16. Trigger a new scene. Text should print much slower.
17. Press Enter / Space / left-click during typing — text snaps to full
    visibility.
18. Drag **Master Volume** to 0. Music should fade. Drag back to 1.
19. Drag **Music Volume** to 0. Music silences but click/hover SFX still play.
20. Drag **SFX Volume** to 0. UI clicks silence too.

All settings persist across Play sessions (stored in PlayerPrefs).

---

## Parameter animation

21. Pick a choice that changes Oxygen, e.g. the reactor "Vent coolant"
    (Oxygen -8, Power +25). Watch the Oxygen and Power cells — they should
    pulse on change.
22. Push Oxygen below 20 (drive a long path). The Oxygen cell should turn
    yellow → red. Cross 0 and the game should end with the criticText
    message (with pulse FX).

---

## Result screen

23. Reach a Victory ending (via Comms Array max boost path with high enough
    Power & Signal). ResultScreen should fade in showing "VICTORY" in gold,
    final stats, and a Return button.
24. Reach a Fail ending (e.g. walk out the airlock). ResultScreen should
    fade in showing "FAILURE" in red.
25. Reach the Truth Revealed secret victory via Hidden Lab → Server Room
    → Decrypt log path (requires Power ≥ 25, Trust ≥ 25, Signal ≥ 4).

---

## Monetization — Mock Purchase Flow

26. Open the catalog. `AsteroidStation_NeonDescent` should show:
    - Dimming veil over the card.
    - "PREMIUM" gold badge top-right.
    - Orange "Unlock · $2.99" button bottom-right.
27. Click the orange Unlock button. An UnlockModal should fade in showing:
    - Product display name + description.
    - $2.99 price.
    - "Mock Purchase" and "Cancel" buttons.
28. Click Cancel. Modal closes, no state change.
29. Click Unlock again, then Mock Purchase. After ~0.6s delay the modal
    closes. The catalog re-renders — the quest now shows "OWNED" green
    badge, no veil, no Unlock button.
30. Click the quest and press Start — should fade out, then in on the
    opening scene.
31. To test re-locking, in the Console run:
    ```csharp
    TextQuestReader.Monetization.MonetizationService.Instance.DebugLockAll();
    ```
    Abandon the quest. The catalog should re-render with PREMIUM badge
    restored.
32. To test "Featured" badge, `SpaceStation_en` should show a cyan FEATURED
    badge top-right (no veil, no Unlock).

---

## Backwards compatibility regressions to confirm absent

33. SomeTestQuest still launches without issue (all old tags `<im ... im>`,
    `<so ... so>` work).
34. Settings panel still has its original Language dropdown, Reset button,
    Quit button — the modern sliders are ADDED below, not REPLACED.
35. Remote quests tab (if you have internet) — switch to it via the Sources
    arrow buttons. Remote list loads.
36. Save / Load: start a quest, navigate two locations, close Play, restart
    Play. Should resume at the second location with parameters intact.

---

## Missing-asset graceful degradation

37. `AsteroidStation_NeonDescent` references `ambient_dread.ogg`,
    `cryo_pod.png`, etc. that aren't bundled. Play it through anyway. The
    Console will log warnings (`Audio not found...`, `Texture file not
    found...`), but text, choices, FX, and progression must all continue.

---

## Troubleshooting

If the catalog shows but the AsteroidStation lock CTA doesn't appear:
- Check `Assets/StreamingAssets/monetization.json` parses cleanly (open it
  in the Inspector or run `jq . < monetization.json`).
- Check that `AsteroidStation_NeonDescent` is listed under
  `premiumQuestNames`.
- Run `MonetizationService.Instance.GetAccessState("AsteroidStation_NeonDescent")`
  in the Console. Expect `LockedPremium`.

If cinematic tags do nothing:
- Confirm `Screen Effects` toggle is on in Settings.
- Check the Console for a "[CinematicEffectsService] Bootstrap called
  without a host canvas" warning. If present, the host canvas couldn't be
  resolved — drag-assign one to the bootstrap.

If the Result screen doesn't appear:
- It's bound to `GamePanel.QuestEnded` which fires on Victory/Fail
  locationType OR on parameter-driven critical end (via ParameterService).
  If the quest ends a different way, that event may not fire.

---

## Editor-only QA helpers

From the Unity Console (Play Mode):

```csharp
TextQuestReader.Monetization.MonetizationService.Instance.DebugUnlockAll();   // unlock everything
TextQuestReader.Monetization.MonetizationService.Instance.DebugLockAll();     // lock everything
TextQuestReader.Settings.GameSettings.MasterVolume = 0f;                      // mute master
TextQuestReader.Settings.GameSettings.TextSpeed = 4f;                         // fastest text
PlayerPrefs.DeleteAll();                                                      // full reset
```
