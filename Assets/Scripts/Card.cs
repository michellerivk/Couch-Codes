using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private int _id;
    [SerializeField] private CardOwner _owner;
    [SerializeField] private TextMeshProUGUI _word;
    [SerializeField] private Image spriteRenderer;

    public enum CardOwner { Red, Blue, Neutral, Bomb }

    public void Init(int id, CardOwner owner, string word) // Card.Init(id, owner, word) in Game Manager
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
                spriteRenderer.color = Color.red;
                break;

            case CardOwner.Blue:
                spriteRenderer.color = Color.blue;
                break;

            case CardOwner.Neutral:
                spriteRenderer.color = new Color32(228, 205, 160, 255);
                break;

            case CardOwner.Bomb:
                spriteRenderer.color = Color.black;
                _word.color = Color.white;
                break;
        }
    }

    public void Start()
    {
        RevealCard();
    }
}
