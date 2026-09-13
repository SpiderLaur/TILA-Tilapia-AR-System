using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowAllLabels : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public Transform modelRoot;
    public GameObject labelPrefab;
    public InfoPanelController infoPanelController;
    public float lineWidth = 0.002f;
    public Color lineColor = Color.white;

    [Header("Zoom Compensation")]
    [Tooltip("Auto-detected from modelRoot.lossyScale.x in Start(). Not meant to be hand-set in the Inspector, since different models (e.g. External at ~1 vs Skeletal at ~0.11877) have very different root scales.")]
    public float baseModelScale = 1f;
    [Tooltip("Auto-detected in Start(): the average local-space distance from each body part to the part cloud's center. Lets the label offset scale to THIS model's own anatomical spread instead of a single fixed world distance, so a tightly-clustered model (e.g. Skeletal) and a more spread-out one (e.g. External) both get a proportionate offset.")]
    public float basePartSpreadRadius = 1f;
    [Tooltip("Label offset distance, expressed as a multiple of basePartSpreadRadius, so offsets stay proportionate to how spread out this particular model's body parts are.")]
    public float labelOffsetRatio = 1.3f;
    [Tooltip("Clamp how much the offset can shrink/grow so labels don't collapse into the model or fly too far when scale changes drastically.")]
    public float minOffsetMultiplier = 0.3f;
    public float maxOffsetMultiplier = 2.5f;
    [Tooltip("Clamp how much the leader-line width can shrink/grow so lines stay a consistent, readable thickness when scale changes drastically.")]
    public float minLineWidthMultiplier = 0.3f;
    public float maxLineWidthMultiplier = 2.5f;

    private List<BodyPartInfo> bodyParts = new List<BodyPartInfo>();
    private List<GameObject> spawnedLabels = new List<GameObject>();
    private List<LineRenderer> spawnedLines = new List<LineRenderer>();
    private bool isShowingAll = false;

    void Start()
    {
        bodyParts.Clear();
        if (modelRoot != null)
        {
            bodyParts.AddRange(modelRoot.GetComponentsInChildren<BodyPartInfo>(true));

            baseModelScale = modelRoot.lossyScale.x;
            if (baseModelScale <= 0.0001f) baseModelScale = 1f; // avoid divide-by-zero later

            basePartSpreadRadius = ComputeAveragePartSpreadRadius();
        }
    }

    // Average local-space distance from each body part to the part cloud's center,
    // measured once at rest. Used to make the label offset proportionate to this
    // specific model's own anatomical spread (see basePartSpreadRadius above).
    float ComputeAveragePartSpreadRadius()
    {
        if (bodyParts.Count == 0) return 1f;

        Vector3 centerLocal = Vector3.zero;
        foreach (var part in bodyParts)
            centerLocal += modelRoot.InverseTransformPoint(GetPartWorldPosition(part));
        centerLocal /= bodyParts.Count;

        float totalDistance = 0f;
        foreach (var part in bodyParts)
        {
            Vector3 partLocalPos = modelRoot.InverseTransformPoint(GetPartWorldPosition(part));
            totalDistance += Vector3.Distance(partLocalPos, centerLocal);
        }

        float average = totalDistance / bodyParts.Count;
        return average <= 0.0001f ? 1f : average;
    }

    public void ToggleShowAll()
    {
        if (isShowingAll) HideAll();
        else ShowAll();
    }

    // Shared by the offset and line-width compensation below: ratio of the model's
    // resting scale to its current (possibly zoomed) scale, clamped so a value derived
    // from it can't collapse to zero or blow up when the model is scaled drastically.
    float GetClampedScaleRatio(float currentScale, float minMultiplier, float maxMultiplier)
    {
        float scaleRatio = baseModelScale / currentScale;
        return Mathf.Clamp(scaleRatio, minMultiplier, maxMultiplier);
    }

    float GetEffectiveOffsetDistance()
    {
        // Use lossyScale so this also works if modelRoot has a scaled parent
        float currentScale = modelRoot.lossyScale.x;
        if (currentScale <= 0.0001f) currentScale = 0.0001f; // avoid divide-by-zero

        float scaleRatio = GetClampedScaleRatio(currentScale, minOffsetMultiplier, maxOffsetMultiplier);

        // Scale by basePartSpreadRadius (this model's own average part-to-center
        // distance) rather than a fixed world distance, so the resting offset is
        // always labelOffsetRatio times as large as THIS model's own anatomical
        // spread — proportionate whether the body parts are tightly clustered
        // (e.g. Skeletal) or more spread out (e.g. External). baseModelScale drops
        // out of the math entirely: it would otherwise multiply basePartSpreadRadius
        // into world space here only to be divided back out by the live scale when
        // Unity renders this local-space value, same cancellation scaleRatio relies on.
        return labelOffsetRatio * basePartSpreadRadius * scaleRatio;
    }

    void ShowAll()
    {
        float effectiveOffset = GetEffectiveOffsetDistance();

        Vector3 centerLocal = Vector3.zero;
        foreach (var part in bodyParts)
            centerLocal += modelRoot.InverseTransformPoint(GetPartWorldPosition(part));
        centerLocal /= bodyParts.Count;

        foreach (BodyPartInfo part in bodyParts)
        {
            Vector3 partWorldPos = GetPartWorldPosition(part);
            Vector3 partLocalPos = modelRoot.InverseTransformPoint(partWorldPos);

            Vector3 dir = partLocalPos - centerLocal;
            if (dir.magnitude < 0.001f) dir = Random.onUnitSphere;
            dir.Normalize();

            Vector3 labelLocalPos = partLocalPos + dir * effectiveOffset;

            GameObject label = Instantiate(labelPrefab, modelRoot);
            label.transform.localPosition = labelLocalPos;
            label.transform.localRotation = Quaternion.identity;

            // Counteract modelRoot's live scale directly so labels render at a
            // constant world-space size, both during zoom AND across different
            // models that have very different absolute Transform scales (e.g.
            // External at scale 1 vs Skeletal at scale 0.11877).
            float modelScale = modelRoot.lossyScale.x;
            if (modelScale <= 0.0001f) modelScale = 0.0001f;
            Vector3 baseLabelScale = label.transform.localScale;
            label.transform.localScale = baseLabelScale / modelScale;

            Billboard bb = label.GetComponent<Billboard>();
            if (bb != null) bb.targetCamera = arCamera;

            // FIX: World Space Canvas needs an Event Camera assigned, or its
            // Button/Graphic Raycaster cannot correctly detect taps on device.
            Canvas labelCanvas = label.GetComponent<Canvas>();
            if (labelCanvas != null) labelCanvas.worldCamera = arCamera;

            TMP_Text labelText = label.GetComponentInChildren<TMP_Text>();
            if (labelText != null) labelText.text = part.partName;

            AutoResizeLabelBox resizer = label.GetComponentInChildren<AutoResizeLabelBox>();
            if (resizer != null) resizer.ResizeToFitText();

            BodyPartInfo capturedPart = part;
            Button labelButton = label.GetComponentInChildren<Button>();
            if (labelButton != null)
            {
                labelButton.onClick.AddListener(() =>
                {
                    infoPanelController.Show(
                        capturedPart.partName,
                        capturedPart.description,
                        capturedPart.voiceClip,
                        capturedPart.linkedPopupModel);
                });
            }

            GameObject lineObj = new GameObject("WorldLine_" + part.partName);
            lineObj.transform.SetParent(modelRoot, false);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = false;
            // NOTE: Unlike a mesh's vertex positions, LineRenderer width is NOT
            // automatically scaled by the object's Transform scale — it renders
            // at whatever widthMultiplier value is set, regardless of modelRoot's
            // lossyScale. Dividing by baseModelScale here was the actual bug: it
            // inflated Skeletal's width by roughly (1 / 0.117) ≈ 8.5x relative to
            // External, since baseModelScale differs drastically between models
            // (External ≈ 1, Skeletal ≈ 0.117) even though both models' widthScaleRatio
            // sits at ~1 at rest. Using widthScaleRatio alone keeps the line at
            // exactly lineWidth for BOTH models at rest, and still lets it grow/shrink
            // within [minLineWidthMultiplier, maxLineWidthMultiplier] as the user
            // pinch-zooms away from each model's own resting scale.
            float widthScaleRatio = GetClampedScaleRatio(modelScale, minLineWidthMultiplier, maxLineWidthMultiplier);
            lr.widthMultiplier = lineWidth * widthScaleRatio;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.SetPosition(0, partLocalPos);
            lr.SetPosition(1, labelLocalPos);

            spawnedLabels.Add(label);
            spawnedLines.Add(lr);
        }

        isShowingAll = true;

        SetAnimationsEnabled(false);
    }

    public void HideAll()
    {
        foreach (GameObject label in spawnedLabels) Destroy(label);
        foreach (LineRenderer line in spawnedLines) Destroy(line.gameObject);

        spawnedLabels.Clear();
        spawnedLines.Clear();
        isShowingAll = false;

        SetAnimationsEnabled(true);
    }

    // Looks in children, on self, AND in parents so it doesn't matter
    // which exact GameObject modelRoot is assigned to in the Inspector.
    void SetAnimationsEnabled(bool enabled)
    {
        FishBodyWaveAnimation waveAnim = modelRoot.GetComponentInChildren<FishBodyWaveAnimation>();
        if (waveAnim == null) waveAnim = modelRoot.GetComponentInParent<FishBodyWaveAnimation>();
        if (waveAnim != null) waveAnim.enabled = enabled;

        SwimAnimation swimAnim = modelRoot.GetComponent<SwimAnimation>();
        if (swimAnim == null) swimAnim = modelRoot.GetComponentInChildren<SwimAnimation>();
        if (swimAnim == null) swimAnim = modelRoot.GetComponentInParent<SwimAnimation>();
        if (swimAnim != null) swimAnim.enabled = enabled;
    }

    Vector3 GetPartWorldPosition(BodyPartInfo part)
    {
        SphereCollider sc = part.GetComponent<SphereCollider>();
        if (sc != null) return sc.transform.TransformPoint(sc.center);

        CapsuleCollider cc = part.GetComponent<CapsuleCollider>();
        if (cc != null) return cc.transform.TransformPoint(cc.center);

        BoxCollider bc = part.GetComponent<BoxCollider>();
        if (bc != null) return bc.transform.TransformPoint(bc.center);

        return part.transform.position;
    }
}