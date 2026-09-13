using UnityEngine;

public class BackButtonHandler : MonoBehaviour
{
    public string targetScene = "HomeScene";

    public void GoBack()
    {
        SceneTransition.Instance.LoadScene(targetScene);
    }
}