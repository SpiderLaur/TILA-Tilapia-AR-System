using UnityEngine;
using UnityEngine.InputSystem;

public class TapDetector : MonoBehaviour
{
    public Camera arCamera;
    [SerializeField] private InfoPanelController infoPanelController;

    // Maximum movement (in pixels) to still count as a tap
    public float tapThreshold = 15f;

    private Vector2 startPosition;
    private bool isTouching = false;

    void Update()
    {
        HandleMouse();
        HandleTouch();
    }

    void HandleMouse()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPosition = Mouse.current.position.ReadValue();
            isTouching = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isTouching)
        {
            Vector2 endPosition = Mouse.current.position.ReadValue();

            if (Vector2.Distance(startPosition, endPosition) < tapThreshold)
            {
                CheckTap(endPosition);
            }

            isTouching = false;
        }
    }

    void HandleTouch()
    {
        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            startPosition = touch.position.ReadValue();
            isTouching = true;
        }

        if (touch.press.wasReleasedThisFrame && isTouching)
        {
            Vector2 endPosition = touch.position.ReadValue();

            if (Vector2.Distance(startPosition, endPosition) < tapThreshold)
            {
                CheckTap(endPosition);
            }

            isTouching = false;
        }
    }

    void CheckTap(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            BodyPartInfo info = hit.collider.GetComponent<BodyPartInfo>();

            if (info != null)
            {
                infoPanelController.Show(info.partName, info.description, info.voiceClip, info.linkedPopupModel);
            }
        }
    }
}