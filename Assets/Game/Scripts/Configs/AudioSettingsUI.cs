using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer mixer;

    [Header("Sliders")]
    public Slider master, music, sfx, voice, ambience;
   // public Toggle muteToggle;

    const string P_MASTER = "MasterVolume", P_MUSIC = "MusicVolume",
                 P_SFX = "SFXVolume", P_VOICE = "VoiceVolume", P_AMB = "AmbienceVolume";

    void OnEnable()
    {
        // carregar sem disparar callbacks
        master.SetValueWithoutNotify(PlayerPrefs.GetFloat(P_MASTER, 1f));
        music.SetValueWithoutNotify(PlayerPrefs.GetFloat(P_MUSIC, 1f));
        sfx.SetValueWithoutNotify(PlayerPrefs.GetFloat(P_SFX, 1f));
        voice.SetValueWithoutNotify(PlayerPrefs.GetFloat(P_VOICE, 1f));
        ambience.SetValueWithoutNotify(PlayerPrefs.GetFloat(P_AMB, 1f));
        // muteToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Mute", 0) == 1);

        // aplicar ao abrir (útil se mudou fora da UI)
        ApplyParam(P_MASTER, master.value);
        ApplyParam(P_MUSIC, music.value);
        ApplyParam(P_SFX, sfx.value);
        ApplyParam(P_VOICE, voice.value);
        ApplyParam(P_AMB, ambience.value);
        //if (muteToggle.isOn) mixer.SetFloat(P_MASTER, -80f);

        // ligar eventos
        master.onValueChanged.AddListener(v => { ApplyParam(P_MASTER, v); SaveFloat(P_MASTER, v); });
        music.onValueChanged.AddListener(v => { ApplyParam(P_MUSIC, v); SaveFloat(P_MUSIC, v); });
        sfx.onValueChanged.AddListener(v => { ApplyParam(P_SFX, v); SaveFloat(P_SFX, v); });
        voice.onValueChanged.AddListener(v => { ApplyParam(P_VOICE, v); SaveFloat(P_VOICE, v); });
        ambience.onValueChanged.AddListener(v => { ApplyParam(P_AMB, v); SaveFloat(P_AMB, v); });
        // muteToggle.onValueChanged.AddListener(OnMute);
    }

    void OnDisable()
    {
        // remove listeners pra evitar múltiplos binds ao reabrir
        master.onValueChanged.RemoveAllListeners();
        music.onValueChanged.RemoveAllListeners();
        sfx.onValueChanged.RemoveAllListeners();
        voice.onValueChanged.RemoveAllListeners();
        ambience.onValueChanged.RemoveAllListeners();
       // muteToggle.onValueChanged.RemoveAllListeners();
    }

    static float ToDb(float v) => Mathf.Lerp(-80f, 0f, v);

    void ApplyParam(string param, float val) { mixer.SetFloat(param, ToDb(val)); }

    void SaveFloat(string key, float v) { PlayerPrefs.SetFloat(key, v); PlayerPrefs.Save(); }

    void OnMute(bool on)
    {
        if (on) mixer.SetFloat(P_MASTER, -80f);
        else ApplyParam(P_MASTER, master.value);
        PlayerPrefs.SetInt("Mute", on ? 1 : 0);
        PlayerPrefs.Save();
    }
}