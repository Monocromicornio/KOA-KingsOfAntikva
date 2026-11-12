using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using Steamworks;
#endif

public class LocalizationManager : MonoBehaviour
{
    private const string LanguagePrefsKey = "Language";
    private const string LocalizationResourcesFolder = "Localization";

    /// <summary>Language used when neither Steam nor the operating system language has a translation.</summary>
    public const string DefaultLanguageCode = "en";
    public const string BrazilianPortugueseLanguageCode = "pt-br";
    public const string SpanishLanguageCode = "es";
    public const string GermanLanguageCode = "de";
    public const string FrenchLanguageCode = "fr";
    public const string ItalianLanguageCode = "it";
    public const string JapaneseLanguageCode = "ja";
    public const string SimplifiedChineseLanguageCode = "zh-hans";
    public const string TraditionalChineseLanguageCode = "zh-hant";

    /// <summary>
    /// A language the player can pick. <see cref="Code"/> is the file name in Resources/Localization (and the column
    /// header in the localization CSV); <see cref="NativeName"/> is shown in the language dropdown, written in the language itself.
    /// </summary>
    public readonly struct LanguageInfo
    {
        public readonly string Code;
        public readonly string NativeName;

        public LanguageInfo(string code, string nativeName)
        {
            Code = code;
            NativeName = nativeName;
        }
    }

    /// <summary>
    /// Every language the game ships. To add one: add it here, add its column to the localization CSV, import the CSV
    /// and export the JSONs (Tools > Localization > Manager). Chinese and Japanese also need a CJK font (see LocalizationFontFallback).
    /// </summary>
    public static readonly IReadOnlyList<LanguageInfo> SupportedLanguages = new[]
    {
        new LanguageInfo(DefaultLanguageCode, "English"),
        new LanguageInfo(BrazilianPortugueseLanguageCode, "Português (BR)"),
        new LanguageInfo(SpanishLanguageCode, "Español"),
        new LanguageInfo(GermanLanguageCode, "Deutsch"),
        new LanguageInfo(FrenchLanguageCode, "Français"),
        new LanguageInfo(ItalianLanguageCode, "Italiano"),
        new LanguageInfo(JapaneseLanguageCode, "日本語"),
        new LanguageInfo(SimplifiedChineseLanguageCode, "简体中文"),
        new LanguageInfo(TraditionalChineseLanguageCode, "繁體中文"),
    };

    // Steam game language names (Steam client > Properties > Language): https://partner.steamgames.com/doc/store/localization/languages
    private static readonly Dictionary<string, string> SteamLanguageToCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "english", DefaultLanguageCode },
        { "brazilian", BrazilianPortugueseLanguageCode },
        { "portuguese", BrazilianPortugueseLanguageCode },
        { "spanish", SpanishLanguageCode },
        { "latam", SpanishLanguageCode },
        { "german", GermanLanguageCode },
        { "french", FrenchLanguageCode },
        { "italian", ItalianLanguageCode },
        { "japanese", JapaneseLanguageCode },
        { "schinese", SimplifiedChineseLanguageCode },
        { "tchinese", TraditionalChineseLanguageCode },
    };

    private static readonly Dictionary<SystemLanguage, string> SystemLanguageToCode = new Dictionary<SystemLanguage, string>
    {
        { SystemLanguage.English, DefaultLanguageCode },
        { SystemLanguage.Portuguese, BrazilianPortugueseLanguageCode },
        { SystemLanguage.Spanish, SpanishLanguageCode },
        { SystemLanguage.German, GermanLanguageCode },
        { SystemLanguage.French, FrenchLanguageCode },
        { SystemLanguage.Italian, ItalianLanguageCode },
        { SystemLanguage.Japanese, JapaneseLanguageCode },
        { SystemLanguage.ChineseSimplified, SimplifiedChineseLanguageCode },
        { SystemLanguage.ChineseTraditional, TraditionalChineseLanguageCode },
        { SystemLanguage.Chinese, SimplifiedChineseLanguageCode }, // generic "Chinese" reported without a script
    };

    public static LocalizationManager Instance { get; private set; }
    public string CurrentLanguage = DefaultLanguageCode;
    public event Action OnLanguageChanged;

    /// <summary>
    /// True once the player picked a language manually; until then the language follows Steam / the operating system.
    /// </summary>
    public static bool HasSavedLanguagePreference => PlayerPrefs.HasKey(LanguagePrefsKey);

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

        string initialLanguage = HasSavedLanguagePreference
            ? PlayerPrefs.GetString(LanguagePrefsKey)
            : DetectPreferredLanguage();
        LoadLanguage(initialLanguage);
    }

    void OnDestroy()
    {
        if (Instance == this)
            LocalizationFontFallback.Release();
    }

    void Start()
    {
        // Steam may finish initializing after this Awake; re-detect so the Steam language wins over the OS language.
        if (HasSavedLanguagePreference) return;

        string detectedLanguage = DetectPreferredLanguage();
        if (!string.Equals(detectedLanguage, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            LoadLanguage(detectedLanguage);
    }

    /// <summary>
    /// Picks the best available language for a player who never chose one:
    /// the Steam game language first, then the operating system language, then English.
    /// </summary>
    public static string DetectPreferredLanguage()
    {
        string steamLanguage = GetSteamLanguageCode();
        if (IsLanguageAvailable(steamLanguage)) return steamLanguage;

        string systemLanguage = SystemLanguageToCode.TryGetValue(Application.systemLanguage, out var systemCode) ? systemCode : null;
        if (IsLanguageAvailable(systemLanguage)) return systemLanguage;

        return DefaultLanguageCode;
    }

    private static string GetSteamLanguageCode()
    {
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
        if (!SteamInitializer.Initialized) return null;

        try
        {
            string steamLanguage = SteamApps.GetCurrentGameLanguage();
            if (!string.IsNullOrEmpty(steamLanguage) && SteamLanguageToCode.TryGetValue(steamLanguage, out var steamCode))
                return steamCode;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LocalizationManager] Could not read the Steam language: {exception.Message}");
        }
#endif
        return null;
    }

    /// <summary>
    /// True when a localization table exists in Resources for the given language code.
    /// </summary>
    public static bool IsLanguageAvailable(string lang)
    {
        return !string.IsNullOrEmpty(lang) && Resources.Load<TextAsset>($"{LocalizationResourcesFolder}/{lang}") != null;
    }

    /// <summary>
    /// Loads the localization table for the given language code (e.g. "en", "pt-br")
    /// and notifies all listeners that the language has changed. Unknown codes fall back to English.
    /// </summary>
    public void LoadLanguage(string lang)
    {
        var textAsset = string.IsNullOrEmpty(lang) ? null : Resources.Load<TextAsset>($"{LocalizationResourcesFolder}/{lang}");
        if (textAsset == null && !string.Equals(lang, DefaultLanguageCode, StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[LocalizationManager] Localization file not found: Resources/{LocalizationResourcesFolder}/{lang}. Falling back to '{DefaultLanguageCode}'.");
            lang = DefaultLanguageCode;
            textAsset = Resources.Load<TextAsset>($"{LocalizationResourcesFolder}/{lang}");
        }

        CurrentLanguage = lang;
        _table.Clear();

        // Registered before the table is announced, so every label already finds the CJK glyphs when it refreshes.
        LocalizationFontFallback.Apply(lang);

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
    /// Choosing a language manually stops the automatic Steam / operating system detection.
    /// </summary>
    public void SetLanguage(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return;

        if (!string.Equals(lang, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            LoadLanguage(lang);

        PlayerPrefs.SetString(LanguagePrefsKey, CurrentLanguage);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Tries to get the localized value for the given key in the current language.
    /// </summary>
    public bool TryGet(string key, out string value) => _table.TryGetValue(key, out value);

    /// <summary>
    /// Returns the value for the given key in the current language, or the fallback when the key is empty,
    /// the manager is not available yet or the key has no translation.
    /// </summary>
    public static string GetTextOrFallback(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key) || Instance == null) return fallback;

        return Instance.TryGet(key, out var localizedValue) && !string.IsNullOrEmpty(localizedValue)
            ? localizedValue
            : fallback;
    }

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
