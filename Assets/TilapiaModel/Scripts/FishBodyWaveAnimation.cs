using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FishBodyWaveAnimation : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveSpeed = 1.5f;
    public float waveFrequency = 1f;
    public float waveAmplitude = 0.008f;

    [Header("Amplitude Growth Toward Tail")]
    public float amplitudeGrowth = 2.5f;

    [Header("Model Orientation")]
    public Axis bodyAxis = Axis.Z;
    public bool reverseDirection = false;

    [Header("Performance")]
    [Tooltip("Recalculate normals every N frames instead of every frame. 1 = every frame (expensive), 3-5 = much cheaper with negligible visual difference for small wave amplitudes.")]
    public int recalculateNormalsEveryNFrames = 4;
    [Tooltip("Update the wave displacement itself every N frames. 1 = smoothest, 2 = still smooth and halves the cost.")]
    public int updateEveryNFrames = 1;

    public enum Axis { X, Y, Z }

    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private Mesh workingMesh;
    private Vector3[] baseVertices;
    private Vector3[] displacedVertices;

    private float minAxisValue;
    private float maxAxisValue;
    private int frameCounter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;

        if (originalMesh == null)
        {
            Debug.LogError("FishBodyWaveAnimation: No mesh found on " + gameObject.name);
            enabled = false;
            return;
        }

        workingMesh = Instantiate(originalMesh);
        meshFilter.mesh = workingMesh;

        // Mark dynamic so Unity optimizes for frequent vertex updates.
        workingMesh.MarkDynamic();

        baseVertices = originalMesh.vertices;
        displacedVertices = new Vector3[baseVertices.Length];

        // Give the mesh a bit of extra headroom in its bounds up front,
        // sized to the max possible displacement, so we don't need to
        // call RecalculateBounds() every frame at all.
        Bounds b = originalMesh.bounds;
        float maxOffset = waveAmplitude;
        b.Expand(maxOffset * 2f);
        workingMesh.bounds = b;

        CalculateBodyLengthRange();
    }

    void CalculateBodyLengthRange()
    {
        minAxisValue = float.MaxValue;
        maxAxisValue = float.MinValue;

        foreach (var v in baseVertices)
        {
            float value = GetAxisValue(v);
            if (value < minAxisValue) minAxisValue = value;
            if (value > maxAxisValue) maxAxisValue = value;
        }
    }

    float GetAxisValue(Vector3 v)
    {
        switch (bodyAxis)
        {
            case Axis.X: return v.x;
            case Axis.Y: return v.y;
            default: return v.z;
        }
    }

    void Update()
    {
        if (workingMesh == null) return;

        frameCounter++;
        if (updateEveryNFrames > 1 && frameCounter % updateEveryNFrames != 0) return;

        float t = Time.time * waveSpeed;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 original = baseVertices[i];
            float axisValue = GetAxisValue(original);

            float normalized = Mathf.InverseLerp(minAxisValue, maxAxisValue, axisValue);
            if (reverseDirection) normalized = 1f - normalized;

            float localAmplitude = waveAmplitude * Mathf.Pow(normalized, amplitudeGrowth);
            float offset = Mathf.Sin(t + normalized * waveFrequency * Mathf.PI * 2f) * localAmplitude;

            Vector3 displaced = original;
            if (bodyAxis == Axis.Z)
                displaced.x += offset;
            else if (bodyAxis == Axis.X)
                displaced.z += offset;
            else
                displaced.x += offset;

            displacedVertices[i] = displaced;
        }

        workingMesh.vertices = displacedVertices;

        // Only recompute normals occasionally — this is the single most
        // expensive call in this script, and the wave amplitude is small
        // enough that skipping frames here is visually unnoticeable.
        if (recalculateNormalsEveryNFrames <= 1 || frameCounter % recalculateNormalsEveryNFrames == 0)
        {
            workingMesh.RecalculateNormals();
        }
        // Bounds no longer need per-frame recalculation — we sized them
        // up front in Start() to cover the max possible wave displacement.
    }
}