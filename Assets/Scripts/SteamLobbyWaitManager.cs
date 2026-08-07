using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace com.onlineobject.objectnet.integration
{
    public class SteamLobbyWaitManager : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI statusText;
        public Button leaveLobbyButton;

        [Header("Settings")]
        public int minPlayersToStart = 2;
        public string gameSceneName = "Game";

        [Header("Optional")]
        public SavePieceOrder savePieceOrder;

        private bool isWaitingForPlayers = false;
        private bool hasStartedGame = false;

#if STEAMWORKS_NET
        private Callback<LobbyChatUpdate_t> lobbyChatUpdate;
        private CSteamID currentLobbyID;

        private void Start()
        {
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

            if (leaveLobbyButton != null)
            {
                leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
            }
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (!isWaitingForPlayers || hasStartedGame)
                return;

            if (callback.m_ulSteamIDLobby != currentLobbyID.m_SteamID)
                return;

            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

            if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered ||
                stateChange == EChatMemberStateChange.k_EChatMemberStateChangeLeft ||
                stateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                UpdatePlayerCount();
            }
        }

        public void StartWaitingForPlayers(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isWaitingForPlayers = true;
            hasStartedGame = false;

            Debug.Log("[SteamLobbyWait] Aguardando jogadores no lobby: " + lobbyID);

            // Atualiza status com o nome da sala
            if (statusText != null)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData(currentLobbyID, "LobbyName");
                if (!string.IsNullOrEmpty(lobbyName))
                {
                    statusText.text = string.Format("Sala ({0})", lobbyName);
                }
                else
                {
                    statusText.text = "Waiting for players...";
                }
            }

            UpdatePlayerCount();
        }

        private void UpdatePlayerCount()
        {
            if (!isWaitingForPlayers || currentLobbyID.m_SteamID == 0)
                return;

            int playerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

            Debug.Log(string.Format("[SteamLobbyWait] Jogadores: {0}/{1}", playerCount, minPlayersToStart));

            if (playerCount >= minPlayersToStart && !hasStartedGame)
            {
                if (statusText != null)
                {
                    statusText.text = "Starting match...";
                }
                Invoke("StartGame", 1f);
            }
        }

        private void StartGame()
        {
            if (hasStartedGame)
                return;

            hasStartedGame = true;
            isWaitingForPlayers = false;

            Debug.Log("[SteamLobbyWait] Iniciando jogo!");


            NetworkAutoLoadController.EnableAutoLoadForGame();

            //if (waitingPanel != null)
           // {
            //    waitingPanel.SetActive(false);
          //  }

            //NetworkAutoLoadController.EnableAutoLoadForGame();


            if (savePieceOrder != null)
            {
                savePieceOrder.enabled = true;
                Debug.Log("[SteamLobbyWait] SavePieceOrder reabilitado");
            }

            Debug.Log("Loading game scen via Steam Lobby Wait Manager");
            SceneLoadingHandler.LoadSceneWithLoading(gameSceneName, "Starting match...");
        }

        private void OnLeaveLobby()
        {
            Debug.Log("[SteamLobbyWait] Saindo do lobby...");

            if (NetworkSteamManager.Instance() != null)
            {
                NetworkSteamManager.Instance().LeaveLobby();
            }

            isWaitingForPlayers = false;
            hasStartedGame = false;
            currentLobbyID = default(CSteamID);
        }
#endif
    }
}
