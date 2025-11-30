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
        if (networkManager != null && networkManager.HasConnection())
        {
            Debug.Log("[SceneCleanup] Conexão ativa detectada. Fechando lobby...");
            LobbyCleanupHelper.CloseLobbyProperly();
        }
        
        SyncronizeTable.ResetAll();
        
        Debug.Log("[SceneCleanup] Limpeza concluída");
    }
}
