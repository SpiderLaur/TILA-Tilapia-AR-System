using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    public void OnBackPressed()
    {
        Debug.Log("Back pressed - returning to HomeScene");
        SceneTransition.Instance.LoadScene("HomeScene");
    }
}