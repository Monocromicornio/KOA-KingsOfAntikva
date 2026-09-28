using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private const string LanguagePrefsKey = "Language";
    private const string LocalizationResourcesFolder = "Localization";

    public static LocalizationManager Instance { get; private set; }
    public string CurrentLanguage = "en"; // default
    public event Action OnLanguageChanged;

    private Dictionary<string, string> _table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    [Serializable]
    public class KV
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class SerializableDict
    {
        public List<KV> items = new List<KV>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string savedLanguage = PlayerPrefs.GetString(LanguagePrefsKey, CurrentLanguage);
        LoadLanguage(savedLanguage);
    }

    /// <summary>
    /// Loads the localization table for the given language code (e.g. "en", "pt-br")
    /// and notifies all listeners that the language has changed.
    /// </summary>
    public void LoadLanguage(string lang)
    {
        CurrentLanguage = lang;
        _table.Clear();

        var textAsset = Resources.Load<TextAsset>($"{LocalizationResourcesFolder}/{lang}");
        if (textAsset != null)
        {
            var dict = JsonConvert.DeserializeObject<SerializableDict>(textAsset.text);
            if (dict != null && dict.items != null)
            {
                foreach (var it in dict.items)
                    _table[it.key] = it.value;
            }
        }
        else
        {
            Debug.LogWarning($"[LocalizationManager] Localization file not found: Resources/{LocalizationResourcesFolder}/{lang}");
        }

        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Changes the active language, reloads the table and persists the choice between sessions.
    /// </summary>
    public void SetLanguage(string lang)
    {
        if (string.IsNullOrEmpty(lang) || string.Equals(lang, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            return;

        LoadLanguage(lang);
        PlayerPrefs.SetString(LanguagePrefsKey, lang);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Tries to get the localized value for the given key in the current language.
    /// </summary>
    public bool TryGet(string key, out string value) => _table.TryGetValue(key, out value);

    /// <summary>
    /// Serializes a localization dictionary to JSON.
    /// </summary>
    public static string ToJson(SerializableDict dict, bool pretty = true)
    {
        return JsonConvert.SerializeObject(dict, pretty ? Formatting.Indented : Formatting.None);
    }

    /// <summary>
    /// Deserializes a localization dictionary from JSON.
    /// </summary>
    public static SerializableDict FromJson(string json)
    {
        return JsonConvert.DeserializeObject<SerializableDict>(json);
    }
}
