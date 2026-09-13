using UnityEngine;

// Attach directly to Heart_Popup, alongside its own ModelGestureControl
// (clampYaw/clampPitch left off so it rotates freely).
//
// GameObject.SetActive always raises OnEnable/OnDisable, so this reacts the
// same way regardless of what flipped Heart_Popup's active state - the View
// 3D button's toggle, ViewModeController forcing it off on a tab switch, or
// InfoPanelController forcing it off when the panel closes.
public class HeartPopupGestureRedirect : MonoBehaviour
{
    [Tooltip("Internal model group's ModelGestureControl. Reset to its default rotation and frozen while Heart_Popup is active; unfrozen (rotation left as-is) when Heart_Popup deactivates.")]
    public ModelGestureControl internalGestureControl;

    [Tooltip("Drag all 6 buttons here: Btn_RotateUp, Btn_RotateDown, Btn_RotateLeft, Btn_RotateRight, Btn_ZoomIn, Btn_ZoomOut. Redirected to Heart_Popup while active, restored to Internal when it deactivates.")]
    public ButtonHoldInput[] holdButtons;

    private ModelGestureControl popupGestureControl;

    void Awake()
    {
        popupGestureControl = GetComponent<ModelGestureControl>();
    }

    void OnEnable()
    {
        if (internalGestureControl != null)
        {
            internalGestureControl.ResetToInitialRotation();
            internalGestureControl.enabled = false;
        }

        RetargetButtons(popupGestureControl);
    }

    void OnDisable()
    {
        if (internalGestureControl != null)
            internalGestureControl.enabled = true;

        RetargetButtons(internalGestureControl);
    }

    void RetargetButtons(ModelGestureControl target)
    {
        if (holdButtons == null) return;
        foreach (var btn in holdButtons)
        {
            if (btn != null) btn.gestureControl = target;
        }
    }
}
