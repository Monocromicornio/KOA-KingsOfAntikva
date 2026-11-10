using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Applies the saved volumes (and mute state) to the mixer as soon as the game boots.
/// </summary>
public class AudioBootstrapper : MonoBehaviour
{
    [Header("Mixer com parâmetros expostos")]
    public AudioMixer mixer;

    void Start()
    {
        AudioVolumeSettings.ApplyAllSavedVolumes(mixer);
    }
}
