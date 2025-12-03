using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _codeText; 

    private void Awake()
    {
        string roomCode = CreateRoomCode();
        _codeText.text += roomCode;
    }

    public string CreateRoomCode()
    {
        string roomCode = "";
        int maxCodeLength = 4;

        string[] PossibleLetters = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        string[] PossibleNumbers = new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

        for (int i = 0; i < maxCodeLength; i++) // Generate the code
        {
            if (UnityEngine.Random.Range(0, 10) < 5) // Make the odds between choosing a letter or a number 50% 
                roomCode += PossibleLetters[Random.Range(0, PossibleLetters.Length)];
            else
                roomCode += PossibleNumbers[Random.Range(0, PossibleNumbers.Length)];
        }

        Debug.Log(roomCode);


        return roomCode;
    }
}
