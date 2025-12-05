using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public enum Language { English, Hebrew, Russian, Czech }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Transform _boardParent;
    [SerializeField] private Language _boardLanguage = Language.English;
    [SerializeField] private TextMeshProUGUI _clueText;
    [SerializeField] private TextMeshProUGUI _clueNumber;

    private WordRecordCollection _wordData;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameManager instances detected, destroying the new one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadWords();
         
        CreateBoard();
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

    // Creates the game board
    private void CreateBoard()
    {
        List<string> usedWords = new List<string>();

        List<string> blueCards = new List<string>();
        List<string> redCards = new List<string>();
        List<string> bombCards = new List<string>();
        List<string> neutralCards = new List<string>();

        for (int i = 0; i < 25; i++)
        {
            Card newCard = Instantiate(_cardPrefab, _boardParent);

            int randomIndex = UnityEngine.Random.Range(0, _wordData.words.Length);

            while (usedWords.Contains(_wordData.words[randomIndex].id))
                randomIndex = UnityEngine.Random.Range(0, _wordData.words.Length);

            usedWords.Add(_wordData.words[randomIndex].id);

            bool addedCard = RandomizeOwner(newCard, randomIndex, blueCards, redCards, bombCards, neutralCards);

            while (!addedCard)
            {
                addedCard = RandomizeOwner(newCard, randomIndex, blueCards, redCards, bombCards, neutralCards);
            }          
        }
    }

    // Randomizes the type of the card (blue, red, bomb, neutral)
    private bool RandomizeOwner(Card newCard, int randomIndex, List<string> blueCards, List<string> redCards, List<string> bombCards, List<string> neutralCards)
    {
        CardOwner owner = (CardOwner)UnityEngine.Random.Range(0, 4);
        WordRecord word = _wordData.words[randomIndex];

        string language = BuildLanguage(_boardLanguage, word);

        switch (owner)
        {
            case CardOwner.Red:
                if (redCards.Count < 9)
                {
                    newCard.Init(word.id, owner, language);
                    redCards.Add(word.id);
                    return true;
                }

                else
                    return false;

            case CardOwner.Blue:
                if (blueCards.Count < 8)
                {
                    newCard.Init(word.id, owner, language);
                    blueCards.Add(word.id);
                    return true;
                }

                else
                    return false;

            case CardOwner.Neutral:
                if (neutralCards.Count < 7)
                {
                    newCard.Init(word.id, owner, language);
                    neutralCards.Add(word.id);
                    return true;
                }

                else
                    return false;

            case CardOwner.Bomb:
                if (bombCards.Count < 1)
                {
                    newCard.Init(word.id, owner, language);
                    bombCards.Add(word.id);
                    return true;
                }
                else
                    return false;
        }

        return false;
    }

    // Returns the language according to the chosen language
    private string BuildLanguage(Language language, WordRecord record)
    {
        switch (language)
        {
            case Language.English: return record.en;
            case Language.Hebrew: return record.he;
            case Language.Russian: return record.ru;
            case Language.Czech: return record.cs;
            default: return record.en;
        }
    }

    // Setting a new clue after getting it from a Clue Master
    public void SetClue(string clueWord, string clueNumber, string team)
    {
        if (_clueText == null || _clueNumber == null) // Make sure the clue texts are assigned
        {
            Debug.LogWarning("GameManager: clue text fields are missing.");
            return;
        }

        _clueText.text = clueWord;
        _clueNumber.text = clueNumber;
    }
}
