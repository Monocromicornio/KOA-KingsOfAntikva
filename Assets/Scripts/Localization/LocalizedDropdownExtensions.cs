using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class LocalizedDropdownExtensions
{
    /// <summary>
    /// Sets the dropdown options to their texts in the current language, keeping the selected index
    /// and without invoking onValueChanged.
    /// </summary>
    public static void SetLocalizedOptions(this TMP_Dropdown dropdown, IReadOnlyList<LocalizedString> options)
    {
        int selectedIndex = dropdown.value;

        if (dropdown.options.Count == options.Count)
        {
            for (int i = 0; i < options.Count; i++)
                dropdown.options[i].text = options[i].GetText();
        }
        else
        {
            var labels = new List<string>(options.Count);
            foreach (var option in options)
                labels.Add(option.GetText());

            dropdown.ClearOptions();
            dropdown.AddOptions(labels);
        }

        if (options.Count > 0)
            dropdown.SetValueWithoutNotify(Mathf.Clamp(selectedIndex, 0, options.Count - 1));

        dropdown.RefreshShownValue();
    }
}
