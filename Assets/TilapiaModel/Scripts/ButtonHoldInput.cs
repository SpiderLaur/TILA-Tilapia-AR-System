using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoldInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum HoldTarget
    {
        RotateUp,
        RotateDown,
        RotateLeft,
        RotateRight,
        ZoomIn,
        ZoomOut
    }

    [Header("Setup")]
    public ModelGestureControl gestureControl;
    public HoldTarget targetFlag;

    [Header("Text Feedback")]
    public TMP_Text label;
    public Color normalTextColor = Color.white;
    public Color pressedTextColor = new Color(0.85f, 0.93f, 1f);

    public void OnPointerDown(PointerEventData eventData)
    {
        SetFlag(true);
        if (label != null) label.color = pressedTextColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetFlag(false);
        if (label != null) label.color = normalTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetFlag(false);
        if (label != null) label.color = normalTextColor;
    }

    void SetFlag(bool value)
    {
        if (gestureControl == null) return;

        switch (targetFlag)
        {
            case HoldTarget.RotateUp: gestureControl.holdRotateUp = value; break;
            case HoldTarget.RotateDown: gestureControl.holdRotateDown = value; break;
            case HoldTarget.RotateLeft: gestureControl.holdRotateLeft = value; break;
            case HoldTarget.RotateRight: gestureControl.holdRotateRight = value; break;
            case HoldTarget.ZoomIn: gestureControl.holdZoomIn = value; break;
            case HoldTarget.ZoomOut: gestureControl.holdZoomOut = value; break;
        }
    }
}