/// <summary>
/// A text defined in code whose value comes from the localization table, with a fallback used when the key is empty or missing.
/// </summary>
public readonly struct LocalizedString
{
    public readonly string Key;
    public readonly string Fallback;

    public LocalizedString(string key, string fallback)
    {
        Key = key;
        Fallback = fallback;
    }

    /// <summary>
    /// True when the text has a localization key (literal texts such as numbers are never translated).
    /// </summary>
    public bool IsTranslatable => !string.IsNullOrEmpty(Key);

    /// <summary>
    /// Creates a text that is shown as-is in every language (numbers, resolutions, "2x"...).
    /// </summary>
    public static LocalizedString Literal(string text) => new LocalizedString(null, text);

    /// <summary>
    /// Returns the text in the current language.
    /// </summary>
    public string GetText() => LocalizationManager.GetTextOrFallback(Key, Fallback);
}
