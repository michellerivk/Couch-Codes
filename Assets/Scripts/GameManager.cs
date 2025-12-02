using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Transform _boardParent;
    

    private WordRecordCollection _wordData;

    private void Awake()
    {
        LoadWords();

        PrintWords();

        SettingCards();
    }

    // Loads the words from the JSON file into an array of strings
    private void LoadWords()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "words.json"); // Create path to the json file

        string jsonString = File.ReadAllText(path); // Turn the file into a string

        _wordData = JsonUtility.FromJson<WordRecordCollection>(jsonString); // Get the words from the json, and insert into an array: _wordData.words
    }
    
    // Prints the words to the console - for debugging purposes
    private void PrintWords()
    {
        if (_wordData == null || _wordData.words == null)
        {
            Debug.Log("No word data loaded.");
            return;
        }

        foreach (var w in _wordData.words)
        {
            Debug.Log($"id={w.id}, en={w.en}, he={w.he}, ru={w.ru}, cs={w.cs}");
        }
    }

    private void SettingCards()
    {
        List<string> usedWords = new List<string>();

        for (int i = 0; i < 25; i++)
        {
            Card newCard = Instantiate(_cardPrefab, _boardParent);

            int randomIndex = UnityEngine.Random.Range(1, 95);

            while (usedWords.Contains(_wordData.words[randomIndex].id))
                randomIndex = UnityEngine.Random.Range(1, 95);

            newCard.Init(_wordData.words[randomIndex].id, _wordData.words[randomIndex].en);
            usedWords.Add(_wordData.words[randomIndex].id);
        }
    }
}
