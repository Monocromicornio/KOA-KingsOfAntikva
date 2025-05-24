using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachinePlayer : MonoBehaviour
{
    MatchController turn;
    List<Piece> lplayers;

    [SerializeField]
    bool played = false;

    void Start()
    {
        turn = FindObjectOfType<MatchController>();

        lplayers = new List<Piece>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!turn.isBlueTurn)
        {           
            //if (turn.Liberate == true)
            {
                if (!played)
                {
                    //StartCoroutine(ResetPlay(3.5f));
                    played = true;
                    StartCoroutine(GetListParts(0.7f));
                    Debug.Log("GetListPeaces()");                    
                    Debug.Log("bPlayed() = " + played);
                }
            }
        }

        if (turn.isBlueTurn)
        {
            played = false;
        }

     }

    private IEnumerator GetListParts(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (lplayers.Count > 0)
        {
            lplayers.Clear();
        }               

        Piece[] players = FindObjectsOfType<Piece>();

        Debug.Log("players count = " + players.Length);
        
        //Colocar todas as peças inimigas na lista lplayers
        foreach(Piece player in players)
        {
            if(player.tag == "Enemy")
            {
                if (player.type != PieceType.Bomb)
                {
                    if (player.type != PieceType.Flag)
                    {
                        lplayers.Add(player);
                    }
                }

                //Debug.Log("GetListPeaces - player = " + player.name);
            }
        }

        //Debug.Log("lplayers count = " + lplayers.Count);
        
        SelectPartsFreeHouse();
        //SelectPeace();


    }

    private IEnumerator ResetPlay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("ResetPlay - bPlayed = " + played);
        if (!turn.isBlueTurn)
        {
            //if (turn.Liberate == true)
            {
                if (played)
                {
                    played = false;
                }
            }
        }
    }

    Piece PlayerSelect;

    private void SelectPartsFreeHouse()
    {
        return;
        /*List<MyPeaces> mypeaces = new List<MyPeaces>();

        //Limpar lista
        mypeaces.Clear();

        //Variavel que guarda a casa selecionada
        int iHouseSelect = 0;

        //array para as peças com casas livres
        int[] ipeaces = new int[lplayers.Count];

        //variavel para contar o loop e usar de indice
        int icountmypeaces = 0;

        //Separa as peças que tem casas livres e não seja bomba, bandeira e soldado
        foreach (Piece player in lplayers)
        {
            int[] iHousesFree = player.HousesFree();

            if (iHousesFree.Length > 0)
            {
                if (player.type != PieceType.Bomb)
                {
                    if (player.type != PieceType.Flag)
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

        if (mypeaces.Count > 0)
        {

            foreach (MyPeaces mypeace in mypeaces)
            {
                Debug.Log("SelectPartsFreeHouse mypeace indexPeace = " + mypeace.indexPeace);
                Debug.Log("SelectPartsFreeHouse mypeace indexHouse = " + mypeace.indexHouse);
            }

            ChoosePart(mypeaces);
        }

*/
    }

    private void ChoosePart(List<MyPeaces> mypeaces)
    {
        int iPeacesCount = mypeaces.Count-1;

        Debug.Log("ChoosePart - iPeacesCount = " + iPeacesCount);

        int iRandomIndexPart = Random.Range(0, iPeacesCount);

        Debug.Log("ChoosePart - iRandomIndexPart = " + iRandomIndexPart);

        int iRandomindexPeace = mypeaces[iRandomIndexPart].indexPeace;

        Debug.Log("ChoosePart - iRandomindexPeace = " + iRandomindexPeace);        

        List<int> lpeacehouses = new List<int>();

        foreach (MyPeaces p in mypeaces)
        {            

            if (p.indexPeace == iRandomindexPeace)
            {
                Debug.Log("ChoosePart - indexPeace = " + p.indexPeace + " - indexHouse = " + p.indexHouse);
                lpeacehouses.Add(p.indexHouse);
            }
        }

        int iCountHouses = lpeacehouses.Count;
        Debug.Log("ChoosePart - iCountHouses = " + iCountHouses);

        int iRandomHouse = Random.Range(0, iCountHouses);
        Debug.Log("ChoosePart - iRandomHouse = " + iRandomHouse);

        int iSelectHouse = lpeacehouses[iRandomHouse];
        Debug.Log("ChoosePart - iSelectHouse = " + iSelectHouse);

        SelectPart(iRandomindexPeace, iSelectHouse);
    }

    private void SelectPart(int iPart, int iHouse)
    {
        foreach (Piece player in lplayers)
        {
            if (player.indexCurrentField == iPart)
            {
                PlayerSelect = player;
            }
        }


        if (PlayerSelect)
        {
            PlayerSelect.SelectPeace();
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
        return;
        /*
        //Lista das peças do computador
        List<MyPeaces> mypeaces = new List<MyPeaces>();

        //Variavel que guarda a casa selecionada
        int iHouseSelect = 0;        

        //array para as peças com casas livres
        int[] ipeaces = new int[lplayers.Count];

        //variavel para contar o loop e usar de indice
        int icountmypeaces = 0;

        //Separa as peças que tem casas livres e não seja bomba, bandeira e soldado
        foreach (Piece player in lplayers)
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

        foreach (Piece player in lplayers)
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
        */
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
    }
}
