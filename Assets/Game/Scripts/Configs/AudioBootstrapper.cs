using UnityEngine;
using UnityEngine.Audio;

public class AudioBootstrapper : MonoBehaviour
{
    [Header("Mixer com parâmetros expostos")]
    public AudioMixer mixer;

    const string P_MASTER = "MasterVolume", P_MUSIC = "MusicVolume", P_SFX = "SFXVolume", P_VOICE = "VoiceVolume", P_AMB = "AmbienceVolume";

    void Start()
    {
        // aplica valores salvos (ou 1.0 padrão) logo no boot
        Apply(P_MASTER, PlayerPrefs.GetFloat(P_MASTER, 1f));
        Apply(P_MUSIC, PlayerPrefs.GetFloat(P_MUSIC, 1f));
        Apply(P_SFX, PlayerPrefs.GetFloat(P_SFX, 1f));
        Apply(P_VOICE, PlayerPrefs.GetFloat(P_VOICE, 1f));
        Apply(P_AMB, PlayerPrefs.GetFloat(P_AMB, 1f));

        // Mute preservado
        if (PlayerPrefs.GetInt("Mute", 0) == 1) mixer.SetFloat(P_MASTER, -80f);
    }

    static float ToDb(float v) => Mathf.Lerp(-80f, 0f, v);

    void Apply(string param, float value) => mixer.SetFloat(param, ToDb(value));
}