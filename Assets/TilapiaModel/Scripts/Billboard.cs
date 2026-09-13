using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;

    void LateUpdate()
    {
        if (targetCamera == null) return;
        transform.rotation = Quaternion.LookRotation(
            transform.position - targetCamera.transform.position,
            targetCamera.transform.up
        );
    }
}