using UnityEngine;
using System.Collections;

public class GameLoadingCoordinator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float maxWaitTime = 15f;

    [SerializeField]
    private bool waitForOpponentProfile = true;

    [SerializeField]
    private bool waitForMatchController = true;

    private bool localPlayerReady = false;
    private bool opponentReady = false;
    private float waitStartTime;

    private void Awake()
    {
        OpponentProfileLoader.OnOpponentProfileLoaded += OnOpponentLoaded;
    }

    private void OnDestroy()
    {
        OpponentProfileLoader.OnOpponentProfileLoaded -= OnOpponentLoaded;
        
        if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsShowing())
        {
            SceneLoadingHandler.HideLoadingScreen();
        }
    }

    private void OnOpponentLoaded()
    {
        opponentReady = true;
        Debug.Log("[GameLoadingCoordinator] Evento de perfil do oponente recebido");
    }

    private void Start()
    {
        waitStartTime = Time.time;

        SceneLoadingHandler.ShowLoadingScreen("Aguardando oponente...");
        SceneLoadingHandler.UpdateLoadingProgress(0.5f);

        StartCoroutine(CheckGameReady());
    }

    private IEnumerator CheckGameReady()
    {
        bool isOnline = IsOnlineGame();

        if (!isOnline)
        {
            Debug.Log("[GameLoadingCoordinator] Jogo offline detectado, ocultando loading screen imediatamente");
            yield return new WaitForSeconds(0.5f);
            SceneLoadingHandler.HideLoadingScreen();
            yield break;
        }

        SceneLoadingHandler.SetLoadingStatus("Conectando com oponente...");

        bool matchControllerReady = false;
        bool opponentProfileReady = false;

        while (Time.time - waitStartTime < maxWaitTime)
        {
            if (waitForMatchController && !matchControllerReady)
            {
                matchControllerReady = CheckMatchControllerReady();
                if (matchControllerReady)
                {
                    Debug.Log("[GameLoadingCoordinator] MatchController pronto");
                    SceneLoadingHandler.UpdateLoadingProgress(0.7f);
                }
            }

            if (waitForOpponentProfile && !opponentProfileReady)
            {
                opponentProfileReady = CheckOpponentProfileReady() || opponentReady;
                if (opponentProfileReady)
                {
                    Debug.Log("[GameLoadingCoordinator] Perfil do oponente carregado");
                    SceneLoadingHandler.UpdateLoadingProgress(0.9f);
                    SceneLoadingHandler.SetLoadingStatus("Preparando partida...");
                }
            }

            bool allReady = (!waitForMatchController || matchControllerReady) &&
                           (!waitForOpponentProfile || opponentProfileReady);

            if (allReady)
            {
                Debug.Log("[GameLoadingCoordinator] Tudo pronto! Ocultando loading screen");
                SceneLoadingHandler.UpdateLoadingProgress(1f);
                yield return new WaitForSeconds(0.5f);
                SceneLoadingHandler.HideLoadingScreen();
                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }

        Debug.LogWarning($"[GameLoadingCoordinator] Timeout após {maxWaitTime} segundos, ocultando loading screen");
        SceneLoadingHandler.HideLoadingScreen();
    }

    private bool IsOnlineGame()
    {
        if (MatchController.instance == null)
            return false;

        return MatchController.instance.hasConnection;
    }

    private bool CheckMatchControllerReady()
    {
        if (MatchController.instance == null)
            return false;

        return MatchController.instance.currentTurn != TurnState.wait;
    }

    private bool CheckOpponentProfileReady()
    {
        if (!IsOnlineGame())
            return true;

        return SyncronizeTable.OpponentSteamId != 0;
    }
}
