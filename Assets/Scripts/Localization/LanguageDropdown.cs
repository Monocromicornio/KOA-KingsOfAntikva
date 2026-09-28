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
    [Serializable]
    public class LanguageOption
    {
        [Tooltip("Language code matching the file name in Resources/Localization (e.g. \"en\", \"pt-br\").")]
        public string code;

        [Tooltip("Name shown in the dropdown (usually written in the language itself).")]
        public string displayName;
    }

    [SerializeField]
    private List<LanguageOption> _languages = new List<LanguageOption>
    {
        new LanguageOption { code = "en", displayName = "English" },
        new LanguageOption { code = "pt-br", displayName = "Português (BR)" },
    };

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
        var optionNames = new List<string>(_languages.Count);
        foreach (var language in _languages)
            optionNames.Add(language.displayName);

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
        if (selectedIndex < 0 || selectedIndex >= _languages.Count) return;

        var manager = LocalizationManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("[LanguageDropdown] LocalizationManager not found in the scene.");
            return;
        }

        manager.SetLanguage(_languages[selectedIndex].code);
    }

    private int FindLanguageIndex(string languageCode)
    {
        for (int i = 0; i < _languages.Count; i++)
        {
            if (string.Equals(_languages[i].code, languageCode, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}
