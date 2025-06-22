using System;
using System.Text;
using com.onlineobject.objectnet;
using UnityEngine;

public class SyncronizeTable : NetworkBehaviour
{
    public static SyncronizeTable instance;
    NetworkManager networkManager => NetworkManager.Instance();
    MatchController matchController => MatchController.instance;
    public TableData table;

    private byte[][] tableParts;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (networkManager.IsServerConnection()) return;
        string encondeTable = EncodeTableDataXml();
        byte[] bytesToEncode = Encoding.UTF8.GetBytes(encondeTable);

        var parts = SplitBytes(bytesToEncode, 5);
        for (int i = 0; i < parts.Length; i++)
            NetworkExecuteOnServer<byte[], int, int>(GetTable, parts[i], i, parts.Length);
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
            tableParts = new byte[size][];

        tableParts[part] = encondeTable;

        foreach (var p in tableParts) if (p == null) return;

        byte[] fullTableBytes = CombineBytes(tableParts);
        DecodeTable(Encoding.UTF8.GetString(fullTableBytes));
    }

    private void DecodeTable(string xmlString)
    {
        TableData tableData = DecodeTableDataXml(xmlString);
        tableData.LoadTable();
        matchController.StartGame(tableData);
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
        // Garante que o xmlDoc está atualizado
        table.LoadTable();
        return table != null ? table.GetXmlString() : string.Empty;
    }

    /// <summary>
    /// Cria e retorna um novo TableData a partir do XML fornecido, sem alterar o TableData atual.
    /// </summary>
    public TableData DecodeTableDataXml(string xml)
    {
        if (string.IsNullOrEmpty(xml)) return null;

        // Cria uma nova instância de TableData
        TableData newTable = ScriptableObject.CreateInstance<TableData>();

        newTable.name = "ClientTable";
        newTable.tableName = "ClientTable";
        newTable.rootName = table.rootName;

        // Carrega o XML na nova instância
        newTable.LoadFromXmlString(xml);

        return newTable;
    }
}
