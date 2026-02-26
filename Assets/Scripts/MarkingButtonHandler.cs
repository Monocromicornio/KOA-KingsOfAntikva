using TMPro;
using UnityEngine;

/// <summary>
/// Placed on each MarkingButton. Reads the button's own TMP text and forwards
/// it to the sibling MinimapMarkingSystem, so the onClick event requires no argument.
/// </summary>
public class MarkingButtonHandler : MonoBehaviour
{
    private MinimapMarkingSystem markingSystem;
    private TextMeshProUGUI buttonText;

    private void Awake()
    {
        markingSystem = GetComponentInParent<MinimapMarkingSystem>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>Reads this button's text and forwards it to the parent MinimapMarkingSystem.</summary>
    public void HandleClick()
    {
        if (markingSystem == null || buttonText == null)
            return;

        string marking = buttonText.text.Trim();
        markingSystem.SelectMarking(marking);
    }
}
