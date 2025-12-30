using UnityEngine;
using com.onlineobject.objectnet;

public class OfflineNetworkManagerStub : NetworkManager
{
    private static OfflineNetworkManagerStub instance;

    public static new OfflineNetworkManagerStub Instance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("OfflineNetworkManager");
            instance = obj.AddComponent<OfflineNetworkManagerStub>();
            DontDestroyOnLoad(obj);
        }
        return instance;
    }

    public new bool HasConnection()
    {
        return false;
    }

    public new bool IsConnected()
    {
        return false;
    }

    public new bool IsServerConnection()
    {
        return false;
    }

    public new bool IsClientConnection()
    {
        return false;
    }
}
