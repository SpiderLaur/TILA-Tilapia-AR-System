using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button listenButton;
    [SerializeField] private Button view3DButton;
    [SerializeField] private AudioSource audioSource;

    [Header("Style")]
    [SerializeField] private float titleFontSize = 22f;
    [SerializeField] private float descriptionMaxSize = 15f;
    [SerializeField] private float descriptionMinSize = 10f;

    private AudioClip currentClip;
    private GameObject currentPopupModel;

    // Whichever organ's popup model is currently shown in the 3D scene (or
    // null if none is). Set any time a popup is activated via the View 3D
    // button, so cleanup code elsewhere (e.g. ViewModeController) can hide
    // it without needing to know which organ it belongs to.
    public GameObject ActivePopupModel { get; private set; }

    void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (listenButton != null) listenButton.onClick.AddListener(OnListenPressed);
        if (view3DButton != null) view3DButton.onClick.AddListener(OnView3DPressed);
        if (panelRoot != null) panelRoot.SetActive(false);
        if (view3DButton != null) view3DButton.gameObject.SetActive(false);
    }

    public void Show(string partName, string description, AudioClip clip = null, GameObject popupModel = null)
    {
        // Tapping a new zone must clear out any popup left active from a
        // previously-tapped organ before we swap the panel's content over.
        ClearActivePopup();

        titleText.text = partName;
        titleText.fontSize = titleFontSize;
        titleText.fontStyle = FontStyles.Bold;

        descriptionText.text = description;
        descriptionText.enableAutoSizing = true;
        descriptionText.fontSizeMax = descriptionMaxSize;
        descriptionText.fontSizeMin = descriptionMinSize;

        currentClip = clip;
        currentPopupModel = popupModel;
        if (view3DButton != null) view3DButton.gameObject.SetActive(popupModel != null);

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (audioSource != null) audioSource.Stop();

        ClearActivePopup();

        panelRoot.SetActive(false);
    }

    // Deactivates whichever organ's popup model is currently shown (if any)
    // and restores Internal's ModelGestureControl via the popup's own
    // OnDisable (see HeartPopupGestureRedirect), without closing the info
    // panel itself. Called from Hide() (tab switch) and from Show() (a new
    // zone tapped while a previous organ's popup is still active).
    public void ClearActivePopup()
    {
        if (ActivePopupModel != null)
        {
            ActivePopupModel.SetActive(false);
            ActivePopupModel = null;
        }
    }

    void OnListenPressed()
    {
        if (currentClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(currentClip);
        }
        else
        {
            Debug.Log("No audio clip assigned for: " + titleText.text);
        }
    }

    void OnView3DPressed()
    {
        if (currentPopupModel == null) return;
        bool nowActive = !currentPopupModel.activeSelf;
        currentPopupModel.SetActive(nowActive);
        ActivePopupModel = nowActive ? currentPopupModel : null;
    }
}