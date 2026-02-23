using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapMarkingSystem : MonoBehaviour
{
    private MinimapController minimapController;
    private string selectedMarking;

    private void Awake()
    {
        minimapController = GetComponentInParent<MinimapController>();
    }

    public void OnMarkingButtonClicked(Button button)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            string marking = buttonText.text.Trim();
            SelectMarking(marking);
        }
    }

    public void SelectMarking(string marking)
    {
        if (selectedMarking == marking)
        {
            selectedMarking = null;
        }
        else
        {
            selectedMarking = marking;
        }

        minimapController?.SetActiveMarking(selectedMarking);
    }

    public void ClearAllMarkings()
    {
        selectedMarking = null;
        minimapController?.ClearAllMarkings();
    }

    public string GetSelectedMarking()
    {
        return selectedMarking;
    }
}
