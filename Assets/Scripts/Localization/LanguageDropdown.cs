using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Binds a TMP_Dropdown to the LocalizationManager so the player can switch the game language.
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class LanguageDropdown : MonoBehaviour
{
    // The languages (and their names) come from code, not from the scene, so adding a language never needs a scene edit.
    private static IReadOnlyList<LocalizationManager.LanguageInfo> Languages => LocalizationManager.SupportedLanguages;

    private TMP_Dropdown _dropdown;
    private LocalizationManager _subscribedManager;

    void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        PopulateOptions();
    }

    void OnEnable()
    {
        _dropdown.onValueChanged.AddListener(HandleDropdownValueChanged);
        StartCoroutine(WaitForManagerThenSync());
    }

    void OnDisable()
    {
        _dropdown.onValueChanged.RemoveListener(HandleDropdownValueChanged);

        if (_subscribedManager != null)
        {
            _subscribedManager.OnLanguageChanged -= SyncSelectionWithCurrentLanguage;
            _subscribedManager = null;
        }
    }

    private void PopulateOptions()
    {
        var optionNames = new List<string>(Languages.Count);
        foreach (var language in Languages)
            optionNames.Add(language.NativeName);

        _dropdown.ClearOptions();
        _dropdown.AddOptions(optionNames);
    }

    private IEnumerator WaitForManagerThenSync()
    {
        while (LocalizationManager.Instance == null)
            yield return null;

        _subscribedManager = LocalizationManager.Instance;
        _subscribedManager.OnLanguageChanged -= SyncSelectionWithCurrentLanguage;
        _subscribedManager.OnLanguageChanged += SyncSelectionWithCurrentLanguage;
        SyncSelectionWithCurrentLanguage();
    }

    /// <summary>
    /// Updates the dropdown selection to match the language currently active in the LocalizationManager.
    /// </summary>
    public void SyncSelectionWithCurrentLanguage()
    {
        var manager = LocalizationManager.Instance;
        if (manager == null) return;

        int languageIndex = FindLanguageIndex(manager.CurrentLanguage);
        if (languageIndex < 0) return;

        _dropdown.SetValueWithoutNotify(languageIndex);
        _dropdown.RefreshShownValue();
    }

    private void HandleDropdownValueChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= Languages.Count) return;

        var manager = LocalizationManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("[LanguageDropdown] LocalizationManager not found in the scene.");
            return;
        }

        manager.SetLanguage(Languages[selectedIndex].Code);
    }

    private int FindLanguageIndex(string languageCode)
    {
        for (int i = 0; i < Languages.Count; i++)
        {
            if (string.Equals(Languages[i].Code, languageCode, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}
