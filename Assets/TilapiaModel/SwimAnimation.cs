using UnityEngine;

public class SwimAnimation : MonoBehaviour
{
    [Header("Overall Body Drift")]
    public float bobSpeed = 0.6f;
    public float bobAmount = 0.005f;
    public float tiltAmount = 1.5f;

    private Vector3 startPos;
    private float previousTilt = 0f;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float t = Time.time;

        float bob = Mathf.Sin(t * bobSpeed) * bobAmount;
        float tilt = Mathf.Sin(t * bobSpeed * 0.7f) * tiltAmount;

        // Apply only the CHANGE in tilt since last frame, as a relative rotation.
        // This lets it "ride along" on top of user rotation input instead of
        // resetting the whole model's rotation every frame.
        float deltaTilt = tilt - previousTilt;
        transform.Rotate(Vector3.forward, deltaTilt, Space.Self);
        previousTilt = tilt;

        transform.localPosition = startPos + new Vector3(0f, bob, 0f);
    }
}