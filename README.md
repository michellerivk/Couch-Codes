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
<img width="1044" height="554" alt="gallery-1" src="https://github.com/user-attachments/assets/2b559383-903e-4a97-9be4-50484e4849c8" />
- Board screen
<img width="997" height="558" alt="gallery-2" src="https://github.com/user-attachments/assets/4ee33be1-c01c-4e59-a581-7a2643228fee" />
- Clue Master board
<img width="1135" height="909" alt="gallery-5" src="https://github.com/user-attachments/assets/5dd033d9-93d0-4b25-9422-ae33f3c10a9d" />
- Guesser phone UI
<img width="1067" height="858" alt="gallery-6" src="https://github.com/user-attachments/assets/e877cbcf-6c88-42e8-b6fb-d2c18d6fd311" />

---

## ⚠️ Known Limitations
- Designed for **LAN play only**
- Single host per room
- The room closes after the host leaves the game
- In order to connect to the host, you need to change the visibility of the network from public to private
<img width="672" height="193" alt="image" src="https://github.com/user-attachments/assets/62d839c4-3ad8-4e09-a488-183d15466a93" />

