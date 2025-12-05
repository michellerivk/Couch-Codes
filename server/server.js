const http = require('http'); // Loads node's built in HTTP module that lets us create the http server
const fs = require('fs');     // Loads the file system module that lets us read/write in files
const path = require('path'); // Loads the path helper module that's used to build paths in a Cross-Platform-Save way
const { Server } = require('socket.io'); // Loads the socket.io module, pulls out it's server class into a variable

const server = http.createServer((req, res) => { // Creates a new HTTP server. req = incoming request, res = outgoing response
console.log('Request for:', req.url); // Log what URL has been requested

  if (req.url === '/' || req.url === '/index.html') { // If the browser request '/' or '/index', send the index.html file
    const filePath = path.join(__dirname, 'public', 'index.html'); //__dirname = the folder where the server.js file lives.
                                                                   // Creates the full path of the index.html file 

    fs.readFile(filePath, (err, data) => { // The err is an internal node catch.
      if (err) 
      {
        console.error('Error reading index.html:', err); // Log an error
        res.writeHead(500, { 'Content-Type': 'text/plain' }); // Send HTTP status code 500 = internal server error
        res.end('Error loading page'); // Sends that text as the body and finishes the response.
        return;
      }

      res.writeHead(200, { 'Content-Type': 'text/html' }); // 200 = HTTP status OK, content type = Tells the browser this is an html page 
      res.end(data); // Sends the data we got to the browser (The contents of the index.html page)
    });
  } 
  else if (req.url === '/styles.css') { // If the client is trying to reach the css file, send them the styles.css file
    const cssPath = path.join(__dirname, 'public', 'styles.css'); //__dirname = the folder where the css file lives.
    fs.readFile(cssPath, (err, data) => {
      if (err) {
        console.error('Error reading styles.css:', err); // The same as above
        res.writeHead(500, { 'Content-Type': 'text/plain' });
        res.end('Error loading CSS');
        return;
      }
      res.writeHead(200, { 'Content-Type': 'text/css' });
      res.end(data); // Sends the contents of the css page to the browser if everything is ok
    });
  }
  else if (req.url === '/favicon.ico') { // If the vlient is trying to access the little picture of the site send that it's ok
    res.writeHead(204);
    res.end();
  }
  else {
    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('Not found');
  }

});

const io = new Server(server, { // Creates a socket.io server instance named io.
  cors: { origin: "*" } // Allow any webpage origin join to connect via socket.io ( Ok for LAN )
});

const rooms = {}; // Create an empty object named rooms that will hold all the rooms

io.on("connection", (socket) => { // Listens to clients connecting. socket = the connection of the client
  console.log("Client connected:", socket.id); // GIving each client a unique ID

  socket.on("joinRoom", ({ name, room, role, team }) => {
    if (!name || !room || !role || !team) { // Validate that there are inputs
    socket.emit("joinError", "Please enter a name, room code, and choose a role and a team."); // If there aren't ask the user to input things
    return;
  }

  const roomCode = room.toUpperCase(); // Uppercase the code incase there are letters in it (for normalization)

  if (!rooms[roomCode]) { // If the room doesn't exist
    
    if (role !== "host") {
      socket.emit("joinError", "Host hasn't started this room yet.");
      return;
    }

    rooms[roomCode] = { players: {} }; // Create an empty player inside a room so that on connection we will get the player's info
  }

  if (role !== "host") { // If a non host joins - reject him
    const hasHost = Object.values(rooms[roomCode].players).some(p => p.role === "host");
    if (!hasHost) {
      socket.emit("joinError", "Host hasn't joined this room yet.");
      return;
    }
  }

  const normalizedNewName = name.toLowerCase().trim(); // Trip the spaces in the name

  const nameTaken = Object.values(rooms[roomCode].players).some(p => // Check if this name already exists
    p.name && p.name.toLowerCase().trim() === normalizedNewName      // .some - checks if at least one element in this array matches this condition
  );

  if (nameTaken) { // If the name exists, return an error
    socket.emit("joinError", "That username is already taken in this room. Please choose another.");
    return;
  }
  
  rooms[roomCode].players[socket.id] = { // Store the info of the player in the storage object
      name, // Player name
      role, // Player role
      team  // Player team
    };

  socket.join(roomCode); // Join the specific player connection to the room (let the player access the room)

  console.log(`Socket ${socket.id} joined room ${roomCode} as a ${role} to the ${team} team, with the name ${name}`); // Log that the player joined

  socket.emit("joinSuccess", { room: roomCode, name, role, team }); // Tell the client the join was successful

  const players = Object.entries(rooms[roomCode].players).map(([id, player]) => ({ // Creates an array of the players.
      id,
      name: player.name,
      role: player.role,
      team: player.team
    }));

    /*
    * will create something like this:
    *
    * const players = [
    * { id: "socketA", name: "Michelle", role: "guesser", team: "red" },
    * { id: "socketB", name: "Lear", role: "clue", team: "blue" }
    * ];
    * 
    */

    io.to(roomCode).emit("roomUpdate", { room: roomCode, players }); // Tell the user the update room state
  });

  socket.on("startGame", ({ room }) => { // When the host presses start game -> send a messsage to the clients
    const roomCode = room.toUpperCase();
    console.log(`startGame received for room ${roomCode}`);

    io.to(roomCode).emit("gameStarted", { room: roomCode });
  });

  socket.on("submitClue", ({ room, clueWord, clueNumber }) => { // A listener for submiting a clue
    if (!room || !clueWord || !clueNumber) { // If the room , the clue, or the number dont exist -> return
      socket.emit("joinError", "Missing clue data.");
      return;
    }

    const roomCode = room.toUpperCase();
    const roomData = rooms[roomCode];

    if (!roomData) { // If there is no data in the room (players, for example) -> return
      socket.emit("joinError", "Room does not exist.");
      return;
    }

    const player = roomData.players[socket.id];
    if (!player) {
      socket.emit("joinError", "You are not part of this room.");
      return;
    }

    if (player.role !== "clue") {
      socket.emit("joinError", "Only clue masters can submit clues.");
      return;
    }

    console.log(
      `Clue from ${player.name} in room ${roomCode}:`,
      clueWord,
      clueNumber
    );

    // Broadcast the new clue to everyone in the room (Host + Clients)
    io.to(roomCode).emit("newClue", {
      room: roomCode,
      clueWord,
      clueNumber,
      team: player.team,
      from: player.name    // optional: who sent it
    });
  });

    socket.on("disconnect", () => { // Listens to clients disconnecting
    console.log("Client disconnected:", socket.id); // Logs the disconnect

    for (const [roomCode, roomData] of Object.entries(rooms)) {
      if (roomData.players[socket.id]) {

        delete roomData.players[socket.id]; // Remove the player from the room

        const players = Object.entries(roomData.players).map(([id, player]) => ({ // Update the player list (build it again)
          id,
          name: player.name,
          role: player.role,
          team: player.team
        }));

        io.to(roomCode).emit("roomUpdate", { room: roomCode, players }); // Tell the room the player left

        if (players.length === 0) { // If the room is empty -> delete it
          delete rooms[roomCode];
          console.log(`Room ${roomCode} is now empty and was removed.`);
        }
      }
    }
  });

});

const PORT = 3000;
server.listen(PORT, '0.0.0.0', () => { // Start the server on the wanted port
  console.log(`Server listening on http://localhost:${PORT}`);
});

