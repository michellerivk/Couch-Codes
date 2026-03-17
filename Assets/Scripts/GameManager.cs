using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public enum Language { English, Hebrew, Russian, Czech }
public enum Team { red, blue }
public enum Status { WaitingForClue, WaitingForGuesses, GameOver }

public struct WinResult
{
    public bool hasWinner;
    public string reason; // How the team won
    public string winningTeam; // The team that won
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Board Objects")]
    [SerializeField] private Card _cardPrefab; // A card on the board
    [SerializeField] private Transform _boardParent; // The board
    public Language _boardLanguage; // The language of the board

    [Header("CM's Clues")]
    [SerializeField] GameObject _clueObject;
    [SerializeField] private TextMeshProUGUI _clueText; // The given clue
    [SerializeField] private TextMeshProUGUI _clueNumber; // The amount of words the clue refers to

    [Header("Cards Counter")]
    [SerializeField] private TextMeshProUGUI _redCards;
    [SerializeField] private TextMeshProUGUI _blueCards;

    [Header("End Game")]
    [SerializeField] GameObject _gameOverObject;
    [SerializeField] private TextMeshProUGUI _endScreenText;

    private List<CardStateInfo> _boardLayoutForClients = new List<CardStateInfo>(); // Info for the HTML board

    private Dictionary<string, Card> _cardsById = new Dictionary<string, Card>(); // A dictionary of cards by their ID

    private WordRecordCollection _wordData; // The data of the word on the card

    public Team _currentTeam { get; private set; } // The team that's currently playing
    public Status _currentStatus { get; private set; } // The status of the game
    public int _guessesRemaining { get; private set; } // Amount of guesses left out of the amount the clue master gave
    public int _redTeamCards { get; private set; } // Amount of red team cards left to guess
    public int _blueTeamCards { get; private set; }  // Amount of blue team cards left to guess
    public bool _wasBombPressed { get; private set; } // Was the bomb pressed

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

        _currentTeam = Team.red;                 // The red team starts
        _currentStatus = Status.WaitingForClue;  // Waiting for first clue
        _guessesRemaining = 0;
        _wasBombPressed = false;

        _boardLanguage = (Language)PlayerPrefs.GetInt("Language", 0);

        // Some starting settings 
        _clueObject.SetActive(true);
        _gameOverObject.SetActive(false);
        _redCards.text = 9.ToString();
        _blueCards.text = 8.ToString();
        _clueText.text = $"Waiting for a {_currentTeam} clue....";
        _clueNumber.text = "?";

        LoadWords();
         
        CreateBoard();
    }
    private void Start()
    {
        if(NetworkManager.Instance != null)
            NetworkManager.Instance.AfterStatusChange();
    }

    // Loads the words from the JSON file into an array of strings
    private void LoadWords()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "words.json"); // Create path to the json file

        string jsonString = File.ReadAllText(path); // Turn the file into a string

        _wordData = JsonUtility.FromJson<WordRecordCollection>(jsonString); // Get the words from the json, and insert into an array: _wordData.words
    }

    // Creates the game board
    private void CreateBoard()
    {
        List<string> usedWords = new List<string>();

        List<string> blueCards = new List<string>();
        List<string> redCards = new List<string>();
        List<string> bombCards = new List<string>();
        List<string> neutralCards = new List<string>();

        _boardLayoutForClients.Clear();

        for (int i = 0; i < 25; i++)
        {
            Card newCard = Instantiate(_cardPrefab, _boardParent);

            int randomIndex = UnityEngine.Random.Range(0, _wordData.words.Length);

            while (usedWords.Contains(_wordData.words[randomIndex].id))
                randomIndex = UnityEngine.Random.Range(0, _wordData.words.Length);

            usedWords.Add(_wordData.words[randomIndex].id);

            bool addedCard = RandomizeOwner(newCard, randomIndex, i, blueCards, redCards, bombCards, neutralCards);

            while (!addedCard)
            {
                addedCard = RandomizeOwner(newCard, randomIndex, i, blueCards, redCards, bombCards, neutralCards);
            }
        }

        _redTeamCards = redCards.Count;
        _blueTeamCards = blueCards.Count;

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.SendBoardState(_boardLayoutForClients);
    }

    // Randomizes the type of the card (blue, red, bomb, neutral)
    private bool RandomizeOwner(Card newCard, int randomIndex, int boardIndex,
                                List<string> blueCards, List<string> redCards, 
                                List<string> bombCards, List<string> neutralCards)
    {
        CardOwner owner = (CardOwner)UnityEngine.Random.Range(0, 4);
        WordRecord word = _wordData.words[randomIndex];

        string language = BuildLanguage(_boardLanguage, word);

        
        if (_boardLanguage == Language.Hebrew) 
        {
            language = ReverseForUnity(language);
        }
        

        string ownerString = owner switch
        {
            CardOwner.Red => "red",
            CardOwner.Blue => "blue",
            CardOwner.Neutral => "neutral",
            CardOwner.Bomb => "bomb",
            _ => "neutral"
        };

        switch (owner)
        {
            case CardOwner.Red:
                if (redCards.Count < 9)
                {
                    newCard.Init(word.id, owner, language);
                    newCard.PlayOpenAnimation(boardIndex * 0.08f);

                    redCards.Add(word.id);

                    _cardsById[word.id] = newCard; // Add the card to the Dict associated with it's ID

                    _boardLayoutForClients.Add(new CardStateInfo
                    {
                        id = word.id,
                        word = language,
                        index = boardIndex,
                        owner = ownerString
                    });

                    return true;
                }

                else
                    return false;

            case CardOwner.Blue:
                if (blueCards.Count < 8)
                {
                    newCard.Init(word.id, owner, language);
                    newCard.PlayOpenAnimation(boardIndex * 0.08f);

                    blueCards.Add(word.id);

                    _cardsById[word.id] = newCard; // Add the card to the Dict associated with it's ID

                    _boardLayoutForClients.Add(new CardStateInfo
                    {
                        id = word.id,
                        word = language,
                        index = boardIndex,
                        owner = ownerString
                    });

                    return true;
                }

                else
                    return false;

            case CardOwner.Neutral:
                if (neutralCards.Count < 7)
                {
                    newCard.Init(word.id, owner, language);
                    newCard.PlayOpenAnimation(boardIndex * 0.08f);

                    neutralCards.Add(word.id);

                    _cardsById[word.id] = newCard; // Add the card to the Dict associated with it's ID

                    _boardLayoutForClients.Add(new CardStateInfo
                    {
                        id = word.id,
                        word = language,
                        index = boardIndex,
                        owner = ownerString
                    });

                    return true;
                }

                else
                    return false;

            case CardOwner.Bomb:
                if (bombCards.Count < 1)
                {
                    newCard.Init(word.id, owner, language);
                    newCard.PlayOpenAnimation(boardIndex * 0.08f);

                    bombCards.Add(word.id);

                    _cardsById[word.id] = newCard; // Add the card to the Dict associated with it's ID

                    _boardLayoutForClients.Add(new CardStateInfo
                    {
                        id = word.id,
                        word = language,
                        index = boardIndex,
                        owner = ownerString
                    });

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
    public void SetClue(string clueWord, int clueNumber, string team)
    {
        var clueTeam = team == "red" ? Team.red : Team.blue;

        if (_currentStatus != Status.WaitingForClue || clueTeam != _currentTeam) // Making sure nothing buggy will send clues
        {
            Debug.LogWarning($"Ignoring clue from {team} while status={_currentStatus}, currentTeam={_currentTeam}");
            return;
        }

        if (_clueText == null || _clueNumber == null) // Make sure the clue texts are assigned
        {
            Debug.LogWarning("GameManager: clue text fields are missing.");
            return;
        }

        // Set in the UI
        _clueText.text = clueWord;
        if (clueNumber < 10)
            _clueNumber.text = clueNumber.ToString();
        else
            _clueNumber.text = "inf";

        // Set the rount initial stats
        _currentTeam = team == "red" ? Team.red : Team.blue;

        _guessesRemaining = clueNumber + 1;

        _currentStatus = Status.WaitingForGuesses;

        NetworkManager.Instance.AfterStatusChange();

    }
    public void OnCardHighlight(string cardId, string team, bool highlighted)
    {
        if (!_cardsById.TryGetValue(cardId, out var card))
        {
            Debug.LogWarning($"OnCardHighlight: no card with id {cardId}");
            return;
        }

        card.ToggleHighlight(highlighted);
    }

    // Handling the guess
    public void HandleGuess(string cardId, string team)
    {
        if (_currentStatus != Status.WaitingForGuesses) // Making sure we're at the right phase
        {
            Debug.LogWarning($"HandleGuess ignored: status={_currentStatus}");
            return;
        }

        if ((_currentTeam == Team.red && team != "red") || // Making sure the correct team is guessing
        (_currentTeam == Team.blue && team != "blue"))
        {
            Debug.LogWarning($"HandleGuess ignored: team {team} tried to guess on {_currentTeam}'s turn");
            return;
        }

        if (_guessesRemaining <= 0)
        {
            Debug.LogWarning("HandleGuess ignored: no guesses remaining.");
            return;
        }

        if (!_cardsById.TryGetValue(cardId, out var cardGuessed))
        {
            Debug.LogWarning($"HandleGuess: unknown cardId {cardId}");
            return;
        }

        cardGuessed.RevealCard();

        string cardOwner = cardGuessed.GetTeamAsString();

        _wasBombPressed = (cardOwner == "bomb");

        _guessesRemaining = Mathf.Max(0, _guessesRemaining - 1); 

        WinResult result = UpdateScoreAndCheckWinner(team, cardOwner);

        // Tell the phones this card is now gone
        NetworkManager.Instance.SendCardRevealed(cardId);

        if (result.hasWinner)
        {
            StopGame(result);
            return;
        }

        if (_currentStatus == Status.WaitingForClue)
        {
            NetworkManager.Instance.AfterStatusChange();
            return;
        }

        if (_guessesRemaining <= 0)
        {
            EndRound();
        }
        else
        {
            NetworkManager.Instance.AfterStatusChange();
        }
    }

    // Updates the current score, and checks if there's a winner
    private WinResult UpdateScoreAndCheckWinner(string currentTeam, string cardOwner)
    {
        var result = new WinResult { hasWinner = false, reason = "" };

        if (_wasBombPressed)
        {
            result.hasWinner = true;
            result.reason = "bomb";
            result.winningTeam = result.winningTeam = (currentTeam == "red") ? "blue" : "red";
            return result;
        }

        if (cardOwner == "neutral")
        {
            SwitchTurnToOtherTeam(); // Switch to the other team if clicked a neutral card
        }

        if (cardOwner == "red")
        {
            _redTeamCards--;
            if (_redTeamCards <= 0)
            {
                result.hasWinner = true;
                result.reason = "cards";
                result.winningTeam = "red";
            }

            if(_currentTeam != Team.red)
            {
                SwitchTurnToOtherTeam(); // Switch to the other team if clicked the wrong team's card
            }
        }
        else if (cardOwner == "blue")
        {
            _blueTeamCards--;
            if (_blueTeamCards <= 0)
            {
                result.hasWinner = true;
                result.reason = "cards";
                result.winningTeam = "blue";
            }

            if (_currentTeam != Team.blue)
            {
                SwitchTurnToOtherTeam(); // Switch to the other team if clicked the wrong team's card
            }
        }

        _redCards.text = _redTeamCards.ToString();
        _blueCards.text = _blueTeamCards.ToString();

        return result;
    }

    // Switch the turn to the other team
    private void SwitchTurnToOtherTeam()
    {
        if (_currentStatus == Status.GameOver)
            return;

        ClearAllHighlights(); // Clear the highlights from the Unity board

        // Switch team
        _currentTeam = (_currentTeam == Team.red) ? Team.blue : Team.red;

        // Waiting for the new clue
        _currentStatus = Status.WaitingForClue;

        // No guesses, until a new clue is given
        _guessesRemaining = 0;

        if (_clueText != null)
            _clueText.text = $"Waiting for a {_currentTeam} clue....";

        if (_clueNumber != null)
            _clueNumber.text = "?";

        // Send the phones the new state
        NetworkManager.Instance.AfterStatusChange();

        // Make the phones clear their highlights too
        NetworkManager.Instance.SendClearHighlights();
    }

    // A fucntion for ending a round - incase I'd like to add anything else
    private void EndRound()
    {
        SwitchTurnToOtherTeam();
    }

    public void OnEndGuessing(string team)
    {
        // Only on status WaitingForGuess, a guess can be submited
        if (_currentStatus != Status.WaitingForGuesses)
        {
            Debug.LogWarning($"OnEndGuessing ignored: status={_currentStatus}");
            return;
        }

        // Only the active team can end their guessing
        if ((_currentTeam == Team.red && team != "red") ||
            (_currentTeam == Team.blue && team != "blue"))
        {
            Debug.LogWarning($"OnEndGuessing ignored: team {team} tried to end {_currentTeam}'s turn");
            return;
        }

        Debug.Log("OnEndGuessing: ending round early by player request.");
        EndRound();
    }

    private void StopGame(WinResult winner)
    {
        Debug.Log($"Game Over! Winner: {winner.winningTeam}, reason: {winner.reason}");

        _currentStatus = Status.GameOver;
        _currentTeam = winner.winningTeam == "red" ? Team.red : Team.blue;


        foreach (var card in _cardsById)
        {
            card.Value.RevealCard();
        }

        _clueObject.SetActive(false);
        _endScreenText.text = $"Game Over!\n The {winner.winningTeam} Team WON";
        _gameOverObject.SetActive(true);

        // Send final turn state (phase = GameOver)
        NetworkManager.Instance.AfterStatusChange();

        // Tell phones who won and why
        NetworkManager.Instance.SendGameOver(winner);
    }

    // Clear all the remaining highlights from the words
    private void ClearAllHighlights()
    {
        foreach (var kvp in _cardsById)
        {
            kvp.Value.ToggleHighlight(false);
        }
    }

    // Start the game over
    public void PlayAgain()
    {
        // Clear the board
        if (_boardParent != null)
        {
            foreach (Transform child in _boardParent)
            {
                Destroy(child.gameObject);
            }
        }

        // Reset all the parameters
        _cardsById.Clear();
        _boardLayoutForClients.Clear();

        _currentTeam = Team.red;
        _currentStatus = Status.WaitingForClue;
        _guessesRemaining = 0;
        _wasBombPressed = false;

        _redTeamCards = 0;
        _blueTeamCards = 0;

        _clueObject.SetActive(true);
        _gameOverObject.SetActive(false);
        _redCards.text = 9.ToString();
        _blueCards.text = 8.ToString();

        // Reset UI
        if (_clueText != null)
            _clueText.text = "Waiting for a clue....";

        if (_clueNumber != null)
            _clueNumber.text = "?";

        // Create the board
        CreateBoard();

        // Inform the server we started again
        NetworkManager.Instance.AfterStatusChange();
    }

    private static string ReverseForUnity(string s)
    {
        return new string(s.Reverse().ToArray());
    }
}
