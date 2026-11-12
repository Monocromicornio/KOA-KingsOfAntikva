using TMPro;
using UnityEngine;

/// <summary>
/// Makes a TextMeshPro text fit the box it was designed for, in any language:
///  - keeps free space at the left and right edges, so the text does not touch the frame of a button or panel;
///  - never breaks a single word in the middle (a one-word label shrinks instead of wrapping);
///  - shrinks the font only as much as needed, never below <c>minScale</c> x the designed font size.
/// A text that already uses TMP's own auto size keeps it (TMP then fits it by itself); only the side padding is added.
/// </summary>
public static class TextFitter
{
    /// <summary>Most free space (in canvas units) kept at each side, so a very wide box does not get a huge margin.</summary>
    private const float MaxSidePadding = 40f;
    private const float SmallestFontSize = 8f;
    private const float Tolerance = 0.5f;
    private const int SearchSteps = 8;

    /// <summary>The font size and margin a label was designed with in the scene.</summary>
    public struct Design
    {
        public float FontSize;
        public Vector4 Margin; // left, top, right, bottom
    }

    /// <summary>Reads the designed font size and margin of a label that was not fitted yet.</summary>
    public static Design Capture(TMP_Text text)
    {
        return new Design
        {
            FontSize = text.enableAutoSizing ? text.fontSizeMax : text.fontSize,
            Margin = text.margin,
        };
    }

    /// <summary>
    /// Sets the text and fits it to its box. Returns the font size used.
    /// If the box has no size yet (the layout was not built), only the text is set: call it again when the size changes.
    /// </summary>
    public static float Fit(TMP_Text text, string value, Design design, float minScale, float sidePaddingFraction)
    {
        value = value ?? string.Empty;
        text.textWrappingMode = CanWrap(value) ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        text.text = value;

        var rect = text.rectTransform.rect;
        if (design.FontSize <= 0f || rect.width < 1f || rect.height < 1f) return text.fontSize;

        float sidePadding = Mathf.Min(rect.width * sidePaddingFraction, MaxSidePadding);
        var margin = design.Margin;
        margin.x = Mathf.Max(margin.x, sidePadding);
        margin.z = Mathf.Max(margin.z, sidePadding);
        text.margin = margin;

        // The designer chose TMP's auto size for this label: TMP fits width and height by itself, honoring the margin.
        if (text.enableAutoSizing) return text.fontSize;

        float availableWidth = rect.width - margin.x - margin.z;
        float availableHeight = rect.height - margin.y - margin.w;
        float largest = design.FontSize;
        float smallest = Mathf.Max(SmallestFontSize, largest * Mathf.Clamp01(minScale));

        text.fontSize = largest;
        if (Fits(text, value, availableWidth, availableHeight)) return largest;

        text.fontSize = smallest;
        if (!Fits(text, value, availableWidth, availableHeight)) return smallest; // best effort: it does not fit even at the smallest size

        float low = smallest, high = largest;
        for (int step = 0; step < SearchSteps; step++)
        {
            float middle = (low + high) * 0.5f;
            text.fontSize = middle;
            if (Fits(text, value, availableWidth, availableHeight)) low = middle; else high = middle;
        }

        text.fontSize = low;
        return low;
    }

    /// <summary>
    /// True when the text has a place to wrap: a space, a line break, or Chinese / Japanese / Korean characters
    /// (which can break between any two characters). A single Latin word must not be split, so it never wraps.
    /// </summary>
    public static bool CanWrap(string value)
    {
        foreach (char character in value)
        {
            if (character == ' ' || character == '\n' || IsCjk(character)) return true;
        }
        return false;
    }

    private static bool IsCjk(char character) => character >= '⺀';

    private static bool Fits(TMP_Text text, string value, float availableWidth, float availableHeight)
    {
        Vector2 preferred = text.GetPreferredValues(value, availableWidth, 0f);
        float lineHeight = text.GetPreferredValues("Ag", 0f, 0f).y; // one line at the current size: a box is never asked to hold less
        if (preferred.x > availableWidth + Tolerance || preferred.y > Mathf.Max(availableHeight, lineHeight) + Tolerance) return false;

        // With wrapping on, TMP splits a word that is wider than the line; treat that as not fitting.
        string longestWord = LongestWord(value);
        return longestWord.Length == 0 || text.GetPreferredValues(longestWord, 0f, 0f).x <= availableWidth + Tolerance;
    }

    private static string LongestWord(string value)
    {
        string longest = string.Empty;
        foreach (string word in value.Split(' ', '\n'))
        {
            if (word.Length > longest.Length && !ContainsCjk(word)) longest = word;
        }
        return longest;
    }

    private static bool ContainsCjk(string value)
    {
        foreach (char character in value)
        {
            if (IsCjk(character)) return true;
        }
        return false;
    }
}
