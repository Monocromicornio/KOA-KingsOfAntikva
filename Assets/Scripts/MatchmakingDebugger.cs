using UnityEngine;
using UnityEngine.SceneManagement;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace com.onlineobject.objectnet.integration
{
    public class MatchmakingDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugLogs = true;
        public bool showOnScreenDebug = true;
        public KeyCode debugKey = KeyCode.F5;

        private string debugInfo = "";
        private GUIStyle guiStyle;

        private void Start()
        {
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 16;
            guiStyle.normal.textColor = Color.white;
            guiStyle.padding = new RectOffset(10, 10, 10, 10);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log("═══════════════════════════════════════");
            Log($"CENA CARREGADA: {scene.name}");
            Log($"Modo: {mode}");
            Log("═══════════════════════════════════════");
        }

        private void Update()
        {
            if (Input.GetKeyDown(debugKey))
            {
                PrintDebugInfo();
            }

            if (showOnScreenDebug)
            {
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugInfo()
        {
            debugInfo = "═══ MATCHMAKING DEBUG ═══\n\n";
            debugInfo += $"Cena Atual: {SceneManager.GetActiveScene().name}\n";

#if STEAMWORKS_NET
            if (NetworkManager.Instance() != null)
            {
                debugInfo += $"NetworkManager Conectado: {NetworkManager.Instance().IsConnected()}\n";
                debugInfo += $"É Servidor: {NetworkManager.Instance().IsServerConnection()}\n";
                debugInfo += $"É Cliente: {NetworkManager.Instance().IsClientConnection()}\n";
                debugInfo += $"Auto-Load Ativo: {NetworkManager.Instance().IsToAutoLoadSceneElements()}\n";
            }
            else
            {
                debugInfo += "NetworkManager: NULL\n";
            }

            if (NetworkSteamManager.Instance() != null)
            {
                var field = NetworkSteamManager.Instance().GetType()
                    .GetField("currentLobbyID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    ulong lobbyID = (ulong)field.GetValue(NetworkSteamManager.Instance());
                    debugInfo += $"\nLobby ID: {lobbyID}\n";

                    if (lobbyID != 0)
                    {
                        CSteamID steamLobbyID = new CSteamID(lobbyID);
                        int playerCount = SteamMatchmaking.GetNumLobbyMembers(steamLobbyID);
                        debugInfo += $"Jogadores no Lobby: {playerCount}\n";
                        
                        debugInfo += "\nJogadores:\n";
                        for (int i = 0; i < playerCount; i++)
                        {
                            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(steamLobbyID, i);
                            string name = SteamFriends.GetFriendPersonaName(memberId);
                            debugInfo += $"  {i+1}. {name}\n";
                        }
                    }
                }
            }
            else
            {
                debugInfo += "\nNetworkSteamManager: NULL\n";
            }

            var waitManager = FindObjectOfType<SteamLobbyWaitManager>();
            if (waitManager != null)
            {
                debugInfo += "\nSteamLobbyWaitManager: ENCONTRADO\n";
                debugInfo += $"Min Players: {waitManager.minPlayersToStart}\n";
                debugInfo += $"Scene Name: {waitManager.gameSceneName}\n";
            }
            else
            {
                debugInfo += "\nSteamLobbyWaitManager: NÃO ENCONTRADO ⚠️\n";
            }
#else
            debugInfo += "\nSTEAMWORKS_NET NÃO DEFINIDO!\n";
#endif

            debugInfo += $"\nPressione {debugKey} para log detalhado";
        }

        private void PrintDebugInfo()
        {
            Log("═══════════════════════════════════════");
            Log("INFORMAÇÕES DE DEBUG DO MATCHMAKING");
            Log("═══════════════════════════════════════");
            Log($"Cena Atual: {SceneManager.GetActiveScene().name}");
            Log($"Build Index: {SceneManager.GetActiveScene().buildIndex}");

#if STEAMWORKS_NET
            if (NetworkManager.Instance() != null)
            {
                Log("\n--- NETWORK MANAGER ---");
                Log($"Conectado: {NetworkManager.Instance().IsConnected()}");
                Log($"Servidor: {NetworkManager.Instance().IsServerConnection()}");
                Log($"Cliente: {NetworkManager.Instance().IsClientConnection()}");
                Log($"Auto-Load Cenas: {NetworkManager.Instance().IsToAutoLoadSceneElements()}");
                Log($"Modo Embutido: {NetworkManager.Instance().InEmbeddedMode()}");
            }

            if (NetworkSteamManager.Instance() != null)
            {
                Log("\n--- STEAM MANAGER ---");
                
                var field = NetworkSteamManager.Instance().GetType()
                    .GetField("currentLobbyID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    ulong lobbyID = (ulong)field.GetValue(NetworkSteamManager.Instance());
                    Log($"Lobby ID: {lobbyID}");

                    if (lobbyID != 0)
                    {
                        CSteamID steamLobbyID = new CSteamID(lobbyID);
                        int playerCount = SteamMatchmaking.GetNumLobbyMembers(steamLobbyID);
                        Log($"Número de Jogadores: {playerCount}");
                        
                        for (int i = 0; i < playerCount; i++)
                        {
                            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(steamLobbyID, i);
                            string name = SteamFriends.GetFriendPersonaName(memberId);
                            Log($"  Jogador {i+1}: {name} (ID: {memberId})");
                        }
                    }
                }
            }

            var waitManager = FindObjectOfType<SteamLobbyWaitManager>();
            if (waitManager != null)
            {
                Log("\n--- STEAM LOBBY WAIT MANAGER ---");
                Log("Componente encontrado!");
                Log($"Min Players: {waitManager.minPlayersToStart}");
                Log($"Game Scene: {waitManager.gameSceneName}");
                Log($"Waiting Panel: {(waitManager.waitingPanel != null ? "Configurado" : "NULL ⚠️")}");
                Log($"Player Count Text: {(waitManager.playerCountText != null ? "Configurado" : "NULL ⚠️")}");
                Log($"Status Text: {(waitManager.statusText != null ? "Configurado" : "NULL ⚠️")}");
            }
            else
            {
                Log("\n⚠️ STEAM LOBBY WAIT MANAGER NÃO ENCONTRADO!");
            }

            var lobbyList = FindObjectOfType<UISteamLobbyList>();
            if (lobbyList != null)
            {
                Log("\n--- UI STEAM LOBBY LIST ---");
                Log("Componente encontrado!");
                
                var waitField = lobbyList.GetType()
                    .GetField("steamLobbyWaitManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (waitField != null)
                {
                    var value = waitField.GetValue(lobbyList);
                    Log($"Steam Lobby Wait Manager Ref: {(value != null ? "Configurado ✓" : "NULL ⚠️")}");
                }

                var savePieceField = lobbyList.GetType()
                    .GetField("savePieceOrder", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (savePieceField != null)
                {
                    var savePieceValue = savePieceField.GetValue(lobbyList);
                    Log($"Save Piece Order Ref: {(savePieceValue != null ? "Configurado ✓" : "NULL ⚠️")}");
                    
                    if (savePieceValue != null)
                    {
                        SavePieceOrder spo = savePieceValue as SavePieceOrder;
                        Log($"Save Piece Order Enabled: {(spo.enabled ? "SIM (vai carregar cena!)" : "NÃO (matchmaking modo)")}");
                    }
                }
            }
#endif

            Log("═══════════════════════════════════════");
        }

        private void OnGUI()
        {
            if (showOnScreenDebug && !string.IsNullOrEmpty(debugInfo))
            {
                GUI.Box(new Rect(10, 10, 400, 500), "");
                GUI.Label(new Rect(20, 20, 380, 480), debugInfo, guiStyle);
            }
        }

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[MatchmakingDebug] {message}");
            }
        }
    }
}
