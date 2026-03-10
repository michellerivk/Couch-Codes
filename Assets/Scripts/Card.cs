using System.Collections;
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
    [SerializeField] private Image _highlightFrame;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Coroutine _spawnRoutine;

    private Vector3 _currentScale = new Vector3 (1.2f, 1.2f, 1);
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(string id, CardOwner owner, string word) // Card.Init(id, owner, word) in Game Manager
    {
        _id = id;
        _owner = owner;
        _word.text = word;
    }

    public void PrepareForSpawn()
    {
        if (_rectTransform != null)
            _rectTransform.localScale = (_currentScale) * 0.8f;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
    }

    public void PlaySpawnAnimation(float duration)
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        _spawnRoutine = StartCoroutine(SpawnAnimation(duration));
    }

    private IEnumerator SpawnAnimation(float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = _currentScale * 0.8f;
        Vector3 endScale = _currentScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (_rectTransform != null)
                _rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);

            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);

            yield return null;

            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFXPitchAdjusted(2);
        }

        if (_rectTransform != null)
            _rectTransform.localScale = _currentScale;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        _spawnRoutine = null;
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

    public void ToggleHighlight(bool on)
    {
        if (_highlightFrame != null)
            _highlightFrame.enabled = on;
    }

    public string GetTeamAsString()
    {
        if (_owner == CardOwner.Bomb)
            return "bomb";

        if (_owner == CardOwner.Red)
            return "red";

        if (_owner == CardOwner.Blue)
            return "blue";

        else
            return "neutral";

    }   
}
