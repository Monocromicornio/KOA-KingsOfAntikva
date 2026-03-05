using UnityEngine;
using com.onlineobject.objectnet;
using Steamworks;

public class NetworkDiagnostics : MonoBehaviour
{
    [Header("Diagnóstico Automático")]
    [SerializeField]
    private bool enableAutoLogging = true;
    
    [SerializeField]
    private float logInterval = 5f;
    
    private float nextLogTime = 0f;
    
    void Update()
    {
        if (!enableAutoLogging) return;
        
        if (Time.time >= nextLogTime)
        {
            nextLogTime = Time.time + logInterval;
            LogDiagnostics();
        }
    }
    
    [ContextMenu("Log Diagnostics Now")]
    public void LogDiagnostics()
    {
        Debug.Log("=== DIAGNÓSTICO DE REDE ===");
        
        var networkManager = NetworkManager.Instance();
        if (networkManager != null)
        {
            Debug.Log($"NetworkManager.IsConnected: {networkManager.IsConnected()}");
            Debug.Log($"NetworkManager.HasConnection: {networkManager.HasConnection()}");
            Debug.Log($"NetworkManager.IsServerConnection: {networkManager.IsServerConnection()}");
            Debug.Log($"NetworkManager.IsClientConnection: {networkManager.IsClientConnection()}");
        }
        else
        {
            Debug.LogError("NetworkManager é NULL!");
        }
        
        var steamManager = NetworkSteamManager.Instance();
        if (steamManager != null)
        {
            Debug.Log("SteamManager: OK");
        }
        else
        {
            Debug.LogError("SteamManager é NULL!");
        }
        
        Debug.Log($"SyncronizeTable.instance: {(SyncronizeTable.Instance != null ? "OK" : "NULL")}");
        Debug.Log($"LocalSteamId: {SyncronizeTable.LocalSteamId}");
        Debug.Log($"OpponentSteamId: {SyncronizeTable.OpponentSteamId}");
        
        Debug.Log($"Meu Steam ID: {SteamUser.GetSteamID().m_SteamID}");
        Debug.Log($"Meu Steam Nome: {SteamFriends.GetPersonaName()}");
        
        if (MatchController.instance != null)
        {
            Debug.Log($"MatchController.finished: {MatchController.instance.finished}");
            Debug.Log($"MatchController.currentTurn: {MatchController.instance.currentTurn}");
            Debug.Log($"MatchController.myTurn: {MatchController.instance.myTurn}");
        }
        
        Debug.Log("=== FIM DO DIAGNÓSTICO ===");
    }
}
