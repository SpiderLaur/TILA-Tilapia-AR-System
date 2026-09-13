using UnityEngine;

public class HomeMenuController : MonoBehaviour
{
    [Header("Exit Confirmation")]
    public GameObject exitConfirmPanel;

    public void OnStartARLearningPressed()
    {
        Debug.Log("Start AR learning pressed - loading ARTilapiaScene");
        SceneTransition.Instance.LoadScene("ARTilapiaScene");
    }

    public void OnSettingsPressed()
    {
        Debug.Log("Settings pressed - loading SettingsScene");
        SceneTransition.Instance.LoadScene("SettingsScene");
    }

    public void OnAboutPressed()
    {
        Debug.Log("About pressed - loading AboutScene");
        SceneTransition.Instance.LoadScene("AboutScene");
    }

    public void OnExitPressed()
    {
        Debug.Log("Exit button pressed - showing confirmation");
        exitConfirmPanel.SetActive(true);
    }

    public void OnExitConfirmPressed()
    {
        Debug.Log("Exit confirmed - closing application");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnExitCancelPressed()
    {
        Debug.Log("Exit cancelled");
        exitConfirmPanel.SetActive(false);
    }
}