using JetBrains.Annotations;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using static PlayerData;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _codeText;
    [SerializeField] private GameObject _playerStats;
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;

    public string roomCode { get; private set; }

    private void Awake()
    {
        roomCode = CreateRoomCode();
        _codeText.text += roomCode;
    }

    private void Start()
    {
        AddTeamPlayer("Michelle", "red" , "Code Master");
    }

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
    public void UpdateLobby(List<PlayersData> players)
    {
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
        }
    }

}
