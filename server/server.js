const http = require('http'); // Loads node's built in HTTP module that lets us create the http server
const fs = require('fs');     // Loads the file system module that lets us read / write in files
const path = require('path'); // Loads the path helper module that's used to build paths in a Cross-Platform-Save way

const server = http.createServer((req, res) => { // Creates a new HTTP server. req = incoming request, res = outgoing response
console.log('Request for:', req.url); // Log what URL has been requested

  if (req.url === '/' || req.url === '/index.html') { // If the browser request '/' or '/index' we send the index.html file
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
  else {
    if (req.url === '/favicon.ico') { // A page for the little icon of the website
        res.writeHead(204); // 204 = No Content
        res.end();
        return;
        }

    else {
        res.writeHead(404, { 'Content-Type': 'text/plain' }); // If the client requesting something other than index.html, send him 404 = page not found
        res.end('Not found');
    }
  }
});

const PORT = 3000;
server.listen(PORT, '0.0.0.0', () => { // Start the server on the wanted port
  console.log(`Server listening on http://localhost:${PORT}`);
});

