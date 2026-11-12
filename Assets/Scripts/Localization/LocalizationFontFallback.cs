using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Makes Chinese and Japanese text readable. The game's own TMP fonts only contain Latin glyphs, so the TMP Settings
/// fallback list gets a dynamic CJK font: the one of the current language first, then Simplified Chinese as a catch-all
/// (it also covers the language names shown in the language dropdown: 日本語 / 简体中文 / 繁體中文).
///
/// Where each font comes from, in order:
///  1. A TMP Font Asset at Resources/Localization/Fonts/&lt;language code&gt; (e.g. "zh-hans", "zh-hant", "ja").
///     Drop one there (for example a dynamic Noto Sans SC/TC/JP asset) to ship the same font on every operating system.
///  2. A font installed on the player's operating system (Microsoft YaHei / JhengHei / Yu Gothic on Windows, PingFang /
///     Hiragino on macOS, Noto Sans CJK on Linux). No font file is added to the build.
/// If neither is found a warning is logged once and Chinese / Japanese text will show missing glyphs.
/// </summary>
public static class LocalizationFontFallback
{
    private const string FontAssetsResourcesFolder = "Localization/Fonts";
    private const string CatchAllLanguageCode = LocalizationManager.SimplifiedChineseLanguageCode;

    // Installed font families to try per language, best match first. All are used in their "Regular" style.
    private static readonly Dictionary<string, string[]> OperatingSystemFontFamilies = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        {
            LocalizationManager.SimplifiedChineseLanguageCode,
            new[] { "Microsoft YaHei", "Microsoft YaHei UI", "PingFang SC", "Noto Sans CJK SC", "Noto Sans SC", "Source Han Sans SC", "WenQuanYi Micro Hei", "SimHei" }
        },
        {
            LocalizationManager.TraditionalChineseLanguageCode,
            new[] { "Microsoft JhengHei", "Microsoft JhengHei UI", "PingFang TC", "Noto Sans CJK TC", "Noto Sans TC", "Source Han Sans TC" }
        },
        {
            LocalizationManager.JapaneseLanguageCode,
            new[] { "Yu Gothic", "Yu Gothic UI", "Meiryo", "Hiragino Sans", "Hiragino Kaku Gothic ProN", "Noto Sans CJK JP", "Noto Sans JP", "MS Gothic" }
        },
    };

    private const string OperatingSystemFontStyle = "Regular";

    private static readonly Dictionary<string, TMP_FontAsset> FontCache = new Dictionary<string, TMP_FontAsset>(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> LanguagesWithoutFont = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static readonly List<TMP_FontAsset> RegisteredFallbacks = new List<TMP_FontAsset>();
    private static HashSet<string> _installedFontFamilies;

    /// <summary>
    /// Registers the CJK fallback fonts for the given language in the TMP Settings fallback list,
    /// replacing the ones registered for the previous language. Safe to call repeatedly.
    /// </summary>
    public static void Apply(string languageCode)
    {
        if (TMP_Settings.instance == null) return;

        var wanted = new List<TMP_FontAsset>(2);
        AddIfFound(wanted, GetFontFor(languageCode));
        AddIfFound(wanted, GetFontFor(CatchAllLanguageCode));

        if (SameFonts(wanted, RegisteredFallbacks)) return;

        var previousFonts = new List<TMP_FontAsset>(RegisteredFallbacks);
        Release();

        var settingsFallbacks = TMP_Settings.fallbackFontAssets;
        if (settingsFallbacks == null)
        {
            settingsFallbacks = new List<TMP_FontAsset>();
            TMP_Settings.fallbackFontAssets = settingsFallbacks;
        }

        foreach (var font in wanted)
        {
            settingsFallbacks.Add(font);
            RegisteredFallbacks.Add(font);
        }

        // TMP remembers every glyph it resolved through a fallback inside the source font itself, so a character found in
        // the previous language's font would keep being used (mixing Chinese / Japanese glyph shapes). Clearing a font
        // asset also clears those remembered glyphs in every other font asset; glyphs are re-added on demand.
        foreach (var font in previousFonts)
        {
            if (font != null)
                font.ClearFontAssetData();
        }
    }

    /// <summary>
    /// Removes the fonts added by <see cref="Apply"/> from the TMP Settings asset, so nothing created at runtime
    /// is left referenced from it (this matters in the Editor, where the settings asset outlives Play Mode).
    /// </summary>
    public static void Release()
    {
        if (RegisteredFallbacks.Count == 0) return;

        var settingsFallbacks = TMP_Settings.fallbackFontAssets;
        if (settingsFallbacks != null)
        {
            foreach (var font in RegisteredFallbacks)
                settingsFallbacks.Remove(font);
        }

        RegisteredFallbacks.Clear();
    }

    private static void AddIfFound(List<TMP_FontAsset> fonts, TMP_FontAsset font)
    {
        if (font != null && !fonts.Contains(font))
            fonts.Add(font);
    }

    private static bool SameFonts(List<TMP_FontAsset> a, List<TMP_FontAsset> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    /// <summary>Returns the CJK font of a language, or null when the language is not CJK or no font could be found.</summary>
    private static TMP_FontAsset GetFontFor(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode) || !OperatingSystemFontFamilies.ContainsKey(languageCode))
            return null;

        if (FontCache.TryGetValue(languageCode, out var cached) && cached != null)
            return cached;

        if (LanguagesWithoutFont.Contains(languageCode))
            return null;

        var font = Resources.Load<TMP_FontAsset>($"{FontAssetsResourcesFolder}/{languageCode}");
        if (font == null)
            font = CreateOperatingSystemFont(languageCode);

        if (font == null)
        {
            LanguagesWithoutFont.Add(languageCode);
            Debug.LogWarning($"[LocalizationFontFallback] No CJK font found for '{languageCode}'. Add a TMP Font Asset at Resources/{FontAssetsResourcesFolder}/{languageCode} " +
                             "or install one of: " + string.Join(", ", OperatingSystemFontFamilies[languageCode]));
            return null;
        }

        FontCache[languageCode] = font;
        return font;
    }

    private static TMP_FontAsset CreateOperatingSystemFont(string languageCode)
    {
        var installed = GetInstalledFontFamilies();

        // Pass 0 tries the families the system reports as installed (no "Unable to find a font file" log per miss).
        // Pass 1 tries the remaining names anyway, in case the platform lists its fonts under a different naming.
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (var family in OperatingSystemFontFamilies[languageCode])
            {
                bool listedAsInstalled = installed.Contains(family);
                if (listedAsInstalled != (pass == 0)) continue;

                var font = TMP_FontAsset.CreateFontAsset(family, OperatingSystemFontStyle);
                if (font != null)
                {
                    font.name = $"{family} (CJK fallback, {languageCode})";
                    return font;
                }
            }
        }

        return null;
    }

    private static HashSet<string> GetInstalledFontFamilies()
    {
        if (_installedFontFamilies != null) return _installedFontFamilies;

        _installedFontFamilies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var family in Font.GetOSInstalledFontNames())
                _installedFontFamilies.Add(family);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LocalizationFontFallback] Could not list the installed fonts: {exception.Message}");
        }

        return _installedFontFamilies;
    }
}
