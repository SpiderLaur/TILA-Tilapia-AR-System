using UnityEngine;
using TMPro;

public class ViewModeController : MonoBehaviour
{
    public enum ViewMode { External, Internal, Skeletal }

    [Header("Model Groups")]
    public GameObject externalGroup;
    public GameObject internalGroup;
    public GameObject skeletalGroup;

    [Header("Tab Backgrounds")]
    public UnityEngine.UI.Image tabExternalBg;
    public UnityEngine.UI.Image tabInternalBg;
    public UnityEngine.UI.Image tabSkeletalBg;

    [Header("Tab Text Labels")]
    public TMP_Text tabExternalText;
    public TMP_Text tabInternalText;
    public TMP_Text tabSkeletalText;

    [Header("Colors")]
    public Color activeBgColor = new Color(0.22f, 0.54f, 0.87f); // blue
    public Color inactiveBgColor = new Color(1f, 1f, 1f, 0.15f);
    public Color activeTextColor = Color.white;
    public Color inactiveTextColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("Rotate / Zoom Buttons")]
    [Tooltip("Drag all 6 buttons here: Btn_RotateUp, Btn_RotateDown, Btn_RotateLeft, Btn_RotateRight, Btn_ZoomIn, Btn_ZoomOut")]
    public ButtonHoldInput[] holdButtons;

    [Header("Label Managers")]
    [Tooltip("Drag the LabelManager GameObject that targets the External model")]
    public ShowAllLabels externalLabelManager;
    [Tooltip("Drag the LabelManager GameObject that targets the Internal model (leave empty if not set up yet)")]
    public ShowAllLabels internalLabelManager;
    [Tooltip("Drag the LabelManager_Skeletal GameObject")]
    public ShowAllLabels skeletalLabelManager;

    [Header("Show All Buttons (per tab)")]
    [Tooltip("Show All Parts button shown only while the External tab is active. Leave empty if not split out yet.")]
    public GameObject externalShowAllButton;
    [Tooltip("Show All Parts button shown only while the Internal tab is active. Leave empty if not split out yet.")]
    public GameObject internalShowAllButton;
    [Tooltip("Show All Parts button shown only while the Skeletal tab is active. Leave empty if not split out yet.")]
    public GameObject skeletalShowAllButton;

    [Header("Close Labels Buttons (per tab)")]
    [Tooltip("Drag the Close button used for the External tab. Starts hidden.")]
    public GameObject closeLabelsButton_External;
    [Tooltip("Drag the Close button used for the Internal tab. Starts hidden.")]
    public GameObject closeLabelsButton_Internal;
    [Tooltip("Drag the Close button used for the Skeletal tab. Starts hidden.")]
    public GameObject closeLabelsButton_Skeletal;

    [Header("Info Panel Cleanup")]
    [Tooltip("Drag the info panel's InfoPanelController here so switching tabs closes it, regardless of which part it was showing.")]
    public InfoPanelController infoPanelController;
    [Tooltip("Drag View3DButton here so switching tabs always forces it inactive.")]
    public GameObject view3DButton;

    private ViewMode currentMode = ViewMode.External;
    private bool isLabelsShowing = false;

    void Start()
    {
        SetMode(ViewMode.External);
    }

    public void SetExternal() => SetMode(ViewMode.External);
    public void SetInternal() => SetMode(ViewMode.Internal);
    public void SetSkeletal() => SetMode(ViewMode.Skeletal);

    void SetMode(ViewMode mode)
    {
        currentMode = mode;

        // Switching tabs always clears any open info panel (and whichever
        // organ's popup model / the View 3D button along with it), regardless
        // of which organ's panel was open or which tab we're going to.
        // InfoPanelController.Hide() already tracks and hides whatever popup
        // is currently active, so no per-organ reference is needed here.
        if (infoPanelController != null) infoPanelController.Hide();
        if (view3DButton != null) view3DButton.SetActive(false);

        // Clear any labels left over from whichever tab was active before,
        // so switching tabs always starts with a clean slate.
        if (externalLabelManager != null) externalLabelManager.HideAll();
        if (internalLabelManager != null) internalLabelManager.HideAll();
        if (skeletalLabelManager != null) skeletalLabelManager.HideAll();

        isLabelsShowing = false;
        if (closeLabelsButton_External != null) closeLabelsButton_External.SetActive(false);
        if (closeLabelsButton_Internal != null) closeLabelsButton_Internal.SetActive(false);
        if (closeLabelsButton_Skeletal != null) closeLabelsButton_Skeletal.SetActive(false);

        if (externalGroup != null) externalGroup.SetActive(mode == ViewMode.External);
        if (internalGroup != null) internalGroup.SetActive(mode == ViewMode.Internal);
        if (skeletalGroup != null) skeletalGroup.SetActive(mode == ViewMode.Skeletal);

        UpdateTab(tabExternalBg, tabExternalText, mode == ViewMode.External);
        UpdateTab(tabInternalBg, tabInternalText, mode == ViewMode.Internal);
        UpdateTab(tabSkeletalBg, tabSkeletalText, mode == ViewMode.Skeletal);

        if (externalShowAllButton != null) externalShowAllButton.SetActive(mode == ViewMode.External);
        if (internalShowAllButton != null) internalShowAllButton.SetActive(mode == ViewMode.Internal);
        if (skeletalShowAllButton != null) skeletalShowAllButton.SetActive(mode == ViewMode.Skeletal);

        RetargetHoldButtons(mode);
    }

    void RetargetHoldButtons(ViewMode mode)
    {
        if (holdButtons == null || holdButtons.Length == 0) return;

        ModelGestureControl activeControl = null;

        switch (mode)
        {
            case ViewMode.External:
                if (externalGroup != null) activeControl = externalGroup.GetComponent<ModelGestureControl>();
                break;
            case ViewMode.Internal:
                if (internalGroup != null) activeControl = internalGroup.GetComponent<ModelGestureControl>();
                break;
            case ViewMode.Skeletal:
                if (skeletalGroup != null) activeControl = skeletalGroup.GetComponent<ModelGestureControl>();
                break;
        }

        foreach (var btn in holdButtons)
        {
            if (btn != null) btn.gestureControl = activeControl;
        }
    }

    GameObject GetCurrentCloseButton()
    {
        switch (currentMode)
        {
            case ViewMode.External: return closeLabelsButton_External;
            case ViewMode.Internal: return closeLabelsButton_Internal;
            case ViewMode.Skeletal: return closeLabelsButton_Skeletal;
        }
        return null;
    }

    // Call this from ShowAllPartsButton's OnClick() instead of calling
    // each LabelManager directly. It only toggles labels for whichever
    // model group is currently visible.
    public void ToggleShowAllForCurrentMode()
    {
        switch (currentMode)
        {
            case ViewMode.External:
                if (externalLabelManager != null) externalLabelManager.ToggleShowAll();
                break;
            case ViewMode.Internal:
                if (internalLabelManager != null) internalLabelManager.ToggleShowAll();
                break;
            case ViewMode.Skeletal:
                if (skeletalLabelManager != null) skeletalLabelManager.ToggleShowAll();
                break;
        }

        isLabelsShowing = !isLabelsShowing;
        GameObject closeBtn = GetCurrentCloseButton();
        if (closeBtn != null) closeBtn.SetActive(isLabelsShowing);
    }

    // Call this from the X/close button. Unlike ToggleShowAllForCurrentMode(),
    // this always hides — it never turns labels back on — so it's safe as a
    // dedicated "close" action regardless of current state.
    public void HideAllLabelsForCurrentMode()
    {
        switch (currentMode)
        {
            case ViewMode.External:
                if (externalLabelManager != null) externalLabelManager.HideAll();
                break;
            case ViewMode.Internal:
                if (internalLabelManager != null) internalLabelManager.HideAll();
                break;
            case ViewMode.Skeletal:
                if (skeletalLabelManager != null) skeletalLabelManager.HideAll();
                break;
        }

        isLabelsShowing = false;
        GameObject closeBtn = GetCurrentCloseButton();
        if (closeBtn != null) closeBtn.SetActive(false);
    }

    void UpdateTab(UnityEngine.UI.Image bg, TMP_Text label, bool isActive)
    {
        if (bg != null) bg.color = isActive ? activeBgColor : inactiveBgColor;
        if (label != null) label.color = isActive ? activeTextColor : inactiveTextColor;
    }
}