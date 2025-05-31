using UnityEngine;

public class PlayerSquad : Squad
{
    protected override void LoadPieces()
    {
        for (int i = 1; i < table.Count(); i++)
        {
            string pieceName = table.GetRecord("Piece", i);
            Piece piece = transform.Find(pieceName).GetComponent<Piece>();

            if (piece == null)
            {
                Debug.LogWarning($"No default piece found with the name {pieceName}");
            }

            int house = int.Parse(table.GetRecord("House", i));
            LinkPieceToGameField(piece, gameFields[house]);
        }
    }
}