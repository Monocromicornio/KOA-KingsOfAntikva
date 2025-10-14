using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VideoSettingsManager : MonoBehaviour
{
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

    void OnEnable()
    {
        LoadDropdownOptions();
        LoadResolutions();
        LoadSettings();

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
    }

    // ---------- Inicialização ----------

    void LoadDropdownOptions()
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string> { "Exclusive Fullscreen", "Windowed", "Borderless Window" });

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new List<string> { "30", "60", "120", "Unlimited" });

        shadowQualityDropdown.ClearOptions();
        shadowQualityDropdown.AddOptions(new List<string> { "No Shadows", "Hard Shadows", "All Shadows" });

        aaDropdown.ClearOptions();
        aaDropdown.AddOptions(new List<string> { "Off", "2x", "4x", "8x" });

        textureDropdown.ClearOptions();
        textureDropdown.AddOptions(new List<string> { "High", "Medium", "Low" });
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
