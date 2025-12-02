using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CardOwner { Red, Blue, Neutral, Bomb } // An enum for the card type

public class Card : MonoBehaviour
{
    private string _id;
    private CardOwner _owner;
    [SerializeField] private TextMeshProUGUI _word;
    [SerializeField] private Image _spriteRenderer;

    public void Init(string id, CardOwner owner, string word) // Card.Init(id, owner, word) in Game Manager
    {
        _id = id;
        _owner = owner;
        _word.text = word;
    }

    public void RevealCard()
    {
        switch (_owner)
        {
            case CardOwner.Red:
                _spriteRenderer.color = Color.red;
                break;

            case CardOwner.Blue:
                _spriteRenderer.color = Color.blue;
                break;

            case CardOwner.Neutral:
                _spriteRenderer.color = new Color32(228, 205, 160, 255);
                break;

            case CardOwner.Bomb:
                _spriteRenderer.color = Color.black;
                _word.color = Color.white;
                break;
        }
    }

    public void Start()
    {
        RevealCard();
    }
}
