using UnityEngine;

public class UIMenuDrift : MonoBehaviour
{
    [SerializeField] private RectTransform target;

    [Header("Drift in UI pixels")]
    [SerializeField] private Vector2 driftAmount = new Vector2(6f, 4f);
    [SerializeField] private float driftSpeed = 0.08f;

    [Header("Optional rotation")]
    [SerializeField] private float rotationAmount = 0.3f;
    [SerializeField] private float rotationSpeed = 0.05f;

    [Header("Menu Safe")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector2 startPos;
    private float startRot;
    private float seedX;
    private float seedY;
    private float seedR;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        startPos = target.anchoredPosition;
        startRot = target.localEulerAngles.z;

        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
        seedR = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        float x = (Mathf.PerlinNoise(seedX, t * driftSpeed) - 0.5f) * 2f * driftAmount.x;
        float y = (Mathf.PerlinNoise(seedY, t * driftSpeed) - 0.5f) * 2f * driftAmount.y;
        float r = (Mathf.PerlinNoise(seedR, t * rotationSpeed) - 0.5f) * 2f * rotationAmount;

        target.anchoredPosition = startPos + new Vector2(x, y);
        target.localRotation = Quaternion.Euler(0f, 0f, startRot + r);
    }
}