using UnityEngine;
using com.onlineobject.objectnet;

#if STEAMWORKS_NET
using Steamworks;
#endif

public static class LobbyCleanupHelper
{
    public static void CloseLobbyProperly()
    {
        Debug.Log("[LobbyCleanupHelper] Fechando lobby adequadamente...");
        
#if STEAMWORKS_NET
        var steamManager = NetworkSteamManager.Instance();
        if (steamManager == null)
        {
            Debug.LogWarning("[LobbyCleanupHelper] NetworkSteamManager não encontrado");
            return;
        }
        
        var networkManager = NetworkManager.Instance();
        if (networkManager == null)
        {
            Debug.LogWarning("[LobbyCleanupHelper] NetworkManager não encontrado");
            return;
        }
        
        bool wasConnected = networkManager.HasConnection();
        
        if (wasConnected)
        {
            bool isHost = steamManager.IsHostInstance() || networkManager.IsServerConnection();
            
            if (isHost)
            {
                Debug.Log("[LobbyCleanupHelper] Você é o HOST - Fechando lobby para todos");
                
                try
                {
                    var currentLobbyId = GetCurrentLobbyId(steamManager);
                    if (currentLobbyId != 0)
                    {
                        CSteamID lobbyId = new CSteamID(currentLobbyId);
                        SteamMatchmaking.SetLobbyMemberLimit(lobbyId, 0);
                        SteamMatchmaking.SetLobbyJoinable(lobbyId, false);
                        
                        Debug.Log($"[LobbyCleanupHelper] Lobby {currentLobbyId} marcado como não-acessível");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[LobbyCleanupHelper] Erro ao fechar lobby: {e.Message}");
                }
            }
            else
            {
                Debug.Log("[LobbyCleanupHelper] Você é CLIENT - Saindo do lobby");
            }
            
            try
            {
                steamManager.LeaveLobby();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LobbyCleanupHelper] Erro ao sair do lobby: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[LobbyCleanupHelper] Sem conexão ativa");
        }
#else
        Debug.LogWarning("[LobbyCleanupHelper] Steamworks não está habilitado");
#endif
    }
    
#if STEAMWORKS_NET
    private static ulong GetCurrentLobbyId(NetworkSteamManager steamManager)
    {
        try
        {
            var lobbyIdField = typeof(NetworkSteamManager).GetField("currentLobbyID", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (lobbyIdField != null)
            {
                return (ulong)lobbyIdField.GetValue(steamManager);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LobbyCleanupHelper] Erro ao obter lobby ID: {e.Message}");
        }
        
        return 0;
    }
#endif
}
