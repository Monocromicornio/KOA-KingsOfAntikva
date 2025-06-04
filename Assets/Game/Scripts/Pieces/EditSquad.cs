using System.Collections.Generic;
using UnityEngine;

public class EditSquad : Squad
{
    private List<EditableField> toChange = new List<EditableField>();

    [SerializeField]
    Piece[] defaultPieces;

    [Header("Sources")]
    [SerializeField]
    AudioSource selectSource;
    [SerializeField]
    AudioSource confirmSource;

    private void Start()
    {
        LoadPieces();
    }

    public override void LoadPieces()
    {
        if (table.Count() > 1)
        {
            for (int i = 1; i < table.Count(); i++)
            {
                string record = table.GetRecord("Piece", i);
                Piece piece = GetPieceByName(record, defaultPieces);
                if (piece == null) continue;

                int houseIndex = int.Parse(table.GetRecord("House", i));
                EditableField editable = editables[houseIndex];

                editable.SetPiece(piece.gameObject, () => ChangePiece(editable));
            }
        }
        else
        {
            for (int i = 0; i < editables.Length; i++)
            {
                if (i < defaultPieces.Length)
                {
                    GameObject piece = defaultPieces[i].gameObject;
                    EditableField editable = editables[i];

                    editable.SetPiece(piece, () => ChangePiece(editable));
                }
            }
        }
    }

    void ChangePiece(EditableField editable){
        if(toChange.Count == 0 && editable.piece == null){
            return;
        }

        if(toChange.Contains(editable)){
            toChange.Clear();
            ResetChange(new []{editable});
        }

        toChange.Add(editable);

        if(toChange.Count == 2){
            if(confirmSource) confirmSource.Play();

            EditableField from = toChange[0];
            GameObject fromPiece = from.piece;

            EditableField to = toChange[1];
            GameObject toPiece = to.piece;

            if(toPiece != null){
                from.SetPiece(toPiece);
                toPiece.transform.position = from.transform.position;
            }
            if(fromPiece != null){
                to.SetPiece(fromPiece);
                fromPiece.transform.position = to.transform.position;
            }

            ResetChange(new []{to, from});
        }
        else{
            if(selectSource) selectSource.Play();
        }
    }

    void ResetChange(EditableField [] deselects = null){
        toChange.Clear();

        if(deselects == null) return;
        foreach(EditableField toDeselect in deselects){
            toDeselect.Deselect();
        }
    }
}