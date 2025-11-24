using UnityEngine;
using com.onlineobject.objectnet;

public class SceneCleanup : MonoBehaviour
{
    [SerializeField]
    private bool cleanupOnAwake = true;
    
    void Awake()
    {
        if (cleanupOnAwake)
        {
            CleanupNetworkState();
        }
    }
    
    [ContextMenu("Cleanup Network State")]
    public void CleanupNetworkState()
    {
        Debug.Log("[SceneCleanup] Iniciando limpeza de estado de rede...");
        
        var networkManager = NetworkManager.Instance();
        if (networkManager != null)
        {
            if (networkManager.HasConnection())
            {
                Debug.Log("[SceneCleanup] Conexão ativa detectada. Desconectando...");
                
                try
                {
                    var steamManager = NetworkSteamManager.Instance();
                    if (steamManager != null)
                    {
                        steamManager.LeaveLobby();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SceneCleanup] Erro ao sair do lobby: {e.Message}");
                }
                
                try
                {
                    networkManager.StopNetwork();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SceneCleanup] Erro ao parar rede: {e.Message}");
                }
            }
        }
        
        SyncronizeTable.ResetAll();
        
        Debug.Log("[SceneCleanup] Limpeza concluída");
    }
}
