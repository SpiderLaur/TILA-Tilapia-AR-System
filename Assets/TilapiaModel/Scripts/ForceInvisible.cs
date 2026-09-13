using UnityEngine;

public class ForceInvisible : MonoBehaviour
{
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (rend != null && rend.enabled)
        {
            rend.enabled = false;
        }
    }
}