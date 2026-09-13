using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Fade Settings")]
    public float fadeDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private Image fadeImage;
    private bool fadeInPending = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("SceneTransitionManager");
        go.AddComponent<SceneTransition>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildFadeCanvas();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Whenever ANY scene finishes loading while the overlay is still black,
    // automatically fade it back out. Runs on THIS object (persists across
    // scene loads), so it can never be killed by the old scene unloading -
    // unlike a coroutine started from a scene-local object like SplashController.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (canvasGroup != null && canvasGroup.alpha > 0f && !fadeInPending)
        {
            fadeInPending = true;
            StartCoroutine(AutoFadeIn());
        }
    }

    IEnumerator AutoFadeIn()
    {
        yield return null; // let the new scene settle for a frame
        yield return StartCoroutine(Fade(canvasGroup.alpha, 0f));
        canvasGroup.blocksRaycasts = false;
        fadeInPending = false;
    }

    void BuildFadeCanvas()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform, false);
            fadeImage = imageGO.AddComponent<Image>();
            fadeImage.color = Color.black;

            RectTransform rt = fadeImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            canvasGroup = imageGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            fadeImage = canvas.GetComponentInChildren<Image>();
            canvasGroup = fadeImage.GetComponent<CanvasGroup>();
        }
    }

    // Used by buttons (HomeMenuController, BackButtonHandler)
    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        canvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
        // Fade-in is handled automatically by OnSceneLoaded above
    }

    // Used by SplashController before its manually-managed async activation
    public IEnumerator FadeToBlack()
    {
        canvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(Fade(0f, 1f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}