using com.onlineobject.objectnet;
using System.Collections;
using UnityEngine;

public class OnlineTurnManager : NetworkBehaviour
{
    public static OnlineTurnManager Instance;
    NetworkManager networkManager => NetworkManager.Instance();
    MatchController matchController => MatchController.instance;

    const int CHANGE_TURN_EVENT = 22980;

    //Subscribe the reciever method to the event
    public override void OnNetworkStarted()
    {
        this.RegisterEvent(CHANGE_TURN_EVENT, this.OnReceivedChangeTurnEvent, true);
    }

    //Event will trigger this method
    private void OnReceivedChangeTurnEvent(IDataStream reader)
    {
        this.ReceiveChangeTurnFromOther();
    }

    //regular method
    private void ReceiveChangeTurnFromOther()
    {
        matchController.ChangeTurnImmediate(); 
    }

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

    public void SetChangeTurn()
    {
        // Execute locally immediately — this peer's turn is ending right now.
        Debug.Log($"[OnlineTurnManager] SetChangeTurn — isServer={networkManager.IsServerConnection()}. Calling ChangeTurnImmediate locally and incrementing counter for remote peer.");
        matchController.ChangeTurnImmediate();

        using (DataStream writer = new DataStream())
        {
            this.Send(CHANGE_TURN_EVENT, writer, DeliveryMode.Reliable);
        }
    }
}
