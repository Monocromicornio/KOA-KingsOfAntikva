using System.Collections.Generic;
using UnityEngine;

public class EditOrderParts : OrdersParts {

    [SerializeField]
    private GameObject[] defaultPieces;
    private HousePicker[] housePickers;

    private List<HousePicker> toChange = new List<HousePicker>();

    [Header("Sources")]
    [SerializeField]
    AudioSource selectSource;
    [SerializeField]
    AudioSource confirmSource;

    void Start()
    {
        housePickers = board.GetHousePickersFromFields();
        Order();
    }

    private void Order()
    {
        if (table.Count() > 1)
        {
            for (int i = 1; i < table.Count(); i++)
            {
                string record = table.GetRecord("Piece", i);
                int house = int.Parse(table.GetRecord("House", i));

                GameObject piece = GetPieceByRecord(record);
                if(piece == null) continue;

                SetHousePickerBusy(housePickers[house], piece);
            }
        }
        else
        {
            for (int i = 0; i < housePickers.Length; i++)
            {
                if (i < defaultPieces.Length)
                {         
                    GameObject piece = defaultPieces[i];
                    SetHousePickerBusy(housePickers[i], piece);
                }
            }
        }
    }

    GameObject GetPieceByRecord(string record){
        GameObject piece = System.Array.Find(
            defaultPieces,
            p => p.name == record
        );
        
        if (piece == null)
        {
            Debug.LogWarning($"No default piece found with the name {record}");
        }

        return piece;
    }

    void SetHousePickerBusy(HousePicker housePicker, GameObject piece, bool resetRotate = false){
        housePicker.SetPiece(piece.gameObject, () => ChangePiece(housePicker));
        piece.transform.position = housePicker.transform.position;
        if(resetRotate) piece.transform.Rotate(0, 0, 0, Space.Self);
    }

    void ChangePiece(HousePicker housePicker){
        if(toChange.Count == 0 && housePicker.piece == null){
            return;
        }

        if(toChange.Contains(housePicker)){
            toChange.Clear();
            ResetChange(new []{housePicker});
        }

        toChange.Add(housePicker);

        if(toChange.Count == 2){
            if(confirmSource) confirmSource.Play();

            HousePicker from = toChange[0];
            GameObject fromPiece = from.piece;

            HousePicker to = toChange[1];
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

    void ResetChange(HousePicker [] deselects = null){
        toChange.Clear();

        if(deselects == null) return;
        foreach(HousePicker toDeselect in deselects){
            toDeselect.Deselect();
        }
    }
}