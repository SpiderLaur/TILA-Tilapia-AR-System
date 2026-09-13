using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Audio Controls")]
    public Slider volumeSlider;
    public Toggle muteToggle;

    [Header("AR Sensitivity (wire once that row exists)")]
    public Slider arSensitivitySlider;

    // PlayerPrefs keys
    private const string VOLUME_KEY = "settings_volume";
    private const string MUTE_KEY = "settings_muted";
    private const string AR_SENSITIVITY_KEY = "settings_ar_sensitivity";

    // Defaults
    private const float DEFAULT_VOLUME = 0.75f;
    private const bool DEFAULT_MUTED = false;
    private const float DEFAULT_AR_SENSITIVITY = 1f;

    void Start()
    {
        LoadSettings();

        // Subscribe AFTER loading so we don't immediately re-save the defaults
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteChanged);

        if (arSensitivitySlider != null)
            arSensitivitySlider.onValueChanged.AddListener(OnArSensitivityChanged);
    }

    void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        bool savedMuted = PlayerPrefs.GetInt(MUTE_KEY, DEFAULT_MUTED ? 1 : 0) == 1;
        float savedArSensitivity = PlayerPrefs.GetFloat(AR_SENSITIVITY_KEY, DEFAULT_AR_SENSITIVITY);

        if (volumeSlider != null)
            volumeSlider.value = savedVolume;

        if (muteToggle != null)
            muteToggle.isOn = savedMuted;

        if (arSensitivitySlider != null)
            arSensitivitySlider.value = savedArSensitivity;

        ApplyAudioState(savedVolume, savedMuted);
    }

    void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
        ApplyAudioState(value, muteToggle != null && muteToggle.isOn);
    }

    void OnMuteChanged(bool isMuted)
    {
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioState(volumeSlider != null ? volumeSlider.value : DEFAULT_VOLUME, isMuted);
    }

    void OnArSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(AR_SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
        // ModelGestureControl.cs should read this value at runtime, e.g.:
        // ModelGestureControl.SensitivityMultiplier = value;
    }

    void ApplyAudioState(float volume, bool isMuted)
    {
        AudioListener.volume = isMuted ? 0f : volume;
    }

    // Call this from other scripts if they need the current AR sensitivity value
    public static float GetSavedArSensitivity()
    {
        return PlayerPrefs.GetFloat(AR_SENSITIVITY_KEY, DEFAULT_AR_SENSITIVITY);
    }
}
