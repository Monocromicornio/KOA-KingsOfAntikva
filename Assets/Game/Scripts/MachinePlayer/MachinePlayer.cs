using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MachinePlayer : MonoBehaviour
{
    MatchController matchController => MatchController.instance;
    List<FakePiece> pieces => matchController.enemySquad.fakePieces;
    List<SelectablePiece> selectablePieces = new List<SelectablePiece>();

    Piece currentPiece;

    [SerializeField]
    bool played = false;

    private void Start()
    {
        GetSelectablePieces();
    }

    public void StartTurn()
    {
        GetSelectablePieces();
        var selectables = GetActiveSelectables();
        int index = Random.Range(0, selectables.Count);
        for (int i = 0; i < selectables.Count; i++)
        {
            Piece piece = selectables[i].piece;
            if (piece.type == PieceType.Soldier)
            {
                index = i;
                break;
            }
        }
        ActionPiece(selectables[index]);
    }

    private void GetSelectablePieces()
    {
        selectablePieces.Clear();
        foreach (FakePiece piece in pieces)
        {
            SelectablePiece selectableField = piece.GetComponent<SelectablePiece>();
            if (selectableField != null)
            {
                selectablePieces.Add(selectableField);
            }
        }
    }

    private List<SelectablePiece> GetActiveSelectables()
    {
        List<SelectablePiece> selectables = new List<SelectablePiece>();
        foreach (SelectablePiece selectable in selectablePieces)
        {
            GameField[][] fields = selectable.GetSelectablesFields();
            foreach (var field in fields)
            {
                if (field.Length > 0)
                {
                    selectables.Add(selectable);
                    break;
                }
            }
        }

        return selectables;
    }

    private void ActionPiece(SelectablePiece selectablePiece)
    {
        Piece piece = selectablePiece.piece;
        var selectedFields = selectablePiece.selectedFields;
        List<GameField> toSelect = new List<GameField>();

        foreach (List<GameField> fields in selectedFields.Values)
        {
            if (fields.Count > 0)
            {
                toSelect.Add(fields.Last());
            }
        }

        int index = Random.Range(0, toSelect.Count);
        piece.SelectedAField(toSelect[index]);
        print("AçÃo -> " + piece.name + "  --  " + toSelect[index].index);
    }

/// <summary>
    /// SelectField: verificar quais peças poder fazer alguma ação*
    /// Sortear entre essas peças para ver qual vai se mexer
    /// Fazer a ação
    /// </summary>
    /*private void SelectPartsFreeHouse()
    {
        List<MyPeaces> mypeaces = new List<MyPeaces>();
        int[] ipeaces = new int[pieces.Count];
        int icountmypeaces = 0;

        foreach (Piece player in pieces)
        {
            int[] iHousesFree = player.HousesFree();

            if (iHousesFree.Length > 0)
            {
                ipeaces[icountmypeaces] = player.indexCurrentField;

                foreach (int i in iHousesFree)
                {
                    if (i > 0)
                    {
                        mypeaces.Add(new MyPeaces(player.indexCurrentField, i));
                    }
                }

                icountmypeaces++;
            }

        }

        if (mypeaces.Count > 0)
        {
            ChoosePart(mypeaces);
        }
    }

    private void ChoosePart(List<MyPeaces> mypeaces)
    {
        int iPeacesCount = mypeaces.Count-1;
        int iRandomIndexPart = Random.Range(0, iPeacesCount);
        int iRandomindexPeace = mypeaces[iRandomIndexPart].indexPeace;      

        List<int> lpeacehouses = new List<int>();

        foreach (MyPeaces p in mypeaces)
        {
            if (p.indexPeace == iRandomindexPeace)
            {
                lpeacehouses.Add(p.indexHouse);
            }
        }

        int iCountHouses = lpeacehouses.Count;
        int iRandomHouse = Random.Range(0, iCountHouses);
        int iSelectHouse = lpeacehouses[iRandomHouse];

        SelectPart(iRandomindexPeace, iSelectHouse);
    }

    private void SelectPart(int iPart, int iHouse)
    {
        foreach (Piece player in pieces)
        {
            if (player.indexCurrentField == iPart)
            {
                currentPiece = player;
            }
        }


        if (currentPiece)
        {
            currentPiece.SelectPeace();
            IEnumerator enumerator = IESelectHouse(iPart,1.0f);
            StartCoroutine(enumerator);
        }
    }

    IEnumerator IESelectHouse(int HousePlayer, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        int[] ihousesfree = iHousesFree().ToArray();

        //HousePlayer++;

        int iHouseFreeCount = ihousesfree.Length;
        int iMaxRows = FindObjectOfType<BoardController>().ColumnLength() + 1;
        Debug.Log("SelectPart iHouseFreeCount =" + iHouseFreeCount);

        if (iHouseFreeCount == 0)
        {
            SelectPartsFreeHouse();
        }
        else
        {
            int iSelectHouse = 0;

            GameField[] gameFields = FindObjectsOfType<GameField>();

            foreach (int ihouses in ihousesfree)
            {
                int Index = IndexHouse(ihouses);

                Debug.Log("SelcetPart - fcs[" + Index + "].Busy = " + gameFields[Index].hasPiece);

                if (iSelectHouse == 0)
                {
                    if (gameFields[Index].hasPiece)
                    {
                        Debug.Log("SelcetPart - fcs[" + Index + "].BusyPiece.tag = " + gameFields[Index].piece.tag);
                        if (gameFields[Index].piece.tag == "Player")
                        {
                            iSelectHouse = ihouses;
                        }
                    }
                    else if (ihouses == (HousePlayer - iMaxRows))
                    {
                        iSelectHouse = ihouses;
                    }
                }
            }

            int iRandomHouse = Random.Range(0, iHouseFreeCount);
            Debug.Log("SelectPart iRandomHouse =" + iRandomHouse);

            if (iSelectHouse == 0)
            {
                iSelectHouse = ihousesfree[iRandomHouse];
            }
            Debug.Log("SelectPart SelectHouse =" + iSelectHouse);            
            Debug.Log("SelectPart HousePlayer = " + HousePlayer);
            Debug.Log("SelectPart (HousePlayer- iMaxRows) = " + (HousePlayer + iMaxRows));            

            if (iHousesFree().Count > 1)
            {
                if (iSelectHouse == (HousePlayer + iMaxRows))
                {
                    Debug.Log("SelectPart iHousesFree().Count = " + iHousesFree().Count);

                    Debug.Log("SelectPart ihousesfree = " + ihousesfree.Length);
                    ihousesfree = null;
                    ihousesfree = iHousesFree(iSelectHouse).ToArray();
                    Debug.Log("SelectPart ihousesfree after remove = " + ihousesfree.Length);

                    iHouseFreeCount = ihousesfree.Length;
                    iRandomHouse = Random.Range(0, iHouseFreeCount);
                    iSelectHouse = ihousesfree[iRandomHouse];

                    Debug.Log("SelectPart New iSelectHouse = " + iSelectHouse);
                }
            }

            IEnumerator enumerator = SelectHouse(iSelectHouse, 0.5f);
            StartCoroutine(enumerator);
        }
    }

    private void SelectPeace()
    {
        //Lista das peças do computador
        List<MyPeaces> mypeaces = new List<MyPeaces>();

        //Variavel que guarda a casa selecionada
        int iHouseSelect = 0;        

        //array para as peças com casas livres
        int[] ipeaces = new int[pieces.Count];

        //variavel para contar o loop e usar de indice
        int icountmypeaces = 0;

        //Separa as peças que tem casas livres e não seja bomba, bandeira e soldado
        foreach (Piece player in pieces)
        {
            //Debug.Log("MachinePlayer - player name = " + player.name);
            //Debug.Log("MachinePlayer - player iFieldLive = " + player.iFieldLive);
            //Debug.Log("MachinePlayer - player.HousesFree().Length = " + player.HousesFree().Length);

            //array para colocar as casas livres
            int[] iHousesFree = player.HousesFree();

            if (iHousesFree.Length > 0)
            {
                if (player.type != PieceType.Soldier)
                {
                    if (player.type != PieceType.Flag)
                    {
                        if (player.type != PieceType.Bomb)
                        {
                            Debug.Log("MachinePlayer player iFieldLive = " + player.indexCurrentField);

                            ipeaces[icountmypeaces] = player.indexCurrentField;

                            foreach (int i in iHousesFree)
                            {
                                Debug.Log("MachinePlayer player iFieldLive = " + player.indexCurrentField + " - iHousesFree = " + i);

                                iHouseSelect = i;

                                if (iHouseSelect > 0)
                                {
                                    mypeaces.Add(new MyPeaces() { indexPeace = player.indexCurrentField, indexHouse = i });
                                }
                            }

                            icountmypeaces++;

                        }
                    }
                }
            }

            
        }

        Debug.Log("mypeaces.Count = " + mypeaces.Count);

        int iRandomPeace = ipeaces[Random.Range(0, (ipeaces.Length - 1))];

        Debug.Log("iRandomPeace = " + iRandomPeace);

        List<int> lpeacehouses = new List<int>();

        foreach (MyPeaces p in mypeaces)
        {
            Debug.Log("indexPeace = " + p.indexPeace + " - indexHouse = " + p.indexHouse);

            if(p.indexPeace == iRandomPeace)
            {
                lpeacehouses.Add(p.indexHouse);
            }
        }

        Debug.Log("lpeacehouses Count = " + lpeacehouses.Count);

        int[] ihouses = lpeacehouses.ToArray();

        Debug.Log("ihouses Length = " + ihouses.Length);

        int iRandomHouse = Random.Range(0, (ihouses.Length - 1));

        Debug.Log("iRandomHouse = " + iRandomHouse);
        Debug.Log("ihouses[iRandomHouse] = " + ihouses[iRandomHouse]);

        mypeaces.Clear();

        foreach (Piece player in pieces)
        {
            if (player.indexCurrentField == ipeaces[iRandomHouse])
            {
                PlayerSelect = player;
            }
        }

        if (PlayerSelect)
        {
            Debug.Log("MachinePlayer PlayerSelect =" + PlayerSelect.name + " - iFieldLive = " + PlayerSelect.indexCurrentField);
            PlayerSelect.SelectPeace();
            IEnumerator enumerator = SelectHouse(ihouses[iRandomHouse], 1.0f);
            StartCoroutine(enumerator);
        }
    }

    private IEnumerator SelectHouse(int house, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        HouseClick(house);
    }

    void HouseClick(int house)
    {        
        GameField[] gameFields = FindObjectsOfType<GameField>();

        foreach (GameField gameField in gameFields)
        {
            if (gameField.select == true)
            {
                if (gameField.index == house)
                {
                    if (!gameField.hasPiece)
                    {
                        gameField.Selection();
                        break;
                    }
                    else
                    {
                        gameField.piece.GetComponent<Piece>().SelectPeace();
                        break;
                    }
                }
            }            
        }        
    }

    private List<int> iHousesFree()
    {        

        GameField[] gameFields = FindObjectsOfType<GameField>();

        List<int> lfreehouses = new List<int>();

        foreach (GameField gameField in gameFields)
        {
            if (gameField.select == true)
            {
                lfreehouses.Add(gameField.index);
            }
        }

        return lfreehouses;
    }

    private List<int> iHousesFree(int iRemoveHouse)
    {

        GameField[] gameFields = FindObjectsOfType<GameField>();

        List<int> lfreehouses = new List<int>();

        foreach (GameField gameField in gameFields)
        {
            if (gameField.select == true)
            {
                lfreehouses.Add(gameField.index);
            }
        }

        lfreehouses.Remove(iRemoveHouse);

        return lfreehouses;
    }

    private int IndexHouse(int index)
    {
        int indexfield = 0;
        GameField[] gameFields = FindObjectsOfType<GameField>();

        int indexloop = 0;

        foreach (GameField gameField in gameFields)
        {
            if (gameField.index == index)
            {
                indexfield = indexloop;
            }

            indexloop++;
        }

        return indexfield;
    }


    public class MyPeaces
    {
        public MyPeaces() { }

        public int indexPeace { get; set; }
        public int indexHouse { get; set; }


        public MyPeaces(int indexPeace, int indexHouse)
        {
            this.indexPeace = indexPeace;
            this.indexHouse = indexHouse;
        }
    }*/
}
