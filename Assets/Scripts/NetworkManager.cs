using SocketIOClient.Newtonsoft.Json;
using SocketIOClient;
using System;
using UnityEngine;
using static SocketIOUnity;
using static PlayerData;
using Newtonsoft.Json;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    [SerializeField] private LobbyManager _lm;

    public SocketIOUnity socket { get; private set; }

    private string _roomCode;
    private bool _didShutdown = false;

    private async void Start() // I want to get the code from the LobbyManger.Awake() first
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

        _roomCode = _lm.roomCode;

        // Ensure Node is up before socket connects
        await NodeServerRunner.Instance.EnsureServerRunning();

        HandleSocketConnection();
    }

    // Connects to the Node using sockets
    public void HandleSocketConnection()
    {
        var uri = new Uri($"http://127.0.0.1:{NodeServerRunner.Instance.Port}"); // Where the node server is

        socket = new SocketIOUnity(uri, new SocketIOOptions // Create a socket
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.unityThreadScope = UnityThreadScope.Update; // Any 'OnUnityThread' callbacks from this socket should be executed during the Update phase.

        socket.OnConnected += (sender, e) => // Join CouchCodes as a host
        {
            Debug.Log("Unity socket.OnConnected to Node");

            var joinData = new
            {
                name = "Host",
                room = _roomCode,  // The room code
                role = "host",
                team = "host"
            };

            socket.Emit("joinRoom", joinData); // The same as socket.on("joinRoom", ({ name, room, role, team })
        };

        socket.OnDisconnected += (sender, e) => // Disconnect log
        {
            Debug.Log($"Unity socket disconnected: {e}");
        };

        socket.OnUnityThread("roomUpdate", response => // Listen to the roomUpdate event, to update the lobby
        {
            Debug.Log($"roomUpdate from server: {response.ToString()}");

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

        socket.OnUnityThread("joinInfo", response =>
        {
            JoinInfoData data = null;

            data = response.GetValue<JoinInfoData>();
            
            if (data == null || !data.ok)
            {
                Debug.LogWarning("joinInfo failed: " + (data?.error ?? "unknown"));
                return;
            }

            Debug.Log($"Host network info: {data.baseUrl} (room {data.room})");

            // Display the IP
            _lm.SetJoinAddress(data.ip, data.port, data.baseUrl);
        });


        socket.OnUnityThread("newClue", response =>
        {
            NewClueData data = null;

            try // Try to get the word data out of the Node message
            {
                data = response.GetValue<NewClueData>();
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse newClue: " + ex.Message);
                return;
            }

            if (data == null)
            {
                Debug.LogWarning("newClue data was null");
                return;
            }

            Debug.Log($"New clue for room {data.room}: {data.clueWord} ({data.clueNumber}) from {data.from}, team {data.team}");

            GameManager.Instance.SetClue(data.clueWord, data.clueNumber, data.team);
        });

        socket.OnUnityThread("highlightCard", response =>
        {
            HighlightCardData data = null;
            try
            {
                data = response.GetValue<HighlightCardData>();
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse highlightCard: " + ex.Message);
                return;
            }

            if (data == null)
            {
                Debug.LogWarning("highlightCard data was null");
                return;
            }

            Debug.Log($"Highlight card {data.cardId} = {data.highlighted} (team {data.team})"); 

            GameManager.Instance.OnCardHighlight(data.cardId, data.team, data.highlighted);
        });

        socket.OnUnityThread("guessCard", response =>
        {
            GuessCardData data = null;

            try
            {
                data = response.GetValue<GuessCardData>();
            }

            catch (Exception ex)
            {
                Debug.LogError("Failed to parse guessCard: " + ex.Message);
                return;
            }

            if (data == null)
            {
                Debug.LogWarning("guessCard data was null");
                return;
            }

            Debug.Log($"Guess from team {data.team} card {data.cardId} in room {data.room}");

            GameManager.Instance.HandleGuess(data.cardId, data.team);
        });

        socket.OnUnityThread("endGuessing", response =>
        {
            EndGuessingData data = null;
            try
            {
                data = response.GetValue<EndGuessingData>();
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse endGuessing: " + ex.Message);
                return;
            }

            if (data == null)
            {
                Debug.LogWarning("endGuessing data was null");
                return;
            }

            Debug.Log($"endGuessing from team {data.team} in room {data.room}");
            GameManager.Instance.OnEndGuessing(data.team);
        });

        Debug.Log($"Connecting to Node server at {uri}");
        socket.Connect();
    }

    // Sends a message to the node that the game has started + room code
    public void OnStartGameButtonPressed()
    {
        if (!_lm.canSwitchScene) // Checks if the scene can be even switched before sending anything to the Node
        {
            return;
        }

        if (NetworkManager.Instance == null || NetworkManager.Instance.socket == null) // If there is no socket return
        {
            Debug.LogWarning("No socket yet, can't send startGame.");
            return;
        }

        NetworkManager.Instance.socket.Emit("startGame", new
        {
            room = _roomCode
        });

        Debug.Log($"Sent startGame for room {_roomCode}");
    }

    public void AfterStatusChange()
    {
        socket.Emit("turnStateUpdate", new
        {
            room = _roomCode,
            activeTeam = GameManager.Instance._currentTeam.ToString().ToLower(), // The current team playing
            phase = GameManager.Instance._currentStatus.ToString(), // The status of the game
            guessesRemaining = GameManager.Instance._guessesRemaining // The amount of guesses left
        });
    }

    public void SendGameOver(WinResult winner)
    {
        if (socket == null)
        {
            Debug.LogWarning("SendGameOver: socket is null");
            return;
        }

        socket.Emit("gameOver", new
        {
            room = _roomCode,
            winningTeam = winner.winningTeam,
            reason = winner.reason
        });

        Debug.Log($"Sent gameOver for room {_roomCode}: winner={winner.winningTeam}, reason={winner.reason}");
    }

    public void SendCardRevealed(string cardId)
    {
        if (socket == null)
        {
            Debug.LogWarning("SendCardRevealed: socket is null");
            return;
        }

        socket.Emit("cardRevealed", new
        {
            room = _roomCode,
            cardId = cardId
        });

        Debug.Log($"Sent cardRevealed for room {_roomCode}, card {cardId}");
    }

    public void SendBoardState(List<CardStateInfo> cards) // Send the board to the HTML client
    {
        if (socket == null)
        {
            Debug.LogWarning("SendBoardState: socket is null");
            return;
        }

        var data = new BoardStateData
        {
            room = _roomCode,
            cards = cards
        };

        socket.Emit("boardState", data);
        Debug.Log($"Sent boardState for room {_roomCode} with {cards.Count} cards");
    }

    public void SendClearHighlights()
    {
        if (socket == null)
        {
            Debug.LogWarning("SendClearHighlights: socket is null");
            return;
        }

        socket.Emit("clearHighlights", new
        {
            room = _roomCode
        });

        Debug.Log($"Sent clearHighlights for room {_roomCode}");
    }

    private void OnApplicationQuit()
    {
        ShutdownSocket();
    }

    private void OnDisable()
    {
        ShutdownSocket();
    }

    private void OnDestroy()
    {
        ShutdownSocket();
    }

    private void ShutdownSocket()
    {
        if (_didShutdown) return;

        _didShutdown = true;

        if (socket == null) return;

        if (!string.IsNullOrEmpty(_roomCode))
        {
            socket.Emit("closeRoom", new { room = _roomCode });
            Debug.Log($"Sent closeRoom for room {_roomCode}");
        }

        // Disconnect
        socket.Disconnect();
        Debug.Log("Socket Disconnect() called");

        socket = null;
    }
}
