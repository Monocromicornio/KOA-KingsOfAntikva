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

    private IEnumerator Start()
    {
        waitStartTime = Time.time;

        SceneLoadingHandler.ShowLoadingScreen("Waiting for opponent...");
        SceneLoadingHandler.UpdateLoadingProgress(0.5f);

        yield return StartCoroutine(CheckGameReady());

        if (CheckMatchControllerReady())
        {
            MatchController.instance.isLoadingScreenFinished = true;
        }
    }

    private IEnumerator CheckGameReady()
    {
        bool isOnline = IsOnlineGame();

        if (!isOnline)
        {
            Debug.Log("[GameLoadingCoordinator] Jogo offline detectado, aguardando peças carregarem...");
            SceneLoadingHandler.SetLoadingStatus("Preparing the board...");
            
            yield return new WaitForSeconds(0.5f);
            
            bool piecesLoaded = false;
            float offlineWaitTime = 0f;
            float maxOfflineWait = 3f;
            
            while (!piecesLoaded && offlineWaitTime < maxOfflineWait)
            {
                piecesLoaded = CheckPiecesLoaded();
                
                if (piecesLoaded)
                {
                    Debug.Log("[GameLoadingCoordinator] Peças carregadas no modo offline");
                    SceneLoadingHandler.UpdateLoadingProgress(0.9f);
                    yield return new WaitForSeconds(0.3f);
                    break;
                }
                
                offlineWaitTime += Time.deltaTime;
                float progress = 0.5f + (offlineWaitTime / maxOfflineWait) * 0.4f;
                SceneLoadingHandler.UpdateLoadingProgress(progress);
                
                yield return null;
            }
            
            SceneLoadingHandler.UpdateLoadingProgress(1f);
            yield return new WaitForSeconds(0.2f);
            SceneLoadingHandler.HideLoadingScreen();
            yield break;
        }

        SceneLoadingHandler.SetLoadingStatus("Connecting to opponent...");

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
                    SceneLoadingHandler.SetLoadingStatus("Preparing match...");
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

        return MatchController.instance.hasStarted == true;
    }

    private bool CheckOpponentProfileReady()
    {
        if (!IsOnlineGame())
            return true;

        return SyncronizeTable.OpponentSteamId != 0;
    }

    private bool CheckPiecesLoaded()
    {
        if (MatchController.instance == null)
            return false;

        PlayerSquad playerSquad = MatchController.instance.playerSquad;
        EnemySquad enemySquad = MatchController.instance.enemySquad;

        if (playerSquad == null || enemySquad == null)
            return false;

        bool playerPiecesLoaded = playerSquad.pieces != null && playerSquad.pieces.Count > 0;
        bool enemyPiecesLoaded = enemySquad.pieces != null && enemySquad.pieces.Count > 0;

        return playerPiecesLoaded && enemyPiecesLoaded;
    }
}
