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
    [Header("Room Code")]
    [SerializeField] private TextMeshProUGUI _codeText;

    [Header("Teams")]
    [SerializeField] private GameObject _playerStats;
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;

    [Header("Language")]
    [SerializeField] private TextMeshProUGUI _languageText;

    [Header("Laptop IP")]
    [SerializeField] private TextMeshProUGUI _joinAddressText;
    [SerializeField] private LobbyIPTyper _ipTyper;

    [Header("Warning")]
    [SerializeField] private TextMeshProUGUI _warning;

    public string roomCode { get; private set; }

    private int _redPlayers = 0;
    private int _bluePlayers = 0;
    private int _codeMasters = 0;

    private int _languageCounter;

    public bool canSwitchScene { get; private set; } = false;

    Dictionary<string, string> _codeMastersTeam = new Dictionary<string, string>();

    private void Awake()
    {
        roomCode = CreateRoomCode();
        _codeText.text += roomCode;

        _languageCounter = PlayerPrefs.GetInt("Language");

        switch (_languageCounter)
        {
            case 0:
                _languageText.text += "Language: English" + " \u2192";
                return;

            case 1:
                _languageText.text += "Language: Hebrew" + " \u2192";
                return;

            case 2:
                _languageText.text += "Language: Russian" + " \u2192";
                return;

            case 3:
                _languageText.text += "Language: Czech" + " \u2192";
                return;
        }

        _warning.text = "";
    }

    // Creates a random room code
    private string CreateRoomCode()
    {
        string roomCode = "";
        int maxCodeLength = 4;

        string[] PossibleLetters = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        string[] PossibleNumbers = new string[9] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        for (int i = 0; i < maxCodeLength; i++) // Generate the code
        {
            if (UnityEngine.Random.Range(1, 10) < 5) // Make the odds between choosing a letter or a number 50% 
                roomCode += PossibleLetters[UnityEngine.Random.Range(0, PossibleLetters.Length)];
            else
                roomCode += PossibleNumbers[UnityEngine.Random.Range(1, PossibleNumbers.Length)];
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
                fullRole = "[CM]";
                break;

            case "guesser":
                fullRole = "[G]";
                break;
        }

        switch (team)
        {
            case "red":
                GameObject redChild = Instantiate(_playerStats, _redTeam);
                TextMeshProUGUI redTxt = redChild.GetComponent<TextMeshProUGUI>();

                if (redTxt != null)
                {
                    redTxt.text = $"{name} {fullRole}";
                }


                break;

            case "blue":
                GameObject blueChild = Instantiate(_playerStats, _blueTeam);
                TextMeshProUGUI blueTxt = blueChild.GetComponent<TextMeshProUGUI>();

                if (blueTxt != null)
                {
                    blueTxt.text = $"{name} : {fullRole}";
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
        if (_redTeam != null)
            foreach (Transform child in _redTeam)
            {
                Destroy(child.gameObject);
            }

        if (_blueTeam != null)
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

    public void SetJoinAddress(string ip, int port, string baseUrl)
    {
        string finalText = $"Enter:\n{baseUrl}";

        if (_ipTyper != null)
        {
            _ipTyper.SetFullText(finalText);
            return;
        }

        if (_joinAddressText != null)
        {
            _joinAddressText.text = finalText;
        }
    }


    public void CheckStartingConditions(string scene)
    {
        if (_redPlayers < 2 || _bluePlayers < 2 || _codeMasters < 2)
        {
            _warning.text = ("Not enough players are in the game!\r\nPlease make sure there're at least:\r\n- 2 blue teamates\r\n- 2 red teamates\r\n- 2 clue masters");
            Debug.Log("Not enough players");
            return;
        }



        if ((_redPlayers + _bluePlayers) > 10)
        {
            _warning.text = ("There are too many players!\r\nPlease make sure there're only 10 players in the game");
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
                    _warning.text = ($"There is more than 1 Clue Master in the {group.Key} team.");
                    Debug.Log($"There are more than 1 Clue Master in the {group.Key} team");
                    return;
                }
            }
        }

        _warning.text = "";

        canSwitchScene = true;

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(0);

        SceneSwitcher.SwitchScene(scene);
    }

    public void ChangeLanguage()
    {
        _languageText.text = "Language: ";

        if (_languageCounter < 2) // if (_languageCounter < 3)
            _languageCounter++;
        else
            _languageCounter = 0;

        switch(_languageCounter)
        {
            case 0:
                _languageText.text += "English" + " \u2192";
                SaveLanguage(0);
                return;

            //case 1:
            //    _languageText.text += "Hebrew" + " \u2192";
            //    SaveLanguage(1);
            //    return;

            //case 2:
            //    _languageText.text += "Russian" + " \u2192";
            //    SaveLanguage(2);
            //    return;

            //case 3:
            //    _languageText.text += "Czech" + " \u2192";
            //    SaveLanguage(3);
            //    return;

            case 1:
                _languageText.text += "Russian" + " \u2192";
                SaveLanguage(2);
                return;

            case 2:
                _languageText.text += "Czech" + " \u2192";
                SaveLanguage(3);
                return;
        }
    }

    private void SaveLanguage(int language)
    {
        PlayerPrefs.SetInt("Language", language);
        PlayerPrefs.Save();
        Debug.Log($"Saved new language, number: {PlayerPrefs.GetInt("Language")}");
    }
}
