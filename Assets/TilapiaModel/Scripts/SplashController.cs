using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider loadingBar;

    [Header("Timing")]
    [Tooltip("Minimum seconds the splash screen stays visible, even if loading finishes faster.")]
    [SerializeField] private float minimumDisplayTime = 3f;

    [Header("Scene To Load")]
    [SerializeField] private string nextSceneName = "HomeScene";

    private void Start()
    {
        loadingBar.value = 0f;
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        float elapsedTime = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;

            float sceneLoadProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumDisplayTime);

            loadingBar.value = Mathf.Min(sceneLoadProgress, timeProgress);

            bool minimumTimeReached = elapsedTime >= minimumDisplayTime;
            bool sceneReady = asyncLoad.progress >= 0.9f;

            if (minimumTimeReached && sceneReady)
            {
                loadingBar.value = 1f;
                yield return new WaitForSeconds(0.3f);

                if (SceneTransition.Instance != null)
                    yield return StartCoroutine(SceneTransition.Instance.FadeToBlack());

                asyncLoad.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}