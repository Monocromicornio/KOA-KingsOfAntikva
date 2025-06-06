using com.onlineobject.objectnet;
using UnityEngine;

public class PlayerSquad : Squad
{
    private bool fromStart = false;

    public void LoadPieces(bool fromStart)
    {
        this.fromStart = fromStart;
        LoadPieces();
    }

    public override void LoadPieces()
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
            InstantiatePiece(piece, GetGameField(house));
        }
    }

    private GameField GetGameField(int index)
    {
        if (!fromStart)
        {
            int lastIndex = gameFields.Length - 1;
            index = lastIndex - index;
        }
        return gameFields[index];
    }

    private async void InstantiatePiece(Piece piece, GameField gameField)
    {
        GameObject obj = piece.gameObject;
        Vector3 pos = gameField.transform.position;
        Quaternion rot = Quaternion.identity;

        if (!fromStart) rot = Quaternion.Euler(0, 180, 0);

        Piece toLink;
        if (MatchController.connection != NetworkConnectionType.Manual)
        {
            GameObject netObj = await NetworkGameObject.Instantiate(obj, pos, rot);
            toLink = netObj.GetComponent<Piece>();
        }
        else
        {
            toLink = Instantiate(piece, pos, rot);
        }

        LinkPieceToGameField(toLink, gameField);
    }

    public void TurnFakePiece(Piece piece)
    {
        if (pieces.Contains(piece))
        {
            pieces.Remove(piece);
        }

        piece.TurnFakePiece(pieceData.fakePiece);
    }
}