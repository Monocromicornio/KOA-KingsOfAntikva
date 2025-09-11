using System.Collections.Generic;
using UnityEngine;

public class EnemySquad : Squad
{
    private List<FakePiece> _fakePieces;
    public List<FakePiece> fakePieces
    {
        get
        {
            _fakePieces ??= new List<FakePiece>();
            return _fakePieces;
        }
    }

    [SerializeField]
    Piece[] defaultPieces;

    public override void LoadPieces()
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
        for (int i = 1; i < pieces.Count; i++)
        {
            int rnd = Random.Range(i, pieces.Count);
            Piece temp = pieces[i];
            pieces[i] = pieces[rnd];
            pieces[rnd] = temp;
        }

        // Position each piece in a random field within the range
        for (int i = 0; i < pieces.Count && fieldIndexes.Count > 0; i++)
        {
            //Select a random field
            int rndField;
            if (i == 0) rndField = 6;
            else rndField = Random.Range(0, fieldIndexes.Count);
            int fieldIndex = fieldIndexes[rndField];

            //Remove from list
            fieldIndexes.RemoveAt(rndField);

            //Get a piece
            Vector3 pos = gameFields[fieldIndex].transform.position;
            Quaternion rot = Quaternion.Euler(0, 180, 0);
            Piece piece = Instantiate(pieces[i], pos, rot);

            //Set as fake
            piece.TurnRedPiece();
        }
    }
}