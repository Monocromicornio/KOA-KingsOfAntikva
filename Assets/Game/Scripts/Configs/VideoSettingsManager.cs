using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class VideoSettingsManager : MonoBehaviour
{
    // ---------- Localized option definitions (order must match the Set* methods below) ----------

    private const string OptionKeyPrefix = "settings.video";
    private const string ScreenModeKeyPrefix = OptionKeyPrefix + ".screen_mode";
    private const string QualityKeyPrefix = OptionKeyPrefix + ".quality";
    private const string FpsLimitKeyPrefix = OptionKeyPrefix + ".fps_limit";
    private const string ShadowQualityKeyPrefix = OptionKeyPrefix + ".shadow_quality";
    private const string AntiAliasingKeyPrefix = OptionKeyPrefix + ".anti_aliasing";
    private const string TextureQualityKeyPrefix = OptionKeyPrefix + ".texture_quality";
    private const char KeySegmentSeparator = '_';

    private static readonly LocalizedString[] ScreenModeOptions =
    {
        new LocalizedString(ScreenModeKeyPrefix + ".exclusive_fullscreen", "Exclusive Fullscreen"),
        new LocalizedString(ScreenModeKeyPrefix + ".windowed", "Windowed"),
        new LocalizedString(ScreenModeKeyPrefix + ".borderless_window", "Borderless Window"),
    };

    private static readonly LocalizedString[] FpsLimitOptions =
    {
        LocalizedString.Literal("30"),
        LocalizedString.Literal("60"),
        LocalizedString.Literal("120"),
        new LocalizedString(FpsLimitKeyPrefix + ".unlimited", "Unlimited"),
    };

    private static readonly LocalizedString[] ShadowQualityOptions =
    {
        new LocalizedString(ShadowQualityKeyPrefix + ".no_shadows", "No Shadows"),
        new LocalizedString(ShadowQualityKeyPrefix + ".hard_shadows", "Hard Shadows"),
        new LocalizedString(ShadowQualityKeyPrefix + ".all_shadows", "All Shadows"),
    };

    private static readonly LocalizedString[] AntiAliasingOptions =
    {
        new LocalizedString(AntiAliasingKeyPrefix + ".off", "Off"),
        LocalizedString.Literal("2x"),
        LocalizedString.Literal("4x"),
        LocalizedString.Literal("8x"),
    };

    private static readonly LocalizedString[] TextureQualityOptions =
    {
        new LocalizedString(TextureQualityKeyPrefix + ".high", "High"),
        new LocalizedString(TextureQualityKeyPrefix + ".medium", "Medium"),
        new LocalizedString(TextureQualityKeyPrefix + ".low", "Low"),
    };

    [Header("UI")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public Toggle vSyncToggle;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown fpsDropdown;    
    public TMP_Dropdown shadowQualityDropdown;
    public TMP_Dropdown aaDropdown;
    public TMP_Dropdown textureDropdown;

    private Resolution[] resolutions;
    private LocalizationManager _subscribedLocalizationManager;

    /// <summary>
    /// Returns every translatable dropdown option used by the video settings, including the project quality levels.
    /// Used by the localization editor to register the keys in the table.
    /// </summary>
    public static IEnumerable<LocalizedString> GetTranslatableOptions()
    {
        return ScreenModeOptions
            .Concat(GetQualityOptions())
            .Concat(FpsLimitOptions)
            .Concat(ShadowQualityOptions)
            .Concat(AntiAliasingOptions)
            .Concat(TextureQualityOptions)
            .Where(option => option.IsTranslatable);
    }

    private static LocalizedString[] GetQualityOptions()
    {
        return QualitySettings.names
            .Select(qualityName => new LocalizedString($"{QualityKeyPrefix}.{MakeKeySegment(qualityName)}", qualityName))
            .ToArray();
    }

    // "Very High" -> "very_high"
    private static string MakeKeySegment(string displayName)
    {
        var segment = new StringBuilder(displayName.Length);
        foreach (char character in displayName.Trim().ToLowerInvariant())
        {
            bool isSeparator = !char.IsLetterOrDigit(character);
            if (isSeparator && (segment.Length == 0 || segment[segment.Length - 1] == KeySegmentSeparator))
                continue;

            segment.Append(isSeparator ? KeySegmentSeparator : character);
        }
        return segment.ToString().TrimEnd(KeySegmentSeparator);
    }

    void OnEnable()
    {
        RefreshLocalizedOptionLabels();
        LoadResolutions();
        LoadSettings();
        StartCoroutine(SubscribeToLanguageChangesWhenReady());

        // Liga os listeners (UI → aplicação imediata)
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
        vSyncToggle.onValueChanged.AddListener(SetVSync);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fpsDropdown.onValueChanged.AddListener(SetFPSLimit);        
        shadowQualityDropdown.onValueChanged.AddListener(SetShadowQuality);
        aaDropdown.onValueChanged.AddListener(SetAntiAliasing);
        textureDropdown.onValueChanged.AddListener(SetTextureQuality);
    }

    void OnDisable()
    {
        // Remove listeners ao fechar a aba
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        screenModeDropdown.onValueChanged.RemoveAllListeners();
        vSyncToggle.onValueChanged.RemoveAllListeners();
        qualityDropdown.onValueChanged.RemoveAllListeners();
        fpsDropdown.onValueChanged.RemoveAllListeners();        
        shadowQualityDropdown.onValueChanged.RemoveAllListeners();
        aaDropdown.onValueChanged.RemoveAllListeners();
        textureDropdown.onValueChanged.RemoveAllListeners();

        if (_subscribedLocalizationManager != null)
        {
            _subscribedLocalizationManager.OnLanguageChanged -= RefreshLocalizedOptionLabels;
            _subscribedLocalizationManager = null;
        }
    }

    // ---------- Inicialização ----------

    /// <summary>
    /// Fills the option dropdowns with labels in the current language, keeping the current selections
    /// and without applying any setting.
    /// </summary>
    public void RefreshLocalizedOptionLabels()
    {
        screenModeDropdown.SetLocalizedOptions(ScreenModeOptions);
        qualityDropdown.SetLocalizedOptions(GetQualityOptions());
        fpsDropdown.SetLocalizedOptions(FpsLimitOptions);
        shadowQualityDropdown.SetLocalizedOptions(ShadowQualityOptions);
        aaDropdown.SetLocalizedOptions(AntiAliasingOptions);
        textureDropdown.SetLocalizedOptions(TextureQualityOptions);
    }

    private IEnumerator SubscribeToLanguageChangesWhenReady()
    {
        while (LocalizationManager.Instance == null)
            yield return null;

        _subscribedLocalizationManager = LocalizationManager.Instance;
        _subscribedLocalizationManager.OnLanguageChanged -= RefreshLocalizedOptionLabels;
        _subscribedLocalizationManager.OnLanguageChanged += RefreshLocalizedOptionLabels;
        RefreshLocalizedOptionLabels();
    }

    void LoadResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        foreach (var res in resolutions)
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width}x{res.height}"));
        resolutionDropdown.RefreshShownValue();
    }

    // ---------- Aplicação e salvamento ----------

    public void SetResolution(int index)
    {
        if (index >= resolutions.Length) return;
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    public void SetScreenMode(int index)
    {
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        if (index == 1) mode = FullScreenMode.Windowed;
        if (index == 2) mode = FullScreenMode.FullScreenWindow;

        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt("ScreenMode", index);
        PlayerPrefs.Save();
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
        PlayerPrefs.Save();
    }

    public void SetFPSLimit(int index)
    {
        int[] fpsValues = { 30, 60, 120, -1 };
        Application.targetFrameRate = fpsValues[index];
        PlayerPrefs.SetInt("FPSLimit", index);
        PlayerPrefs.Save();
    }

    public void SetRenderDistance(float value)
    {
        if (Camera.main != null)
            Camera.main.farClipPlane = value;
        PlayerPrefs.SetFloat("RenderDistance", value);
        PlayerPrefs.Save();
    }

    public void SetShadowQuality(int index)
    {
        switch (index)
        {
            case 0: QualitySettings.shadows = ShadowQuality.Disable; break;
            case 1: QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2: QualitySettings.shadows = ShadowQuality.All; break;
        }
        PlayerPrefs.SetInt("ShadowQuality", index);
        PlayerPrefs.Save();
    }

    public void SetAntiAliasing(int index)
    {
        int[] aaLevels = { 0, 2, 4, 8 };
        QualitySettings.antiAliasing = aaLevels[index];
        PlayerPrefs.SetInt("AA", index);
        PlayerPrefs.Save();
    }

    public void SetTextureQuality(int index)
    {
        QualitySettings.globalTextureMipmapLimit = index;
        PlayerPrefs.SetInt("TextureQuality", index);
        PlayerPrefs.Save();
    }

    // ---------- Restaura valores ao abrir ----------
    void LoadSettings()
    {
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        screenModeDropdown.value = PlayerPrefs.GetInt("ScreenMode", 1);
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        qualityDropdown.value = PlayerPrefs.GetInt("Quality", 2);
        fpsDropdown.value = PlayerPrefs.GetInt("FPSLimit", 1);        
        shadowQualityDropdown.value = PlayerPrefs.GetInt("ShadowQuality", 2);
        aaDropdown.value = PlayerPrefs.GetInt("AA", 2);
        textureDropdown.value = PlayerPrefs.GetInt("TextureQuality", 0);
    }
}
