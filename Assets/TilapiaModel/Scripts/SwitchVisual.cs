using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SwitchVisual : MonoBehaviour
{
    [Header("References")]
    public RectTransform handle;
    public Image trackImage;

    [Header("Colors")]
    public Color onColor = new Color(0.2f, 0.6f, 1f);
    public Color offColor = new Color(0.3f, 0.3f, 0.35f);

    [Header("Handle Local X Positions")]
    public float onPosX = 35f;
    public float offPosX = -35f;

    [Header("Animation Speed")]
    public float animSpeed = 8f;

    private Toggle toggle;
    private float targetX;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void Start()
    {
        // Snap instantly to correct starting position (no animation on load)
        ApplyState(toggle.isOn, instant: true);
    }

    void OnToggleChanged(bool isOn)
    {
        ApplyState(isOn, instant: false);
    }

    void ApplyState(bool isOn, bool instant)
    {
        targetX = isOn ? onPosX : offPosX;
        trackImage.color = isOn ? onColor : offColor;

        if (instant)
        {
            Vector2 pos = handle.anchoredPosition;
            pos.x = targetX;
            handle.anchoredPosition = pos;
        }
    }

    void Update()
    {
        Vector2 pos = handle.anchoredPosition;
        if (Mathf.Abs(pos.x - targetX) > 0.1f)
        {
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * animSpeed);
            handle.anchoredPosition = pos;
        }
    }
}
