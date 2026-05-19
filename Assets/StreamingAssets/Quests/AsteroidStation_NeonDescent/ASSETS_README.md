# Asteroid Station: Neon Descent — Placeholder Assets

The showcase quest references the following image / music / SFX names. The
project intentionally ships **without** binary copies of these — placeholder
images and audio are not bundled to keep the repo small and copyright-clean.

Drop your own assets into the matching folders before publishing the build.
The game will degrade gracefully if a file is missing (it'll log a warning
and skip the visual/audio, but text and choices continue).

## Images (`Images/` folder, PNG or JPG)

| File          | Suggested mood                                  |
|---------------|--------------------------------------------------|
| `preview.png` | Catalog thumbnail — neon-lit asteroid station    |
| `cryo_pod`    | Frosted cryopod interior, red emergency light    |
| `corridor`    | Long dim corridor, pulsing red lights            |
| `main_hub`    | Holographic station map, half-red                |
| `bridge`      | Cracked viewport, asteroid in distance           |
| `medbay`      | Empty medical bay, flatlining monitors           |
| `reactor`     | Reactor housing, coolant steam                   |
| `comms`       | Wall of receivers, white noise displays          |
| `storage`     | Crates, stand-alone scrubber, classified box     |
| `hidden_lab`  | Hidden workbench, photographs of V. Lindgren     |
| `server_room` | Stripped-out server racks                        |
| `cargo`       | Cargo hold, opened asteroid sample case          |
| `observation` | Open vacuum, pulsing magenta asteroid            |
| `quarters`    | Six bunks, locker tags                           |
| `engine_bay`  | Drive offline, diverted power conduit            |
| `airlock`     | Sealed inner door, pressure-wash burns           |
| `signal_sent` | Calm constellation, rescue beacon                |
| `suffocation` | Blackout / heartbeat-flatline visual             |
| `truth`       | Soft magenta-violet light, peaceful              |

## Music (`Musics/` folder, OGG/MP3/WAV)

| File             | Use                                            |
|------------------|------------------------------------------------|
| `ambient_dread`  | Cryopod / opening loop                         |
| `ambient_pulse`  | Main hub / exploration                         |
| `ambient_low`    | Server room                                    |
| `ambient_deep`   | Observation deck                               |
| `reactor_hum`    | Reactor room                                   |
| `hopeful_outro`  | Victory endings                                |
| `sad_outro`      | Death ending                                   |

## SFX (`Sounds/` folder, OGG/WAV)

| File             | Trigger                                         |
|------------------|-------------------------------------------------|
| `door_hiss`      | Entering corridor                               |
| `console_hum`    | Bridge ambient hit                              |
| `heart_monitor`  | Medbay flatline                                 |
| `radio_static`   | Comms array                                     |
| `paper_rustle`   | Quarters / V's note                             |
| `warning_klaxon` | Airlock                                         |
| `alarm`          | Oxygen failure                                  |
| `med_inject`     | Adrenaline shot                                 |
| `explosion_small`| Reactor vent                                    |
| `soft_static`    | V. Lindgren recorder                            |

If the reader hits an asset that does not exist, you'll see a warning in the
Unity console — but the quest will still play through. Cinematic tags
(`<fx ...>`) work independently of audio/image files.
