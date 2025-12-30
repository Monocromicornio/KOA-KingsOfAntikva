using UnityEngine;

public class OfflineSyncronizeTableStub : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[OfflineSyncronizeTableStub] Synchronization disabled for offline mode");
    }

    public void SyncTable()
    {
        Debug.Log("[OfflineSyncronizeTableStub] SyncTable called (no-op in offline mode)");
    }

    public void SendTableUpdate()
    {
        Debug.Log("[OfflineSyncronizeTableStub] SendTableUpdate called (no-op in offline mode)");
    }
}
