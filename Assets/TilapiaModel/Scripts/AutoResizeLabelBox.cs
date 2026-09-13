using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AutoResizeLabelBox : MonoBehaviour
{
    [Header("References")]
    public TMP_Text labelText;
    public RectTransform background;

    [Header("Padding")]
    public float paddingX = 20f;
    public float paddingY = 12f;

    public void ResizeToFitText()
    {
        if (labelText == null || background == null) return;

        labelText.ForceMeshUpdate(true, true);

        Vector2 textSize = labelText.GetRenderedValues(false);

        float width = textSize.x + paddingX;
        float height = textSize.y + paddingY;

        background.sizeDelta = new Vector2(width, height);

        RectTransform textRect = labelText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = textSize;
    }
}