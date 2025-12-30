using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class OfflineGameManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[OfflineGameManager] Initializing offline mode...");

        if (FindFirstObjectByType<OfflineMatchController>() == null)
        {
            GameObject matchControllerObj = new GameObject("OfflineMatchController");
            matchControllerObj.AddComponent<OfflineMatchController>();
            Debug.Log("[OfflineGameManager] OfflineMatchController created");
        }

        if (OfflineNetworkManagerStub.Instance() == null)
        {
            Debug.LogError("[OfflineGameManager] Failed to create OfflineNetworkManagerStub!");
        }
        else
        {
            Debug.Log("[OfflineGameManager] OfflineNetworkManagerStub created");
        }
    }
}
