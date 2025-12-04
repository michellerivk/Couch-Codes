using JetBrains.Annotations;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerData;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _codeText;
    [SerializeField] private GameObject _playerStats;
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;

    public string roomCode { get; private set; }

    private int _redPlayers = 0;
    private int _bluePlayers = 0;
    private int _codeMasters = 0;

    public bool canSwitchScene { get; private set; } = false;

    Dictionary<string, string> _codeMastersTeam = new Dictionary<string, string>();


    private void Awake()
    {
        roomCode = CreateRoomCode();
        _codeText.text += roomCode;
    }

    // Creates a random room code
    private string CreateRoomCode()
    {
        string roomCode = "";
        int maxCodeLength = 4;

        string[] PossibleLetters = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        string[] PossibleNumbers = new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

        for (int i = 0; i < maxCodeLength; i++) // Generate the code
        {
            if (UnityEngine.Random.Range(0, 10) < 5) // Make the odds between choosing a letter or a number 50% 
                roomCode += PossibleLetters[UnityEngine.Random.Range(0, PossibleLetters.Length)];
            else
                roomCode += PossibleNumbers[UnityEngine.Random.Range(0, PossibleNumbers.Length)];
        }

        Debug.Log(roomCode);


        return roomCode;
    }

    // Adds a player to his chosen team
    public void AddTeamPlayer(string name, string team, string role)
    {
        string fullRole = "";

        if (name == null || team == null || role == null)
        {
            Debug.Log("One of the parameters was not entered");
            return;
        }

        if (name == "Host" || team == "host" || role == "host")
        {
            Debug.Log("The user is the host - no need to put in a ny team");
            return;
        }

        switch (role)
        {
            case "clue":
                fullRole = "Clue Master";
                break;

            case "guesser":
                fullRole = "Guesser";
                break;
        }

        switch (team)
        {
            case "red":
                GameObject redChild = Instantiate(_playerStats, _redTeam);
                TextMeshProUGUI redTxt = redChild.GetComponent<TextMeshProUGUI>();

                if (redTxt != null)
                {
                    redTxt.text = $"Player: {name} - Role: {fullRole}";
                }


                break;

            case "blue":
                GameObject blueChild = Instantiate(_playerStats, _blueTeam);
                TextMeshProUGUI blueTxt = blueChild.GetComponent<TextMeshProUGUI>();

                if (blueTxt != null)
                {
                    blueTxt.text = $"Player: {name} - Role: {fullRole}";
                }

                break;
        }
    }

    // Updates the players in the lobby, if another one joined
    public void UpdateLobby(List<PlayersData> players)
    {
        _redPlayers = 0;
        _bluePlayers = 0;
        _codeMasters = 0;
        _codeMastersTeam.Clear();

        // Destroy the existing players so there won't be any duplicates
        foreach (Transform child in _redTeam)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in _blueTeam)
        {
            Destroy(child.gameObject);
        }

        // Insert the new snapshot
        foreach (var player in players)
        {
            AddTeamPlayer(player.name, player.team, player.role);

            if (player.team == "red")
                _redPlayers++;

            if (player.team == "blue")
                _bluePlayers++;

            if (player.role == "clue")
            {
                _codeMastersTeam.Add(player.name, player.team);
                _codeMasters++;
            }
        }
    }

    public void CheckStartingConditions(string scene)
    {
        // Disabled for testing, but need to enable before a build!!!
        /*
        if (_redPlayers < 2 || _bluePlayers < 2 || _codeMasters < 2)
        {
            Debug.Log("Not enough players");
            return;
        }
        */
        


        if ((_redPlayers + _bluePlayers) > 10)
        {
            Debug.Log("Too many players");
            return;
        }

        if (_codeMastersTeam.Values.GroupBy(value => value).Any(group => group.Count() > 1))
        {
            var groups = _codeMastersTeam.Values.GroupBy(value => value);

            foreach (var group in groups)
            {
                if (group.Count() > 1)
                {
                    Debug.Log($"There are more than 1 Clue Master in the {group.Key} team");
                    return;
                }
            }
        }

        canSwitchScene = true;
        SceneSwitcher.SwitchScene(scene);
    }
}
