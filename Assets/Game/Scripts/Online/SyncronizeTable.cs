using System;
using System.Collections;
using System.Text;
using com.onlineobject.objectnet;
using UnityEngine;
using Steamworks;

public class SyncronizeTable : NetworkBehaviour
{
    public static SyncronizeTable instance;
    NetworkManager networkManager => NetworkManager.Instance();
    MatchController matchController => MatchController.instance;
    public TableData table;
    
    private static TableData cachedTableReference;

    private static byte[][] tableParts;

    private static TableData pendingTableData;

    public static TableData PendingTableData => pendingTableData;

    public static void ClearPendingTable() { pendingTableData = null; }

    public static ulong LocalSteamId { get; private set; }
    public static ulong OpponentSteamId { get; private set; }
    
    public static event Action<ulong> OnOpponentSteamIdReceived;

    void Awake()
    {
        instance = this;
        LocalSteamId = SteamUser.GetSteamID().m_SteamID;
        OpponentSteamId = 0;
        
        if (table != null && cachedTableReference == null)
        {
            cachedTableReference = table;
            Debug.Log($"[SyncronizeTable] Referência de TableData cacheada: {table.name}");
        }
        
        if (table == null && cachedTableReference != null)
        {
            table = cachedTableReference;
            Debug.Log($"[SyncronizeTable] TableData restaurado do cache: {table.name}");
        }
        
        Debug.Log($"[SyncronizeTable] Steam ID local salvo: {LocalSteamId}");
        Debug.Log($"[SyncronizeTable] TableData status: {(table != null ? $"OK ({table.name})" : "NULL!")}");
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            Debug.Log("[SyncronizeTable] Instância destruída");
            instance = null;
        }
    }

    void Start()
    {
        StartCoroutine(SendSteamIdDelayed());

        if (networkManager.IsServerConnection()) return;

        StartCoroutine(SendPartsToServer());
    }
    
    private IEnumerator SendSteamIdDelayed()
    {
        yield return new WaitForSeconds(2f);
        
        Debug.Log($"[SyncronizeTable] Enviando meu Steam ID para oponente: {LocalSteamId}");
        NetworkExecute<ulong>(ReceiveOpponentSteamId, LocalSteamId);
    }
    
    private void ReceiveOpponentSteamId(ulong opponentId)
    {
        if (opponentId == LocalSteamId)
        {
            Debug.Log("[SyncronizeTable] Recebi meu próprio Steam ID (ignorando)");
            return;
        }
        
        if (OpponentSteamId != 0)
        {
            Debug.Log($"[SyncronizeTable] Steam ID do oponente já foi definido: {OpponentSteamId}");
            return;
        }
        
        OpponentSteamId = opponentId;
        Debug.Log($"[SyncronizeTable] ✓ Steam ID do oponente recebido: {OpponentSteamId}");
        
        OnOpponentSteamIdReceived?.Invoke(OpponentSteamId);
    }

    IEnumerator SendPartsToServer()
    {
        if (table == null)
        {
            Debug.LogError("[SyncronizeTable] ERRO: Não é possível enviar tabela - table é null!");
            yield break;
        }
        
        Debug.Log($"[SyncronizeTable] Iniciando envio da tabela para servidor: {table.name}");
        
        string encondeTable = EncodeTableDataXml();
        byte[] bytesToEncode = Encoding.UTF8.GetBytes(encondeTable);

        Debug.Log($"[SyncronizeTable] Tabela codificada: {bytesToEncode.Length} bytes");

        var parts = SplitBytes(bytesToEncode, 5);
        for (int i = 0; i < parts.Length; i++)
        {
            Debug.Log($"[SyncronizeTable] Enviando parte {i + 1}/{parts.Length}");
            NetworkExecuteOnServer<byte[], int, int>(GetTable, parts[i], i, parts.Length);
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("[SyncronizeTable] Todas as partes da tabela foram enviadas");
    }

    public void SetChangeTurn()
    {
        NetworkExecute(ChangeTurn);
    }

    private void ChangeTurn()
    {
        matchController.ChangeTurnImmediate();
    }

    private void GetTable(byte[] encondeTable, int part, int size)
    {
        if (tableParts == null || tableParts.Length != size)
        {
            tableParts = new byte[size][];
            Debug.Log($"[SyncronizeTable] Servidor: Iniciando recepção de tabela ({size} partes)");
        }

        tableParts[part] = encondeTable;
        Debug.Log($"[SyncronizeTable] Servidor: Parte {part + 1}/{size} recebida ({encondeTable.Length} bytes)");

        foreach (var p in tableParts) if (p == null) return;

        Debug.Log("[SyncronizeTable] Servidor: Todas as partes recebidas, combinando...");
        byte[] fullTableBytes = CombineBytes(tableParts);
        Debug.Log($"[SyncronizeTable] Servidor: Tabela completa recebida ({fullTableBytes.Length} bytes total)");
        DecodeTable(Encoding.UTF8.GetString(fullTableBytes));
    }

    private void DecodeTable(string xmlString)
    {
        TableData tableData = DecodeTableDataXml(xmlString);

        if (tableData == null)
        {
            Debug.LogError("[SyncronizeTable] Falha ao decodificar tabela - tableData é null");
            return;
        }

        tableData.LoadTable();

        if (matchController != null)
        {
            matchController.StartGame(tableData);
        }
        else
        {
            Debug.Log("[SyncronizeTable] MatchController não pronto, delegando ao NetworkAutoLoadController...");
            pendingTableData = tableData;
            NetworkAutoLoadController.ScheduleStartGame();
        }
    }

    /// <summary>
    /// Divide um array de bytes em partes aproximadamente iguais.
    /// </summary>
    private byte[][] SplitBytes(byte[] bytes, int numParts)
    {
        int total = bytes.Length;
        int partSize = total / numParts;
        int remainder = total % numParts;
        byte[][] parts = new byte[numParts][];

        int offset = 0;
        for (int i = 0; i < numParts; i++)
        {
            int currentPartSize = partSize + (i < remainder ? 1 : 0);
            parts[i] = new byte[currentPartSize];
            Array.Copy(bytes, offset, parts[i], 0, currentPartSize);
            offset += currentPartSize;
        }
        return parts;
    }

    /// <summary>
    /// Combina uma lista de arrays de bytes em um único array.
    /// </summary>
    private byte[] CombineBytes(byte[][] parts)
    {
        int totalLength = 0;
        foreach (var partBytes in parts)
            totalLength += partBytes.Length;

        byte[] fullTableBytes = new byte[totalLength];
        int currentOffset = 0;
        foreach (var partBytes in parts)
        {
            Buffer.BlockCopy(partBytes, 0, fullTableBytes, currentOffset, partBytes.Length);
            currentOffset += partBytes.Length;
        }
        return fullTableBytes;
    }

    /// <summary>
    /// Retorna o XML atual do TableData como string.
    /// </summary>
    public string EncodeTableDataXml()
    {
        if (table == null)
        {
            Debug.LogError("[SyncronizeTable] ERRO: Não é possível codificar - table é null!");
            return string.Empty;
        }
        
        try
        {
            table.LoadTable();
            string xml = table.GetXmlString();
            
            if (string.IsNullOrEmpty(xml))
            {
                Debug.LogError("[SyncronizeTable] ERRO: XML gerado está vazio!");
                return string.Empty;
            }
            
            Debug.Log($"[SyncronizeTable] XML codificado com sucesso ({xml.Length} caracteres)");
            return xml;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SyncronizeTable] ERRO ao codificar XML: {e.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Cria e retorna um novo TableData a partir do XML fornecido, sem alterar o TableData atual.
    /// </summary>
    public TableData DecodeTableDataXml(string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            Debug.LogError("[SyncronizeTable] XML string está vazio");
            return null;
        }

        if (table == null)
        {
            Debug.LogError("[SyncronizeTable] TableData de referência (table) está null! Verifique se foi atribuído no Inspector.");
            return null;
        }

        TableData newTable = ScriptableObject.CreateInstance<TableData>();

        newTable.name = "ClientTable";
        newTable.tableName = "ClientTable";
        newTable.rootName = table.rootName;

        try
        {
            newTable.LoadFromXmlString(xml);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SyncronizeTable] Erro ao carregar XML: {e.Message}");
            return null;
        }

        return newTable;
    }
    
    public static void ResetOpponentSteamId()
    {
        OpponentSteamId = 0;
        Debug.Log("[SyncronizeTable] Steam ID do oponente resetado");
    }
    
    public static void ResetAll()
    {
        OpponentSteamId = 0;
        instance = null;
        tableParts = null;
        pendingTableData = null;
        
        if (OnOpponentSteamIdReceived != null)
        {
            foreach (var d in OnOpponentSteamIdReceived.GetInvocationList())
            {
                OnOpponentSteamIdReceived -= (Action<ulong>)d;
            }
        }
        
        Debug.Log("[SyncronizeTable] Estado completo resetado");
    }
}
