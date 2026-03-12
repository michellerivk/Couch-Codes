using System.Collections;
using TMPro;
using UnityEngine;

public class StartGameIdle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private RectTransform labelRect;

    [Header("Arrow Nudge")]
    [SerializeField] private float nudgeDistance = 7f;
    [SerializeField] private float nudgeDuration = 0.18f;
    [SerializeField] private Vector2 pauseRange = new Vector2(1.8f, 3.0f);

    [Header("Label Pulse")]
    [SerializeField] private float pulseAmount = 0.012f;
    [SerializeField] private float pulseSpeed = 0.9f;

    [Header("Menu Safe")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector2 arrowStartPos;
    private Vector3 labelStartScale;
    private Coroutine arrowRoutine;

    private void Awake()
    {
        if (arrowRect != null)
            arrowStartPos = arrowRect.anchoredPosition;

        if (labelRect != null)
            labelStartScale = labelRect.localScale;
    }

    private void OnEnable()
    {
        arrowRoutine = StartCoroutine(ArrowLoop());
    }

    private void OnDisable()
    {
        if (arrowRoutine != null)
            StopCoroutine(arrowRoutine);

        if (arrowRect != null)
            arrowRect.anchoredPosition = arrowStartPos;

        if (labelRect != null)
            labelRect.localScale = labelStartScale;
    }

    private void Update()
    {
        if (labelRect == null)
            return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float scale = 1f + Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f) * pulseAmount;

        labelRect.localScale = labelStartScale * scale;
    }

    private IEnumerator ArrowLoop()
    {
        while (true)
        {
            float pause = Random.Range(pauseRange.x, pauseRange.y);
            yield return Wait(pause);

            yield return MoveArrow(
                arrowStartPos,
                arrowStartPos + Vector2.right * nudgeDistance,
                nudgeDuration
            );

            yield return MoveArrow(
                arrowRect.anchoredPosition,
                arrowStartPos,
                nudgeDuration * 1.15f
            );
        }
    }

    private IEnumerator MoveArrow(Vector2 from, Vector2 to, float duration)
    {
        if (arrowRect == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += DeltaTime();
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            arrowRect.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }

        arrowRect.anchoredPosition = to;
    }

    private float DeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private object Wait(float seconds)
    {
        return useUnscaledTime
            ? new WaitForSecondsRealtime(seconds)
            : new WaitForSeconds(seconds);
    }
}