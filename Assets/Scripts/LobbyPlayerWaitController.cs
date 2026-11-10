using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace com.onlineobject.objectnet.integration
{
    public class LobbyPlayerWaitController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI statusText;
        public Button leaveLobbyButton;

        [Header("Settings")]
        public int minPlayersToStart = 2;
        public string gameSceneName = "Game";
        public float checkInterval = 0.5f;

        private bool isInLobby = false;
        private int currentPlayerCount = 0;
        private bool hasStartedGame = false;
        private float nextCheckTime = 0f;

#if STEAMWORKS_NET
        private CSteamID currentLobbyID;

        private void Awake()
        {
            if (leaveLobbyButton != null)
            {
                leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
            }
        }

        private void Update()
        {
            if (!isInLobby || hasStartedGame)
                return;

            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                UpdatePlayerCount();
            }
        }

        public void OnLobbyCreated(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isInLobby = true;
            hasStartedGame = false;

            Debug.Log("[LobbyPlayerWait] Lobby criado: " + lobbyID);

            UpdateRoomStatusText();
            UpdatePlayerCount();
        }

        public void OnLobbyJoined(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isInLobby = true;
            hasStartedGame = false;

            Debug.Log("[LobbyPlayerWait] Entrou no lobby: " + lobbyID);

            UpdateRoomStatusText();
            UpdatePlayerCount();
        }

        /// <summary>Atualiza o status com o nome da sala (ou a mensagem de espera), no idioma atual.</summary>
        private void UpdateRoomStatusText()
        {
            if (statusText == null) return;

            string lobbyName = SteamMatchmaking.GetLobbyData(currentLobbyID, "LobbyName");
            statusText.text = string.IsNullOrEmpty(lobbyName)
                ? GameTexts.LobbyWaitingForPlayers.GetText()
                : string.Format(GameTexts.LobbyRoomLabelFormat.GetText(), lobbyName);
        }

        private void UpdatePlayerCount()
        {
            if (!isInLobby || currentLobbyID.m_SteamID == 0)
                return;

            currentPlayerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

            Debug.Log(string.Format("[LobbyPlayerWait] Jogadores no lobby: {0}/{1}", currentPlayerCount, minPlayersToStart));

            if (currentPlayerCount >= minPlayersToStart && !hasStartedGame)
            {
                if (statusText != null)
                {
                    statusText.text = GameTexts.LobbyStartingMatch.GetText();
                }
                StartGame();
            }
        }

        private void StartGame()
        {
            hasStartedGame = true;

            Debug.Log("[LobbyPlayerWait] Iniciando jogo! Jogadores: " + currentPlayerCount);

            if (NetworkManager.Instance().IsServerConnection() && !NetworkManager.Instance().IsConnected())
            {
                NetworkManager.Instance().StartNetwork();
            }

            Debug.Log("Loading game scen via Lobby Player Wait Controller");
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>Sai do lobby atual. Ignorado quando este controlador não está em um lobby.</summary>
        public void OnLeaveLobby()
        {
            if (!isInLobby)
                return;

            Debug.Log("[LobbyPlayerWait] Saindo do lobby...");

            LobbyCleanupHelper.CloseLobbyProperly();

            isInLobby = false;
            hasStartedGame = false;
            currentLobbyID = default(CSteamID);
        }

        public void ForceStartGame()
        {
            if (isInLobby && !hasStartedGame)
            {
                Debug.Log("[LobbyPlayerWait] Forçando início do jogo...");
                StartGame();
            }
        }
#endif
    }
}
