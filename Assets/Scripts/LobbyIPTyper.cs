using System.Collections;
using TMPro;
using UnityEngine;

public class LobbyIPTyper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text displayText;

    [Header("Waiting Text")]
    [SerializeField] private string waitingText = "Waiting for IP...";

    [Header("Timing")]
    [SerializeField] private float typeDelay = 0.05f;
    [SerializeField] private float eraseDelay = 0.03f;
    [SerializeField] private float waitAfterTyped = 0.8f;
    [SerializeField] private float waitAfterErased = 0.25f;
    [SerializeField] private float cursorBlinkDelay = 0.45f;
    [SerializeField] private bool useUnscaledTime = true;

    private bool finalTextRequested;
    private string finalText = "";
    private Coroutine cursorRoutine;

    private void Awake()
    {
        if (displayText == null)
            displayText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (displayText == null)
        {
            Debug.LogError("LobbyIPTyper: displayText is not assigned.");
            enabled = false;
            return;
        }

        StartCoroutine(WaitingLoop());
    }

    public void SetFullText(string textToDisplay)
    {
        finalText = textToDisplay;
        finalTextRequested = true;
    }

    private IEnumerator WaitingLoop()
    {
        while (!finalTextRequested)
        {
            yield return TypeText(waitingText);

            if (finalTextRequested)
                break;

            yield return Wait(waitAfterTyped);
            yield return EraseText(waitingText);

            if (finalTextRequested)
                break;

            yield return Wait(waitAfterErased);
        }

        yield return TypeFinalText();
    }

    private IEnumerator TypeText(string textToType)
    {
        for (int i = 0; i <= textToType.Length; i++)
        {
            if (finalTextRequested)
                yield break;

            displayText.text = textToType.Substring(0, i) + "_";
            yield return Wait(typeDelay);
        }
    }

    private IEnumerator EraseText(string textToErase)
    {
        for (int i = textToErase.Length; i >= 0; i--)
        {
            if (finalTextRequested)
                yield break;

            displayText.text = textToErase.Substring(0, i) + "_";
            yield return Wait(eraseDelay);
        }
    }

    private IEnumerator TypeFinalText()
    {
        displayText.text = "";

        for (int i = 0; i <= finalText.Length; i++)
        {
            displayText.text = finalText.Substring(0, i) + "_";
            yield return Wait(typeDelay);
        }

        if (cursorRoutine != null)
            StopCoroutine(cursorRoutine);

        cursorRoutine = StartCoroutine(BlinkCursor());
    }

    private IEnumerator BlinkCursor()
    {
        bool showCursor = true;

        while (true)
        {
            displayText.text = finalText + (showCursor ? "_" : "");
            showCursor = !showCursor;
            yield return Wait(cursorBlinkDelay);
        }
    }

    private object Wait(float seconds)
    {
        return useUnscaledTime
            ? new WaitForSecondsRealtime(seconds)
            : new WaitForSeconds(seconds);
    }
}