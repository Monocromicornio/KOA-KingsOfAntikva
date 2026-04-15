using com.onlineobject.objectnet;
using UnityEngine;

public class OnlineTurnManager : NetworkBehaviour
{
    public static OnlineTurnManager Instance;
    NetworkManager networkManager => NetworkManager.Instance();
    MatchController matchController => MatchController.instance;

    private NetworkVariable<int> serverTurnCounter = 0;
    private NetworkVariable<int> clientTurnCounter = 0;

    private bool turnCallbackRegistered = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[OnlineTurnManager] Duplicate instance detected — destroying new one. Only the network-instantiated instance should exist per match.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void ActiveUpdate()
    {
        if (turnCallbackRegistered) return;
        turnCallbackRegistered = true;
        Debug.Log("[OnlineTurnManager] Active udpate fired. Am i Server connection? " + NetworkManager.Instance().IsServerConnection());

        if (networkManager.IsServerConnection())
        {
            clientTurnCounter.OnValueChange((int oldValue, int newValue) =>
            {
                Debug.Log($"[OnlineTurnManager] clientTurnCounter changed {oldValue} → {newValue} (host received client turn end) — calling ChangeTurnImmediate.");
                matchController.ChangeTurnImmediate();
            });
        }
        else
        {
            // Client receives this when the HOST increments their counter.
            serverTurnCounter.OnValueChange((int oldValue, int newValue) =>
            {
                Debug.Log($"[OnlineTurnManager] serverTurnCounter changed {oldValue} → {newValue} (client received host turn end) — calling ChangeTurnImmediate.");
                matchController.ChangeTurnImmediate();
            });
        }
    }


    // Called every frame on the client (non-owner of this NetworkObject).
    private void PassiveUpdate()
    {
        if (turnCallbackRegistered) return;
        turnCallbackRegistered = true;
        Debug.Log("[OnlineTurnManager] Passive udpate fired. Am i Server connection? " + NetworkManager.Instance().IsServerConnection());

        if (NetworkManager.Instance().IsServerConnection())
        {
            clientTurnCounter.OnValueChange((int oldValue, int newValue) =>
            {
                Debug.Log($"[OnlineTurnManager] clientTurnCounter changed {oldValue} → {newValue} (host received client turn end) — calling ChangeTurnImmediate.");
                matchController.ChangeTurnImmediate();
            });
        }
        else
        {
            // Client receives this when the HOST increments their counter.
            serverTurnCounter.OnValueChange((int oldValue, int newValue) =>
            {
                Debug.Log($"[OnlineTurnManager] serverTurnCounter changed {oldValue} → {newValue} (client received host turn end) — calling ChangeTurnImmediate.");
                matchController.ChangeTurnImmediate();
            });
        }
    }

    public void SetChangeTurn()
    {
        // Execute locally immediately — this peer's turn is ending right now.
        Debug.Log($"[OnlineTurnManager] SetChangeTurn — isServer={networkManager.IsServerConnection()}. Calling ChangeTurnImmediate locally and incrementing counter for remote peer.");
        matchController.ChangeTurnImmediate();

        // Increment the counter that belongs to this peer so the other peer's OnValueChange fires.
        if (networkManager.IsServerConnection())
        {
            Debug.Log("[OnlineTurnManager] Increasing server Turn Counter");
           // TakeControl();
            serverTurnCounter.SetValue((int)serverTurnCounter + 1);
        }
        else
        {
            Debug.Log("[OnlineTurnManager] Increasing client Turn Counter");
         //   TakeControl();
            clientTurnCounter.SetValue((int)clientTurnCounter + 1);

        }
    }
}
