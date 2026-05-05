[English](README.md) | [Українська](README.ua.md) | [Русский](README.ru.md)

# Text Quest Reader

![Unity](https://img.shields.io/badge/engine-Unity-000000?logo=unity&logoColor=white)
![Language](https://img.shields.io/badge/language-C%23-blue)
![Format](https://img.shields.io/badge/data-JSON-orange)
![License](https://img.shields.io/badge/license-MIT-green)

A text quest reader for branching story scenarios inspired by the mechanics of Space Rangers.

Together with [Text Quest Editor](https://github.com/albruevich/Text-Quest-Editor), it forms a system for creating and launching custom text quests.

Unlike similar systems, it is not tied to a specific game or platform — quests can be used in any project.

Supports:
- locations and transitions
- parameters and formulas
- images
- sounds and music
- local and remote quests (via API)

The project is open source and can be used:
- as a ready-to-use text quest reader
- as an example of JSON quest integration in Unity
- as a foundation for creating your own reader with a custom UI

---

## Demo

<img src="docs/screen_1.webp" width="600">
<img src="docs/screen_2.webp" width="600">

---

## Quick Start

### Launching from Unity

1. Open the project in Unity
2. Open the scene:
   `Assets/_Scenes/MainScene.unity`
3. Press **Play**

---

### Testing

After launch:

- select a quest (for example, **Asteroid Station**)
- click **Start Selected Quest**

If everything works correctly, the quest will start.

---

## Ready Builds

Downloads are available here:
👉 https://github.com/albruevich/QuestReader/releases

### How to Run

1. Download the archive for your platform
2. Extract it
3. Run the `.exe` / `.app`

---

## Adding Quests

There are two ways:

### 1. Via Folder

Place the quest into:

Assets/StreamingAssets/Quests/

---

### 2. Via Interface

- **Add Quests** — opens the quest folder
- **Refresh** — refreshes the list

<img src="docs/add_quests.webp" width="400">

Allows adding quests without restarting the application.

---

## Important

The quest name inside `quest.json`:

"questName": "YourQuest"

must match the folder name.

---

## Quest Structure

Each quest is stored as a separate folder:

Assets/StreamingAssets/Quests/YourQuest/

### Contents:

- `quest.json` — logic and data (required)
- `Images/` — images (optional)
- `Sounds/` — sounds (optional)
- `Musics/` — music (optional)

<img src="docs/quest_structure.webp" width="280">

The structure is created automatically in **Text Quest Editor**.
No manual setup is required.

---

## Creating Quests

A separate tool is used:
👉 [Text Quest Editor](https://github.com/albruevich/Text-Quest-Editor)

Allows you to:
- visually create locations
- configure parameters
- define transitions
- export a ready-to-use quest

---

## Remote Quests (API)

The reader supports loading quests from a server.

### Features

- viewing available quests
- downloading quests without manual file copying
- launching remote quests

---

## Requirements

### Unity (from source)
- Unity 6.4+

### Ready Builds
- Unity installation is not required

---

## Resources

- some images were generated with AI
- sounds and music: https://pixabay.com/

---

## License

MIT License

---

## Project Usage

If you use the project as a base for your own project or reuse parts of the code:

- mention the author: **albruevich**
- add a link:
  https://github.com/albruevich/Text-Quest-Reader
