using UnityEngine;
using com.onlineobject.objectnet;

public class NetworkAutoLoadController : MonoBehaviour
{
    private static NetworkAutoLoadController instance;
    private bool autoLoadDisabledForMatchmaking = false;

    private NetworkGlobalEvents networkEvents;
    private EventReference originalOnConnected;
    private EventReference originalOnServerStarted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        networkEvents = FindObjectOfType<NetworkGlobalEvents>();
        if (networkEvents != null)
        {
            originalOnConnected = networkEvents.onConnected;
            originalOnServerStarted = networkEvents.onServerStarted;
        }
    }

    public static void DisableAutoLoadForMatchmaking()
    {
        if (instance == null) return;

        if (NetworkManager.Instance() != null)
        {
            NetworkManager.Instance().DisableAutoLoadSceneElements();
        }

        if (instance.networkEvents != null)
        {
            instance.networkEvents.onConnected = null;
            instance.networkEvents.onServerStarted = null;
            Debug.Log("[AutoLoadController] EventReferences onConnected e onServerStarted desabilitados");
        }

        instance.autoLoadDisabledForMatchmaking = true;
        Debug.Log("[AutoLoadController] Auto-load desabilitado para matchmaking");
    }

    public static void EnableAutoLoadForGame()
    {
        if (instance == null) return;

        if (NetworkManager.Instance() != null)
        {
            NetworkManager.Instance().EnableAutoLoadSceneElements();
        }

        if (instance.networkEvents != null)
        {
            instance.networkEvents.onConnected = instance.originalOnConnected;
            instance.networkEvents.onServerStarted = instance.originalOnServerStarted;
            Debug.Log("[AutoLoadController] EventReferences onConnected e onServerStarted reabilitados");
        }

        instance.autoLoadDisabledForMatchmaking = false;
        Debug.Log("[AutoLoadController] Auto-load reabilitado para iniciar jogo");
    }

    public static bool IsAutoLoadDisabledForMatchmaking()
    {
        return instance != null && instance.autoLoadDisabledForMatchmaking;
    }
}
