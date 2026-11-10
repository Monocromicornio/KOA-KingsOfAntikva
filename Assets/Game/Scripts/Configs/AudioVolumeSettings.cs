using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Shared audio volume rules used by the boot sequence and the settings UI:
/// exposed mixer parameter names, PlayerPrefs keys, the perceptual volume curve and the mute state.
/// </summary>
public static class AudioVolumeSettings
{
    // Exposed mixer parameters (also used as PlayerPrefs keys for the slider values).
    public const string MasterVolumeParameter = "MasterVolume";
    public const string MusicVolumeParameter = "MusicVolume";
    public const string SfxVolumeParameter = "SFXVolume";
    public const string VoiceVolumeParameter = "VoiceVolume";
    public const string AmbienceVolumeParameter = "AmbienceVolume";

    public const string MuteKey = "Mute";
    public const float DefaultLinearVolume = 1f;

    private const float SilentDecibels = -80f;
    private const float MinimumLinearVolume = 0.0001f; // 20 * log10(0.0001) = -80 dB
    private const float DecibelsPerDecade = 20f;

    /// <summary>
    /// Every volume parameter driven by a slider, in display order.
    /// </summary>
    public static readonly string[] AllVolumeParameters =
    {
        MasterVolumeParameter,
        MusicVolumeParameter,
        SfxVolumeParameter,
        VoiceVolumeParameter,
        AmbienceVolumeParameter,
    };

    /// <summary>
    /// True when the player muted the game through the mute toggle.
    /// </summary>
    public static bool IsMuted => PlayerPrefs.GetInt(MuteKey, 0) == 1;

    /// <summary>
    /// Converts a linear slider value (0-1) to decibels using a logarithmic curve,
    /// so the perceived loudness changes evenly along the whole slider.
    /// </summary>
    public static float LinearToDecibels(float linearVolume)
    {
        return Mathf.Log10(Mathf.Max(linearVolume, MinimumLinearVolume)) * DecibelsPerDecade;
    }

    /// <summary>
    /// Returns the saved linear volume (0-1) for the given mixer parameter.
    /// </summary>
    public static float GetSavedVolume(string parameterName)
    {
        return PlayerPrefs.GetFloat(parameterName, DefaultLinearVolume);
    }

    /// <summary>
    /// Persists the linear volume (0-1) for the given mixer parameter.
    /// </summary>
    public static void SaveVolume(string parameterName, float linearVolume)
    {
        PlayerPrefs.SetFloat(parameterName, linearVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// True when the mixer exposes the given parameter (SetFloat silently fails otherwise).
    /// </summary>
    public static bool IsParameterExposed(AudioMixer mixer, string parameterName)
    {
        return mixer != null && mixer.GetFloat(parameterName, out _);
    }

    /// <summary>
    /// Applies a linear volume (0-1) to the given mixer parameter. Master stays silent while muted.
    /// </summary>
    public static void ApplyVolume(AudioMixer mixer, string parameterName, float linearVolume)
    {
        if (mixer == null) return;

        bool keepMasterSilent = parameterName == MasterVolumeParameter && IsMuted;
        mixer.SetFloat(parameterName, keepMasterSilent ? SilentDecibels : LinearToDecibels(linearVolume));
    }

    /// <summary>
    /// Applies every saved volume (and the mute state) to the mixer, skipping parameters the mixer does not expose.
    /// </summary>
    public static void ApplyAllSavedVolumes(AudioMixer mixer)
    {
        if (mixer == null) return;

        foreach (string parameterName in AllVolumeParameters)
        {
            if (IsParameterExposed(mixer, parameterName))
                ApplyVolume(mixer, parameterName, GetSavedVolume(parameterName));
        }
    }

    /// <summary>
    /// Saves the mute state and reapplies the master volume accordingly.
    /// </summary>
    public static void SetMuted(AudioMixer mixer, bool isMuted)
    {
        PlayerPrefs.SetInt(MuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyVolume(mixer, MasterVolumeParameter, GetSavedVolume(MasterVolumeParameter));
    }
}
