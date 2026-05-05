[English](README.md) | [Українська](README.ua.md) | [Русский](README.ru.md)

# Text Quest Reader

![Unity](https://img.shields.io/badge/engine-Unity-000000?logo=unity&logoColor=white)
![Language](https://img.shields.io/badge/language-C%23-blue)
![Format](https://img.shields.io/badge/data-JSON-orange)
![License](https://img.shields.io/badge/license-MIT-green)

A text quest reader for running branching narrative quests, inspired by the mechanics of the game "Space Rangers".

Together with the Text Quest Editor, it forms a complete system for creating, publishing, and running custom text quests.

Supports:
- branching logic and transitions  
- parameters and conditions  
- images and sounds  
- local and remote quests (via API)  

---

## Demo

<img src="docs/screen_1.webp" width="600">
<img src="docs/screen_2.webp" width="600">

---

## Quest Sources

### Local
Assets/StreamingAssets/Quests/

### Remote
Loaded from backend API.

⚠️ First load may take a few seconds due to server cold start.

---

## Controls

- Arrow Up / Down — navigate  
- Enter — confirm  
- Esc — abandon quest  

---

## UX Improvements

- input is blocked during remote loading  
- start button disabled while loading  
- blocker overlay prevents accidental actions  
- remote loading can be cancelled  

---

## Backend

- GET /quests  
- GET /quest/{id}  
- GET /quest/{id}/package  

---

## License

MIT
