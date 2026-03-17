using System;
using System.Threading;
using System.Threading.Tasks;
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
    [SerializeField] private Image _middleRenderer;
    [SerializeField] private Image _outsideRenderer;
    //[SerializeField] private Image _highlightFrame;

    [Header("Cute Highlight")]
    [SerializeField] private Material _highlightMaterialTemplate;
    [SerializeField] private Color _highlightColor = new Color(1f, 0.92f, 0.60f, 1f);
    [SerializeField] private float _highlightScale = 1.03f;
    [SerializeField] private float _scaleAnimDuration = 0.12f;
    [SerializeField] private Animator _anim;

    private Material _runtimeMaterial;
    private Vector3 _baseScale;

    private CancellationTokenSource _scaleCts;

    private static readonly int HighlightOnID = Shader.PropertyToID("_HighlightOn");
    private static readonly int HighlightColorID = Shader.PropertyToID("_HighlightColor");

    private bool _isRevealed = false;

    private void Awake()
    {
        if (_anim != null)
            _anim.enabled = false;

        _baseScale = transform.localScale;

        transform.localScale = Vector3.zero;

        if (_spriteRenderer != null && _highlightMaterialTemplate != null)
        {
            _runtimeMaterial = new Material(_highlightMaterialTemplate);
            _spriteRenderer.material = _runtimeMaterial;

            _runtimeMaterial.SetFloat(HighlightOnID, 0f);
            _runtimeMaterial.SetColor(HighlightColorID, _highlightColor);
        }
    }

    private void OnDestroy()
    {
        CancelScaleAnimation();

        if (_runtimeMaterial != null)
            Destroy(_runtimeMaterial);
    }

    public void Init(string id, CardOwner owner, string word) // Card.Init(id, owner, word) in Game Manager
    {
        _id = id;
        _owner = owner;
        _word.text = word;
    }

    public void RevealCard()
    {
        _isRevealed = true;

        switch (_owner)
        {
            case CardOwner.Red:
                _spriteRenderer.color = new Color32(198, 82, 81, 255);
                _middleRenderer.color = new Color32(167, 44, 20, 255);
                _outsideRenderer.color = new Color32(198, 82, 81, 255);
                break;

            case CardOwner.Blue:
                _spriteRenderer.color = new Color32(80, 107, 197, 255);
                _middleRenderer.color = new Color32(20, 65, 166, 255);
                _outsideRenderer.color = new Color32(80, 107, 197, 255);
                break;

            case CardOwner.Neutral:
                _spriteRenderer.color = new Color32(228, 205, 160, 255);
                _middleRenderer.color = new Color32(206, 164, 95, 255);
                _outsideRenderer.color = new Color32(228, 205, 160, 255);
                break;

            case CardOwner.Bomb:
                _spriteRenderer.color = Color.black;
                _middleRenderer.color = new Color32(86, 73, 79, 255);
                _outsideRenderer.color = Color.black;
                _word.color = Color.white;
                break;
        }
    }

    public void ToggleHighlight(bool on)
    {
        if (!_isRevealed)
        {
            _spriteRenderer.color = on
                ? new Color32(199, 171, 93, 255)
                : new Color32(216, 182, 127, 255);
        }

        if (_runtimeMaterial != null)
        {
            _runtimeMaterial.SetFloat(HighlightOnID, on ? 1f : 0f);
            _runtimeMaterial.SetColor(HighlightColorID, _highlightColor);
        }

        Vector3 targetScale = on ? _baseScale * _highlightScale : _baseScale;
        StartScaleAnimation(targetScale, _scaleAnimDuration);
    }

    private void StartScaleAnimation(Vector3 targetScale, float duration)
    {
        CancelScaleAnimation();

        _scaleCts = new CancellationTokenSource();
        _ = AnimateScaleAsync(targetScale, duration, _scaleCts.Token);
    }

    private void CancelScaleAnimation()
    {
        if (_scaleCts != null)
        {
            _scaleCts.Cancel();
            _scaleCts.Dispose();
            _scaleCts = null;
        }
    }

    private async Task AnimateScaleAsync(Vector3 targetScale, float duration, CancellationToken token)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                if (this == null || gameObject == null)
                    return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, eased);

                await Task.Yield();
            }

            if (this != null && gameObject != null)
                transform.localScale = targetScale;
        }
        catch (OperationCanceledException)
        {
            // Expected when highlight is toggled rapidly or object is destroyed.
        }
    }

    public void PlayOpenAnimation(float delay)
    {
        _ = PlayOpenAnimationAsync(delay);
    }

    public async Task PlayOpenAnimationAsync(float delay)
    {
        if (delay > 0f)
            await Task.Delay(Mathf.RoundToInt(delay * 1000f));

        if (this == null || gameObject == null || _anim == null)
            return;

        _anim.enabled = true;
        _anim.Play("CardOpens", 0, 0f);
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
