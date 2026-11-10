using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Binds the audio settings sliders (and the optional mute toggle) to the mixer and persists their values.
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer mixer;

    [Header("Sliders")]
    [Tooltip("Opcional: deixe vazio se a interface não tiver o slider.")]
    public Slider master, music, sfx, voice, ambience;

    [Header("Optional")]
    [Tooltip("Opcional: se não houver toggle de mudo, o estado de mudo antigo é limpo ao abrir o painel.")]
    public Toggle muteToggle;

    private readonly List<Slider> _boundSliders = new List<Slider>();

    void OnEnable()
    {
        BindMuteToggle();

        BindSlider(master, AudioVolumeSettings.MasterVolumeParameter);
        BindSlider(music, AudioVolumeSettings.MusicVolumeParameter);
        BindSlider(sfx, AudioVolumeSettings.SfxVolumeParameter);
        BindSlider(voice, AudioVolumeSettings.VoiceVolumeParameter);
        BindSlider(ambience, AudioVolumeSettings.AmbienceVolumeParameter);
    }

    void OnDisable()
    {
        // remove listeners pra evitar múltiplos binds ao reabrir
        foreach (var slider in _boundSliders)
            slider.onValueChanged.RemoveAllListeners();
        _boundSliders.Clear();

        if (muteToggle != null)
            muteToggle.onValueChanged.RemoveListener(HandleMuteToggled);
    }

    private void BindMuteToggle()
    {
        if (muteToggle == null)
        {
            // Without a mute toggle the player could never undo a previously saved mute.
            if (AudioVolumeSettings.IsMuted)
                AudioVolumeSettings.SetMuted(mixer, false);
            return;
        }

        muteToggle.SetIsOnWithoutNotify(AudioVolumeSettings.IsMuted);
        muteToggle.onValueChanged.AddListener(HandleMuteToggled);
    }

    private void BindSlider(Slider slider, string parameterName)
    {
        if (slider == null) return;

        // The same slider must never drive two parameters (e.g. Voice pointing to the SFX slider).
        if (_boundSliders.Contains(slider))
        {
            Debug.LogWarning($"[AudioSettingsUI] O slider '{slider.name}' já controla outro parâmetro; '{parameterName}' foi ignorado.");
            return;
        }

        if (!AudioVolumeSettings.IsParameterExposed(mixer, parameterName))
        {
            Debug.LogWarning($"[AudioSettingsUI] O mixer não expõe '{parameterName}'; o slider '{slider.name}' foi ocultado.");
            slider.gameObject.SetActive(false);
            return;
        }

        float savedVolume = AudioVolumeSettings.GetSavedVolume(parameterName);
        slider.SetValueWithoutNotify(savedVolume);
        AudioVolumeSettings.ApplyVolume(mixer, parameterName, savedVolume);

        slider.onValueChanged.AddListener(linearVolume =>
        {
            AudioVolumeSettings.ApplyVolume(mixer, parameterName, linearVolume);
            AudioVolumeSettings.SaveVolume(parameterName, linearVolume);
        });
        _boundSliders.Add(slider);
    }

    private void HandleMuteToggled(bool isMuted)
    {
        AudioVolumeSettings.SetMuted(mixer, isMuted);
    }
}
