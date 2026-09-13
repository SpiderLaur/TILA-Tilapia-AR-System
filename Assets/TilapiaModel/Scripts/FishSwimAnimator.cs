using UnityEngine;
using UnityEngine.UI;

// Attach this to the fish's GameObject (e.g. "Swimfish").
// It reads the loading Slider's value each frame and moves the fish
// horizontally to match, with a bit of vertical bob and rotational wiggle.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class FishSwimAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Slider driving the loading bar (drag your LoadingBar object here).")]
    public Slider loadingSlider;

    [Tooltip("The Slider's RectTransform, used to know the bar's width.")]
    public RectTransform barRectTransform;

    [Header("Bob / Wiggle")]
    [Tooltip("How many pixels up/down the fish bobs.")]
    public float bobAmplitude = 4f;

    [Tooltip("How fast the fish bobs (cycles per second).")]
    public float bobSpeed = 3f;

    [Tooltip("How many degrees the fish tilts side to side.")]
    public float wiggleAmplitude = 6f;

    [Tooltip("How fast the fish wiggles (cycles per second).")]
    public float wiggleSpeed = 4f;

    [Header("Positioning")]
    [Tooltip("Extra horizontal offset in pixels (nudge fish forward/back along the bar).")]
    public float xOffset = 0f;

    RectTransform fishRect;
    float baseY;

    void Awake()
    {
        fishRect = GetComponent<RectTransform>();
        baseY = fishRect.anchoredPosition.y;
    }

    void Update()
    {
        if (loadingSlider == null || barRectTransform == null)
            return;

        // Map slider value (0-1) to the bar's width in local space.
        float barWidth = barRectTransform.rect.width;
        float t = Mathf.Clamp01(loadingSlider.value);
        float x = (t * barWidth) - (barWidth * 0.5f) + xOffset;

        // Bob up/down.
        float bob = Mathf.Sin(Time.time * bobSpeed * Mathf.PI * 2f) * bobAmplitude;

        // Slight tilt wiggle, like swimming.
        float wiggle = Mathf.Sin(Time.time * wiggleSpeed * Mathf.PI * 2f) * wiggleAmplitude;

        fishRect.anchoredPosition = new Vector2(x, baseY + bob);
        fishRect.localRotation = Quaternion.Euler(0f, 0f, wiggle);
    }
}