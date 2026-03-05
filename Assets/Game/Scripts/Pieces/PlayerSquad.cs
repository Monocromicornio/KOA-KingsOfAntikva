using System.Collections.Generic;
using System.Threading.Tasks;
using com.onlineobject.objectnet;
using UnityEngine;

public class PlayerSquad : Squad
{
    private async Task LoadPiecesInternal(TableData table, bool isMy)
    {
        var tasks = new List<Task>();

        for (int i = 1; i < table.Count(); i++)
        {
            string pieceName = table.GetRecord("Piece", i);
            Piece piece = GetPieceByName(pieceName);

            if (piece == null)
            {
                Debug.LogWarning($"No default piece found with the name {pieceName}");
                continue;
            }

            int house = int.Parse(table.GetRecord("House", i));
            tasks.Add(InstantiatePiece(piece, house, isMy));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Loads the client's pieces from the given table. Returns a Task that completes when all pieces are instantiated.
    /// </summary>
    public Task LoadPieces(TableData table)
    {
        return LoadPiecesInternal(table, false);
    }

    /// <summary>
    /// Loads the host's (local player's) pieces. Returns a Task that completes when all pieces are instantiated.
    /// </summary>
    public override Task LoadPieces()
    {
        return LoadPiecesInternal(table, true);
    }

    private GameField GetGameField(int index, bool reverse = false)
    {
        if (reverse)
        {
            int lastIndex = gameFields.Length - 1;
            index = lastIndex - index;
        }
        return gameFields[index];
    }

    private async Task InstantiatePiece(Piece piece, int field, bool isMy = true)
    {
        GameField gameField = GetGameField(field, !isMy);
        bool isOnline = MatchController.instance != null && MatchController.instance.hasConnection;

        GameObject obj = piece.gameObject;
        Vector3 pos = gameField.transform.position;
        Quaternion rot = isMy ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        if (isOnline)
        {
            GameObject netObj = await NetworkGameObject.Instantiate(obj, pos, rot);
            Piece toLink = netObj.GetComponent<Piece>();
            toLink.SetAsMyPiece(isMy);
            if (!isMy) toLink.SetControlToClient();
        }
        else
        {
            Piece spawned = Instantiate(piece, pos, rot);
            spawned.SetAsMyPiece(isMy);
        }
    }
}