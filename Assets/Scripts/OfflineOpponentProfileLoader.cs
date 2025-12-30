using UnityEngine;
using TMPro;

public class OfflineOpponentProfileLoader : MonoBehaviour
{
    [Header("References")]
    public TMP_Text opponentNameText;
    public TMP_Text opponentLevelText;

    [Header("Offline Settings")]
    public string offlineOpponentName = "Oponente";
    public int offlineOpponentLevel = 1;

    private void Start()
    {
        if (opponentNameText != null)
        {
            opponentNameText.text = offlineOpponentName;
        }

        if (opponentLevelText != null)
        {
            opponentLevelText.text = $"Nível {offlineOpponentLevel}";
        }

        Debug.Log($"[OfflineOpponentProfileLoader] Displaying: {offlineOpponentName}, Level {offlineOpponentLevel}");
    }
}
