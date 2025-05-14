using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdersParts : MonoBehaviour
{
    public TableData table;
    protected BoardController board {
        get {
            return BoardController.instance;
        }
    }
    public Field[] fields {
        get {
            return board.fields.ToArray();
        }
    }

    public bool isPlayerBoard = true;

    void Start()
    {
        //StartCoroutine(Order(0.5f));
        if (isPlayerBoard)
        {
            StartCoroutine(StartLoadPieces());
        }
        else
        {
            StartCoroutine(StartLoadEnemyPieces());
        }
        
    }

    int iPiecesRecord = 0;

    private IEnumerator StartLoadPieces()
    {
        while (iPiecesRecord==0)
        {
            if (table.Loaded())
            {
                iPiecesRecord = table.Count();
                LoadPieces(iPiecesRecord);                
                Debug.Log("table Count = " + table.Count());

            }
            yield return null;
        }
    }

    string[,] aOrderEnemies;

    private IEnumerator StartLoadEnemyPieces()
    {
        aOrderEnemies = OrderEnemy();

        while (iPiecesRecord == 0)        
        {            
            iPiecesRecord = aOrderEnemies.Length;            
            LoadPieces((iPiecesRecord/2)); ;
            Debug.Log("iPiecesRecord = " + iPiecesRecord);
            
            yield return null;
        }
    }

    private void LoadPieces(int length)
    {
        if (isPlayerBoard)
        {
            for (int i = 1; i < length; i++)
            {
                Debug.Log("Name piece = " + table.GetRecord("Piece", i) + " - House = " + table.GetRecord("House", i));

                GameObject piece = transform.Find(table.GetRecord("Piece", i).ToString()).gameObject;
                int house = int.Parse(table.GetRecord("House", i));

                fields[house].GetComponent<FieldController>().Busy = true;
                fields[house].GetComponent<FieldController>().BusyPiece = piece;
                fields[house].GetComponent<FieldController>().SetTextForce(piece.GetComponent<Player>().GetForceType());
                piece.GetComponent<Player>().iFieldLive = fields[house].GetComponent<FieldController>().index;
                piece.GetComponent<Player>().SetTargetField(fields[house].transform);
                piece.transform.Rotate(0, 0, 0, Space.Self);
            }
        }
        else
        {            
            for (int i = 0; i < length; i++)
            {

                Debug.Log("LoadPieces aOrderEnemies piece = " + aOrderEnemies[i, 0]);
                GameObject piece = transform.Find(aOrderEnemies[i, 0].ToString()).gameObject;

                Debug.Log("LoadPieces aOrderEnemies field = " + aOrderEnemies[i, 1]);
                int house = int.Parse(aOrderEnemies[i, 1].ToString());

                fields[house].GetComponent<FieldController>().Busy = true;
                fields[house].GetComponent<FieldController>().BusyPiece = piece;
                fields[house].GetComponent<FieldController>().SetTextForce(piece.GetComponent<Player>().GetForceType());
                piece.GetComponent<Player>().iFieldLive = fields[house].GetComponent<FieldController>().index;
                piece.GetComponent<Player>().SetTargetField(fields[house].transform);
                piece.transform.Rotate(0, 180, 0, Space.Self);
                
            }
        }
    }

    private string[,] OrderEnemy()    
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Enemy");

        string[,] orderpieaces = new string[pieces.Length, 2];

        int PiecesCount = pieces.Length;
        int FieldsCount = fields.Length-1;

        List<string> lpieces = new List<string>();

        foreach (GameObject piece in pieces)
        {
            lpieces.Add(piece.name);
            Debug.Log("List lpieces = " + piece);
        }

        Debug.Log("PiecesCount = " + PiecesCount);
        Debug.Log("FieldsCount = " + FieldsCount);

        for (int i = 0; i < PiecesCount; i++)
        {
            int index = Random.Range(0, lpieces.Count);
            orderpieaces[i, 0] = lpieces[index];
            orderpieaces[i, 1] = fields[FieldsCount].GetComponent<FieldController>().index.ToString();
            lpieces.Remove(lpieces[index]);
            FieldsCount--;
            Debug.Log("OrderEnemy piece = " + orderpieaces[i, 0]);
            Debug.Log("OrderEnemy house = " + orderpieaces[i, 1]);
        }
        

        return orderpieaces;
    }

    /*private IEnumerator Order(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Fields = GameObject.FindGameObjectsWithTag("Field");

        int count = 0;

        if (bPlayer)
        {
            foreach (GameObject Field in Fields)
            {
                Debug.Log(Field.name);

                if (count < (Pieces.Length))
                {
                    Field.GetComponent<FieldController>().Busy = true;
                    Field.GetComponent<FieldController>().BusyPiece = Pieces[count].gameObject;
                    Field.GetComponent<FieldController>().SetTextForce(Pieces[count].GetComponent<Player>().Force.ToString());
                    Pieces[count].GetComponent<Player>().iFieldLive = Field.GetComponent<FieldController>().index;
                    Pieces[count].GetComponent<Player>().SetTargetField(Field.transform);
                    Pieces[count].transform.Rotate(0, 0, 0, Space.Self);
                    count++;
                }
            }
        }
        else
        {
            count = 0;

            GameObject[] fcs = FindObjectOfType<OrdersParts>().Fields;

            int ifields = fcs.Length-1;

            for (int i = ifields; i > 0; i--)
            {                

                if (count < (Pieces.Length))
                {
                    fcs[i].GetComponent<FieldController>().Busy = true;
                    fcs[i].GetComponent<FieldController>().BusyPiece = Pieces[count].gameObject;
                    Pieces[count].GetComponent<Player>().iFieldLive = fcs[i].GetComponent<FieldController>().index;
                    Pieces[count].GetComponent<Player>().SetTargetField(fcs[i].transform);
                    Pieces[count].transform.Rotate(0, 180, 0, Space.Self);                                        
                    count++;
                }
            }
        }


        //Fields[0].GetComponent<FieldController>().Busy = true;
        //Fields[0].GetComponent<FieldController>().BusyPiece = Pieces[0].gameObject;
        //Pieces[0].GetComponent<Player>().iFieldLive = Fields[0].GetComponent<FieldController>().Index;

        print("OrdersParts - WaitAndPrint " + Time.time);            

    }*/
}
