using UnityEngine;
using TMPro;

public class OfflinePlayerProfileDisplay : MonoBehaviour
{
    [Header("References")]
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;

    [Header("Offline Settings")]
    public string offlinePlayerName = "Jogador";
    public int offlinePlayerLevel = 1;

    private void Start()
    {
        if (playerNameText != null)
        {
            playerNameText.text = offlinePlayerName;
        }

        if (playerLevelText != null)
        {
            playerLevelText.text = $"Nível {offlinePlayerLevel}";
        }

        Debug.Log($"[OfflinePlayerProfileDisplay] Displaying: {offlinePlayerName}, Level {offlinePlayerLevel}");
    }
}
