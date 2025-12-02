using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.LightTransport;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Transform _boardParent;
    

    private WordRecordCollection _wordData;

    private void Awake()
    {
        LoadWords();

        PrintWords();

        CreatingBoard();
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

    private void CreatingBoard()
    {
        List<string> usedWords = new List<string>();

        List<string> blueCards = new List<string>();
        List<string> redCards = new List<string>();
        List<string> bombCards = new List<string>();
        List<string> neutralCards = new List<string>();



        for (int i = 0; i < 25; i++)
        {
            Card newCard = Instantiate(_cardPrefab, _boardParent);

            int randomIndex = UnityEngine.Random.Range(1, 95);

            while (usedWords.Contains(_wordData.words[randomIndex].id))
                randomIndex = UnityEngine.Random.Range(1, 95);

            usedWords.Add(_wordData.words[randomIndex].id);

            bool addedCard = RandomizeOwner(newCard, randomIndex, blueCards, redCards, bombCards, neutralCards);

            while (!addedCard)
            {
                addedCard = RandomizeOwner(newCard, randomIndex, blueCards, redCards, bombCards, neutralCards);
            }          
        }
    }

    private bool RandomizeOwner(Card newCard, int randomIndex, List<string> blueCards, List<string> redCards, List<string> bombCards, List<string> neutralCards)
    {
        CardOwner owner = (CardOwner)UnityEngine.Random.Range(0, 4);

        switch (owner)
        {
            case CardOwner.Red:
                if (redCards.Count < 10)
                {
                    newCard.Init(_wordData.words[randomIndex].id, owner, _wordData.words[randomIndex].en);
                    redCards.Add(_wordData.words[randomIndex].id);
                    return true;
                }

                else
                {
                    return false;
                }

            case CardOwner.Blue:
                if (blueCards.Count < 9)
                {
                    newCard.Init(_wordData.words[randomIndex].id, owner, _wordData.words[randomIndex].en);
                    blueCards.Add(_wordData.words[randomIndex].id);
                    return true;
                }

                else
                {
                    return false;
                }

            case CardOwner.Neutral:
                if (neutralCards.Count < 8)
                {
                    newCard.Init(_wordData.words[randomIndex].id, owner, _wordData.words[randomIndex].en);
                    neutralCards.Add(_wordData.words[randomIndex].id);
                    return true;
                }

                else
                {
                    return false;
                }

            case CardOwner.Bomb:
                if (bombCards.Count < 1)
                {
                    newCard.Init(_wordData.words[randomIndex].id, owner, _wordData.words[randomIndex].en);
                    bombCards.Add(_wordData.words[randomIndex].id);
                    return true;
                }

                else
                {
                    return false;
                }
        }

        return false;
    }
}
