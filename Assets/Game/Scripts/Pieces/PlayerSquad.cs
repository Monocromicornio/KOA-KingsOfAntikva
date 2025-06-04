using com.onlineobject.objectnet;
using UnityEngine;

public class PlayerSquad : Squad
{
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
            InstantiatePiece(piece, gameFields[house]);
        }
    }

    private async void InstantiatePiece(Piece piece, GameField gameField)
    {
        GameObject obj = piece.gameObject;
        Vector3 pos = gameField.transform.position;
        Quaternion rot = Quaternion.identity;

        Piece toLink;
        if (MatchController.online)
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
}