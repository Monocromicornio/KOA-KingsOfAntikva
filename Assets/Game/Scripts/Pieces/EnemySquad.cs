using System.Collections.Generic;
using UnityEngine;

public class EnemySquad : Squad
{
    protected override void LoadPieces()
    {
        int pieceCount = defaultPieces.Length;
        int lastIndex = gameFields.Length - 1;
        int firstIndex = lastIndex - pieceCount + 1;

        if (firstIndex < 0) firstIndex = 0;

        // Create a list with the target field indices
        List<int> fieldIndexes = new List<int>();
        for (int i = firstIndex; i <= lastIndex; i++)
            fieldIndexes.Add(i);

        // Shuffle the pieces
        List<Piece> pieces = new List<Piece>(defaultPieces);
        for (int i = 0; i < pieces.Count; i++)
        {
            int rnd = Random.Range(i, pieces.Count);
            Piece temp = pieces[i];
            pieces[i] = pieces[rnd];
            pieces[rnd] = temp;
        }

        // Position each piece in a random field within the range
        for (int i = 0; i < pieces.Count && fieldIndexes.Count > 0; i++)
        {
            int rndField = Random.Range(0, fieldIndexes.Count);
            int fieldIndex = fieldIndexes[rndField];
            fieldIndexes.RemoveAt(rndField);

            gameFields[fieldIndex].SetPiece(pieces[i]);
            pieces[i].GoToField(gameFields[fieldIndex]);
        }
    }
}