using UnityEngine;

public class BodyPartInfo : MonoBehaviour
{
    public string partName;

    [TextArea(3, 10)]
    public string description;

    public AudioClip voiceClip;

    public GameObject linkedPopupModel;
}