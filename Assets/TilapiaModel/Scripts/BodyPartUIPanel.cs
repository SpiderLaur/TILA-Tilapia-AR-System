using UnityEngine;
using TMPro;

public class BodyPartUIPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Style (applies to every body part)")]
    [SerializeField] private float titleFontSize = 24f;
    [SerializeField] private float descriptionFontSizeMax = 16f;
    [SerializeField] private float descriptionFontSizeMin = 10f;
    [SerializeField] private Color titleColor = new Color(0.55f, 0.78f, 1f);
    [SerializeField] private Color descriptionColor = new Color(0.85f, 0.85f, 0.85f);

    public void ShowInfo(string partName, string description)
    {
        titleText.text = partName;
        titleText.fontSize = titleFontSize;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = titleColor;

        descriptionText.text = description;
        descriptionText.color = descriptionColor;
        descriptionText.enableAutoSizing = true;
        descriptionText.fontSizeMax = descriptionFontSizeMax;
        descriptionText.fontSizeMin = descriptionFontSizeMin;

        panelRoot.SetActive(true);
    }

    public void HideInfo()
    {
        panelRoot.SetActive(false);
    }
}