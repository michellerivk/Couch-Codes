using SocketIOClient.Newtonsoft.Json;
using SocketIOClient;
using System;
using UnityEngine;
using static SocketIOUnity;
using static PlayerData;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    [SerializeField] private LobbyManager _lm;

    public SocketIOUnity socket { get; private set; }

    private void Start() // We want to get the code from the LobbyManger.Awake() first
    {
        // Using the singleton Data Structure to keep the network running between scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_lm == null)
        {
            Debug.LogError("NetworkManager: '_lm' is not set in the Inspector");
            return;
        }

        HandleSocketConnection();
    }

    public void HandleSocketConnection()
    {
        var uri = new Uri("http://localhost:3000"); // Where the node server is

        socket = new SocketIOUnity(uri, new SocketIOOptions // Create a socket
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            // Can add query for AUTH in the future (Prob. won't)
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.unityThreadScope = UnityThreadScope.Update; // Any 'OnUnityThread' callbacks from this socket should be executed during the Update phase.

        socket.OnConnected += (sender, e) => // Join CouchCodes as a host
        {
            Debug.Log("Unity socket.OnConnected to Node");

            var joinData = new
            {
                name = "Host",
                room = _lm.roomCode,  // The room code
                role = "host",
                team = "host"
            };

            socket.Emit("joinRoom", joinData); // The same as socket.on("joinRoom", ({ name, room, role, team })
        };

        socket.OnDisconnected += (sender, e) => // Disconnect log
        {
            Debug.Log("Unity socket disconnected: " + e);
        };

        socket.OnUnityThread("roomUpdate", response => // Listen to the roomUpdate event, to update the lobby
        {
            Debug.Log("roomUpdate from server: " + response.ToString());

            // Parse the JSON response
            var json = response.ToString();

            RoomUpdateData data = null;

            data = response.GetValue<RoomUpdateData>();

            if (data == null || data.players == null) // Check if there're players who joined
            {
                Debug.LogWarning("roomUpdate has no players");
                return;
            }

            _lm.UpdateLobby(data.players);
        });

        Debug.Log("Connecting to Node server at " + uri);
        socket.Connect();
    }
}
