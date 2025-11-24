using UnityEngine;
using Steamworks;
using System.Collections;

public class OpponentProfileLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerProfileDisplay enemyProfileDisplay;
    
    [SerializeField]
    private float maxSearchTime = 30f;
    
    private bool foundOpponent = false;
    private float searchTime = 0f;
    
    private void Start()
    {
        Debug.Log("[OpponentProfileLoader] Iniciando sistema de perfil do oponente");
        
        SyncronizeTable.OnOpponentSteamIdReceived += OnOpponentSteamIdReceived;
        
        StartCoroutine(WaitForOpponentSteamId());
    }
    
    private void OnDestroy()
    {
        SyncronizeTable.OnOpponentSteamIdReceived -= OnOpponentSteamIdReceived;
    }
    
    private void OnOpponentSteamIdReceived(ulong opponentSteamId)
    {
        Debug.Log($"[OpponentProfileLoader] ✓ Evento recebido - Steam ID do oponente: {opponentSteamId}");
        
        if (opponentSteamId != 0 && opponentSteamId != SteamUser.GetSteamID().m_SteamID)
        {
            foundOpponent = true;
            SetOpponentProfile(opponentSteamId);
        }
    }
    
    private IEnumerator WaitForOpponentSteamId()
    {
        searchTime = 0f;
        
        while (!foundOpponent && searchTime < maxSearchTime)
        {
            yield return new WaitForSeconds(1f);
            searchTime += 1f;
            
            if (SyncronizeTable.OpponentSteamId != 0)
            {
                ulong opponentId = SyncronizeTable.OpponentSteamId;
                ulong localId = SteamUser.GetSteamID().m_SteamID;
                
                if (opponentId != localId)
                {
                    Debug.Log($"[OpponentProfileLoader] ✓ Steam ID do oponente encontrado: {opponentId}");
                    foundOpponent = true;
                    SetOpponentProfile(opponentId);
                    yield break;
                }
            }
            
            Debug.Log($"[OpponentProfileLoader] Aguardando Steam ID do oponente... ({searchTime:F0}s)");
        }
        
        if (!foundOpponent)
        {
            Debug.LogWarning($"[OpponentProfileLoader] Timeout: Oponente não encontrado após {maxSearchTime} segundos");
        }
    }
    
    private void SetOpponentProfile(ulong steamId)
    {
        if (enemyProfileDisplay != null)
        {
            Debug.Log($"[OpponentProfileLoader] Configurando perfil do oponente com Steam ID: {steamId}");
            enemyProfileDisplay.SetCustomSteamId(steamId);
            enemyProfileDisplay.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("[OpponentProfileLoader] EnemyProfileDisplay não está configurado!");
        }
    }
    
    public void ForceRefresh()
    {
        Debug.Log("[OpponentProfileLoader] ForceRefresh chamado");
        foundOpponent = false;
        searchTime = 0f;
        
        if (SyncronizeTable.OpponentSteamId != 0)
        {
            SetOpponentProfile(SyncronizeTable.OpponentSteamId);
        }
        else
        {
            StartCoroutine(WaitForOpponentSteamId());
        }
    }
}
