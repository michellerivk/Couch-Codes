# Couch Codes

**Couch Codes** is a local multiplayer party game inspired by *Codenames*, built as a **solo portfolio project** to explore **real-time communication with sockets** and multi-device gameplay.

The game runs on a **PC as the host (Unity)** while players join using their **phones via a web browser**, all connected over the same local network (LAN).

---

## 🎯 Project Goals

This project was created to:
- Deeply understand **socket-based networking**
- Design a **shared game state** across Unity and web clients
- Handle **turn-based multiplayer logic**
- Explore **reconnection** and **room management**
- Build a complete, playable system from scratch as a solo developer

---

## 🕹️ Gameplay Overview

- One player runs the game on PC (Unity build) - this is the **host**
- Other players join using their phones by entering a **room code**
- Players choose a **role** and **team**
- Teams take turns giving clues and guessing words
- Guessing the wrong card (or the bomb 💣) can immediately end the game
- There are 4 possible languages to run the game on (English, Russian, Czech, Hebrew)

---

## 👥 Player Roles

### 🔍 Clue Master
- Sees the **full board with card colors** on their phone
- Gives a **single-word clue + number**
- Cannot guess

### 🎯 Guesser
- Sees only the **words**
- Highlights and submits guesses
- Can end their turn early

---

## ✨ Features

- Real-time multiplayer using **Socket.IO**
- Unity host + web-based mobile clients
- Turn-based game flow (clue -> guess -> switch teams)
- Highlight synchronization between clients and Unity
- Card reveal system (cards disappear after being guessed)
- Game-over handling with win reasons (bomb / all cards guessed)
- Local reconnection support using browser storage
- Room auto-close when host disconnects
- LAN-friendly setup (no external servers required)

---

## 🛠️ Tech Stack

**Host / Game Logic**
- Unity (C#)
- Custom GameManager & NetworkManager architecture

**Networking**
- Node.js
- Socket.IO (WebSocket transport)

**Client (Phones)**
- HTML / CSS / JavaScript
- Socket.IO client
- Runs in any modern mobile browser

---

## ▶️ How to Run the Game

### Requirements
- Windows PC
- Phones on the **same local network (LAN)**
- Firewall permission (first run only)

### Steps
1. Download and run the **Unity executable** (Link: https://michellerivk.itch.io/couch-codes)
2. When prompted, **approve the firewall pop-up**
   - This allows local network communication
3. The **Node.js server starts automatically**
   - `node.exe` is bundled inside the Unity project
4. On your phone:
   - Open a browser
   - Enter the PC’s local IP address
   - Join using the room code shown on the host
5. Choose a role, team, and start playing 🎉

> No manual server setup is required.

---

## 📷 Screenshots
- Main Menu
<img width="994" height="507" alt="Main Menu" src="https://github.com/user-attachments/assets/e9f0bc58-65ee-4784-ade7-ea78a56148e0" />
- Lobby screen
- Clue Master board
- Guesser phone UI
- Game over screen

---

## ⚠️ Known Limitations
- Designed for **LAN play only**
- Single host per room
- The room closes after the host leaves the game

---

## 🚀 Future Improvements

- Online matchmaking (non-LAN)
- Spectator mode
- Better mobile animations
- Host migration
- Sound effects & music
- Custom word packs
