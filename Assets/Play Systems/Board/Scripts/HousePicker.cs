using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousePicker : MonoBehaviour
{
    // Start is called before the first frame update
    public string NickName;
    public string ColumnName;
    public int Index;
    public int Row;
    public int Column;
    public Transform Target;

    public bool Status = false;
    public bool Busy;

    public bool AttackMode = false;

    public GameObject BusyPiece;

    [SerializeField]
    TextMesh TxtForce;

    GameObject VisualActive;

    AudioSource Select;
    

    void Start()
    {
        if (GameObject.Find("Select"))
        {
            Select = GameObject.Find("Select").GetComponent<AudioSource>();
        }



        if (transform.Find("Preview"))
        {
            VisualActive = transform.Find("Preview").gameObject;
            VisualActive.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {

        Debug.Log("Selected field: " + name);
        Debug.Log("Selected NickName: " + NickName);
        Debug.Log("Index field: " + Index);
        Debug.Log("Column field: " + Column);
        Debug.Log("Row field: " + Row);        

        Selection();

    }

    private void Selection()
    {

        GameObject[] houses = GameObject.FindGameObjectsWithTag("Field");

        foreach(GameObject house in houses)
        {
            house.GetComponent<HousePicker>().Disable();
        }

        Select.Play();

        Status = true;
        VisualActive.SetActive(true);
        ActivateCollisionPieces();

    }

    public void Disable()
    {
        Status = false;
        VisualActive.SetActive(false);
    }

    void ActivateCollisionPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject piece in pieces)
        {
            (piece.GetComponent(typeof(BoxCollider)) as Collider).enabled = true;
        }
    }
}
