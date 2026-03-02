using System.Collections;
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

    /// <summary>
    /// Agenda a inicialização do jogo assim que o MatchController estiver disponível,
    /// usando a tabela armazenada em SyncronizeTable.PendingTableData.
    /// </summary>
    public static void ScheduleStartGame()
    {
        if (instance == null)
        {
            Debug.LogError("[AutoLoadController] Instância não encontrada ao tentar agendar StartGame");
            return;
        }

        Debug.Log("[AutoLoadController] Agendando StartGame para quando MatchController estiver pronto");
        instance.StartCoroutine(instance.WaitAndStartGame());
    }

    private IEnumerator WaitAndStartGame()
    {
        const float timeout = 10f;
        float elapsed = 0f;

        while (MatchController.instance == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (MatchController.instance == null)
        {
            Debug.LogError("[AutoLoadController] Timeout aguardando MatchController para iniciar jogo");
            SyncronizeTable.ClearPendingTable();
            yield break;
        }

        TableData data = SyncronizeTable.PendingTableData;
        SyncronizeTable.ClearPendingTable();

        if (data == null)
        {
            Debug.LogError("[AutoLoadController] PendingTableData é null ao tentar iniciar jogo");
            yield break;
        }

        Debug.Log("[AutoLoadController] MatchController pronto, iniciando jogo com tabela pendente");
        MatchController.instance.StartGame(data);
    }
}
