using com.onlineobject.objectnet;
using UnityEngine;

public class PlayerSquad : Squad
{
    public void LoadPieces(TableData table)
    {
        for (int i = 1; i < table.Count(); i++)
        {
            string pieceName = table.GetRecord("Piece", i);
            Piece piece = GetPieceByName(pieceName);

            if (piece == null)
            {
                Debug.LogWarning($"No default piece found with the name {pieceName}");
            }

            int house = int.Parse(table.GetRecord("House", i));
            InstantiatePiece(piece, house, table == this.table);
        }
    }

    public override void LoadPieces()
    {
        LoadPieces(table);
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

    private async void InstantiatePiece(Piece piece, int field, bool isMy = true)
    {
        GameField gameField = GetGameField(field, !isMy);
        bool isOnline = MatchController.connection != NetworkConnectionType.Manual;

        GameObject obj = piece.gameObject;
        Vector3 pos = gameField.transform.position;
        Quaternion rot = isMy? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        Piece toLink;
        if (isOnline)
        {
            GameObject netObj = await NetworkGameObject.Instantiate(obj, pos, rot);
            toLink = netObj.GetComponent<Piece>();
            if (!isMy) toLink.SetControlToClient();
        }
        else
        {
            Instantiate(piece, pos, rot);
        }
    }
}