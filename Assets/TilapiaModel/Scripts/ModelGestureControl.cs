using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ModelGestureControl : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 0.3f;
    public float buttonRotationSpeed = 60f; // degrees per second while a rotate button is held

    [Header("Rotation Limit (Internal cutaway model only)")]
    [Tooltip("Leave unchecked for External/Skeletal so they keep rotating freely. Check this only on Internal's own ModelGestureControl instance.")]
    public bool clampYaw = false;
    [Tooltip("Max degrees of yaw left/right from the model's starting front-facing orientation, when clampYaw is on.")]
    public float maxYawAngle = 85f;
    [Tooltip("Leave unchecked for External/Skeletal so they keep rotating freely. Check this only on Internal's own ModelGestureControl instance.")]
    public bool clampPitch = false;
    [Tooltip("Max degrees of pitch up/down from the model's starting orientation, when clampPitch is on.")]
    public float maxPitchAngle = 85f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.005f;
    public float minScale = 0.5f;
    public float maxScale = 2.5f;
    public float buttonZoomSpeed = 0.5f; // scale units per second while a zoom button is held

    [Header("Sensitivity (from Settings)")]
    [Tooltip("Multiplier applied to all rotation/zoom speeds. Loaded from PlayerPrefs at Start via SettingsController.")]
    public float sensitivityMultiplier = 1f;

    private Vector3 initialScale;
    private Quaternion initialRotation;
    private float initialPinchDistance;
    private bool isPinching = false;
    private float currentYaw = 0f;
    private float currentPitch = 0f;

    // Button hold state - set these from ButtonHoldInput.cs on each UI button
    [HideInInspector] public bool holdRotateUp = false;
    [HideInInspector] public bool holdRotateDown = false;
    [HideInInspector] public bool holdRotateLeft = false;
    [HideInInspector] public bool holdRotateRight = false;
    [HideInInspector] public bool holdZoomIn = false;
    [HideInInspector] public bool holdZoomOut = false;

    void Start()
    {
        initialScale = transform.localScale;
        initialRotation = transform.rotation;

        // Pull the saved AR sensitivity value from Settings (0.5–2 range, default 1)
        sensitivityMultiplier = SettingsController.GetSavedArSensitivity();
    }

    // Snaps back to this object's rotation as of scene load, and clears the
    // running yaw/pitch accumulators so a subsequent clamped rotation (e.g.
    // Internal's) measures from this reset position rather than wherever the
    // accumulators were left when input was last frozen.
    public void ResetToInitialRotation()
    {
        transform.rotation = initialRotation;
        currentYaw = 0f;
        currentPitch = 0f;
    }

    void Update()
    {
        HandleButtonInput();
        HandleMouseInput();
        HandleTouchInput();
    }

    void HandleButtonInput()
    {
        if (holdRotateUp)
            RotatePitch(buttonRotationSpeed * sensitivityMultiplier * Time.deltaTime);
        if (holdRotateDown)
            RotatePitch(-buttonRotationSpeed * sensitivityMultiplier * Time.deltaTime);
        if (holdRotateLeft)
            RotateYaw(buttonRotationSpeed * sensitivityMultiplier * Time.deltaTime);
        if (holdRotateRight)
            RotateYaw(-buttonRotationSpeed * sensitivityMultiplier * Time.deltaTime);

        if (holdZoomIn)
            ApplyZoom(buttonZoomSpeed * sensitivityMultiplier * Time.deltaTime);
        if (holdZoomOut)
            ApplyZoom(-buttonZoomSpeed * sensitivityMultiplier * Time.deltaTime);
    }

    void HandleMouseInput()
    {
        if (Mouse.current == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            RotateYaw(-delta.x * rotationSpeed * sensitivityMultiplier);
            RotatePitch(delta.y * rotationSpeed * sensitivityMultiplier);
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            ApplyZoom(scroll * zoomSpeed * sensitivityMultiplier * 20f);
        }
    }

    void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        var touches = Touchscreen.current.touches;
        int activeCount = 0;
        bool anyTouchOverUI = false;

        foreach (var t in touches)
        {
            if (t.press.isPressed)
            {
                activeCount++;
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(t.touchId.ReadValue()))
                {
                    anyTouchOverUI = true;
                }
            }
        }

        if (anyTouchOverUI)
        {
            isPinching = false;
            return;
        }

        if (activeCount == 1)
        {
            isPinching = false;
            Vector2 delta = Touchscreen.current.primaryTouch.delta.ReadValue();
            RotateYaw(-delta.x * rotationSpeed * sensitivityMultiplier);
            RotatePitch(delta.y * rotationSpeed * sensitivityMultiplier);
        }
        else if (activeCount == 2)
        {
            Vector2 pos0 = Vector2.zero;
            Vector2 pos1 = Vector2.zero;
            int found = 0;

            foreach (var t in touches)
            {
                if (t.press.isPressed)
                {
                    if (found == 0) pos0 = t.position.ReadValue();
                    else if (found == 1) pos1 = t.position.ReadValue();
                    found++;
                }
            }

            float currentDistance = Vector2.Distance(pos0, pos1);

            if (!isPinching)
            {
                initialPinchDistance = currentDistance;
                isPinching = true;
            }
            else
            {
                float difference = currentDistance - initialPinchDistance;
                ApplyZoom(difference * zoomSpeed * sensitivityMultiplier);
                initialPinchDistance = currentDistance;
            }
        }
        else
        {
            isPinching = false;
        }
    }

    // Applies a yaw rotation, clamping the running total to ±maxYawAngle when
    // clampYaw is on (Internal's cutaway model only). Applies only the remaining
    // legal delta once the limit is hit, so rotation stops cleanly at the edge
    // instead of overshooting then snapping back.
    void RotateYaw(float degrees)
    {
        float newYaw = currentYaw + degrees;
        if (clampYaw)
            newYaw = Mathf.Clamp(newYaw, -maxYawAngle, maxYawAngle);

        float appliedDelta = newYaw - currentYaw;
        currentYaw = newYaw;
        if (appliedDelta != 0f)
            transform.Rotate(Vector3.up, appliedDelta, Space.World);
    }

    // Same clamping approach as RotateYaw, for pitch (up/down tilt). Also gated
    // per-instance (clampPitch), so External/Skeletal keep tilting freely.
    void RotatePitch(float degrees)
    {
        float newPitch = currentPitch + degrees;
        if (clampPitch)
            newPitch = Mathf.Clamp(newPitch, -maxPitchAngle, maxPitchAngle);

        float appliedDelta = newPitch - currentPitch;
        currentPitch = newPitch;
        if (appliedDelta != 0f)
            transform.Rotate(Vector3.right, appliedDelta, Space.World);
    }

    void ApplyZoom(float amount)
    {
        // Scale the increment relative to this object's own starting scale,
        // so zoom feels consistent whether the model's base scale is 1 or 0.1.
        float scaledAmount = amount * initialScale.x;
        Vector3 newScale = transform.localScale + Vector3.one * scaledAmount;
        float clamped = Mathf.Clamp(newScale.x, initialScale.x * minScale, initialScale.x * maxScale);
        transform.localScale = Vector3.one * clamped;
    }
}