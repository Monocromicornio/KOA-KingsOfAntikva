using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachinePlayer : MonoBehaviour
{
    Turn turn;
    List<Player> lplayers;

    [SerializeField]
    bool bPlayed = false;

    // Start is called before the first frame update   
    void Start()
    {
        turn = FindObjectOfType<Turn>();

        lplayers = new List<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(turn.TurnPlayer=="Enemy")
        {           
            if (turn.Liberate == true)
            {
                if (!bPlayed)
                {
                    //StartCoroutine(ResetPlay(3.5f));
                    bPlayed = true;
                    StartCoroutine(GetListParts(0.7f));
                    Debug.Log("GetListPeaces()");                    
                    Debug.Log("bPlayed() = " + bPlayed);
                }
            }
        }

        if (turn.TurnPlayer == "Player")
        {
            bPlayed = false;
        }

     }

    private IEnumerator GetListParts(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (lplayers.Count > 0)
        {
            lplayers.Clear();
        }               

        Player[] players = FindObjectsOfType<Player>();

        Debug.Log("players count = " + players.Length);
        
        //Colocar todas as peças inimigas na lista lplayers
        foreach(Player player in players)
        {
            if(player.tag == "Enemy")
            {
                if (player.Types != Player.ItemType.Bomba)
                {
                    if (player.Types != Player.ItemType.Bandeira)
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
        Debug.Log("ResetPlay - bPlayed = " + bPlayed);
        if (turn.TurnPlayer == "Enemy")
        {
            if (turn.Liberate == true)
            {
                if (bPlayed)
                {
                    bPlayed = false;
                }
            }
        }
    }

    Player PlayerSelect;

    private void SelectPartsFreeHouse()
    {
        List<MyPeaces> mypeaces = new List<MyPeaces>();

        //Limpar lista
        mypeaces.Clear();

        //Variavel que guarda a casa selecionada
        int iHouseSelect = 0;

        //array para as peças com casas livres
        int[] ipeaces = new int[lplayers.Count];

        //variavel para contar o loop e usar de indice
        int icountmypeaces = 0;

        //Separa as peças que tem casas livres e não seja bomba, bandeira e soldado
        foreach (Player player in lplayers)
        {
            Debug.Log("MachinePlayer - player name = " + player.name);
            Debug.Log("MachinePlayer - player iFieldLive = " + player.iFieldLive);
            Debug.Log("MachinePlayer - player.HousesFree().Length = " + player.HousesFree().Length);

            //array para colocar as casas livres
            int[] iHousesFree = player.HousesFree();

            if (iHousesFree.Length > 0)
            {
                if (player.Types != Player.ItemType.Bomba)
                {
                    if (player.Types != Player.ItemType.Bandeira)
                    {

                        Debug.Log("MachinePlayer player iFieldLive = " + player.iFieldLive);

                        ipeaces[icountmypeaces] = player.iFieldLive;

                        foreach (int i in iHousesFree)
                        {
                            Debug.Log("MachinePlayer player iFieldLive = " + player.iFieldLive + " - iHousesFree = " + i);

                            iHouseSelect = i;

                            if (iHouseSelect > 0)
                            {
                                mypeaces.Add(new MyPeaces() { indexPeace = player.iFieldLive, indexHouse = i });
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
        foreach (Player player in lplayers)
        {
            if (player.iFieldLive == iPart)
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
        int iMaxRows = FindObjectOfType<BoardController>().GetFields() + 1;
        Debug.Log("SelectPart iHouseFreeCount =" + iHouseFreeCount);

        if (iHouseFreeCount == 0)
        {
            SelectPartsFreeHouse();
        }
        else
        {
            int iSelectHouse = 0;

            FieldController[] fcs = FindObjectsOfType<FieldController>();

            foreach (int ihouses in ihousesfree)
            {
                int Index = IndexHouse(ihouses);

                Debug.Log("SelcetPart - fcs[" + Index + "].Busy = " + fcs[Index].Busy);

                if (iSelectHouse == 0)
                {
                    if (fcs[Index].Busy)
                    {
                        Debug.Log("SelcetPart - fcs[" + Index + "].BusyPiece.tag = " + fcs[Index].BusyPiece.tag);
                        if (fcs[Index].BusyPiece.tag == "Player")
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
        int[] ipeaces = new int[lplayers.Count];

        //variavel para contar o loop e usar de indice
        int icountmypeaces = 0;

        //Separa as peças que tem casas livres e não seja bomba, bandeira e soldado
        foreach (Player player in lplayers)
        {
            //Debug.Log("MachinePlayer - player name = " + player.name);
            //Debug.Log("MachinePlayer - player iFieldLive = " + player.iFieldLive);
            //Debug.Log("MachinePlayer - player.HousesFree().Length = " + player.HousesFree().Length);

            //array para colocar as casas livres
            int[] iHousesFree = player.HousesFree();

            if (iHousesFree.Length > 0)
            {
                if (player.Types != Player.ItemType.Soldado)
                {
                    if (player.Types != Player.ItemType.Bandeira)
                    {
                        if (player.Types != Player.ItemType.Bomba)
                        {
                            Debug.Log("MachinePlayer player iFieldLive = " + player.iFieldLive);

                            ipeaces[icountmypeaces] = player.iFieldLive;

                            foreach (int i in iHousesFree)
                            {
                                Debug.Log("MachinePlayer player iFieldLive = " + player.iFieldLive + " - iHousesFree = " + i);

                                iHouseSelect = i;

                                if (iHouseSelect > 0)
                                {
                                    mypeaces.Add(new MyPeaces() { indexPeace = player.iFieldLive, indexHouse = i });
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

        foreach (Player player in lplayers)
        {
            if (player.iFieldLive == ipeaces[iRandomHouse])
            {
                PlayerSelect = player;
            }
        }

        if (PlayerSelect)
        {
            Debug.Log("MachinePlayer PlayerSelect =" + PlayerSelect.name + " - iFieldLive = " + PlayerSelect.iFieldLive);
            PlayerSelect.SelectPeace();
            IEnumerator enumerator = SelectHouse(ihouses[iRandomHouse], 1.0f);
            StartCoroutine(enumerator);
        }
    }

    private IEnumerator SelectHouse(int house, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        HouseClick(house);
        //FieldController[] fcs = FindObjectsOfType<FieldController>();
        //int indexfield = IndexHouse(house);
        //fcs[indexfield].Selection();

    }

    void HouseClick(int house)
    {        
        FieldController[] fcs = FindObjectsOfType<FieldController>();        

        foreach (FieldController fc in fcs)
        {
            if (fc.Status == true)
            {
                if (fc.index == house)
                {
                    if (!fc.Busy)
                    {
                        fc.Selection();
                        break;
                    }
                    else
                    {
                        fc.BusyPiece.GetComponent<Player>().SelectPeace();
                        break;
                    }
                }
            }            
        }        
    }

    private List<int> iHousesFree()
    {        

        FieldController[] fcs = FindObjectsOfType<FieldController>();

        List<int> lfreehouses = new List<int>();

        foreach (FieldController fc in fcs)
        {
            if (fc.Status == true)
            {
                lfreehouses.Add(fc.index);
            }
        }

        return lfreehouses;
    }

    private List<int> iHousesFree(int iRemoveHouse)
    {

        FieldController[] fcs = FindObjectsOfType<FieldController>();

        List<int> lfreehouses = new List<int>();

        foreach (FieldController fc in fcs)
        {
            if (fc.Status == true)
            {
                lfreehouses.Add(fc.index);
            }
        }

        lfreehouses.Remove(iRemoveHouse);

        return lfreehouses;
    }

    private int IndexHouse(int index)
    {
        int indexfield = 0;
        FieldController[] fcs = FindObjectsOfType<FieldController>();

        int indexloop = 0;

        foreach (FieldController fc in fcs)
        {
            if (fc.index == index)
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
