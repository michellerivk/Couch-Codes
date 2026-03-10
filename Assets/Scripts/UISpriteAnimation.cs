using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 10f;
    [SerializeField] private bool loop = true;

    private Image uiImage;
    private int frameIndex;
    private float timer;

    private void Awake()
    {
        uiImage = GetComponent<Image>();

        if (frames != null && frames.Length > 0)
            uiImage.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || fps <= 0f)
            return;

        timer += Time.deltaTime;
        float frameTime = 1f / fps;

        while (timer >= frameTime)
        {
            timer -= frameTime;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (loop)
                    frameIndex = 0;
                else
                {
                    frameIndex = frames.Length - 1;
                    enabled = false;
                    return;
                }
            }

            uiImage.sprite = frames[frameIndex];
        }
    }
}