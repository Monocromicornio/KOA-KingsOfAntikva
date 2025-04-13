using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePiece : MonoBehaviour
{
    // Start is called before the first frame update
    public int iHouse = 0;
    int iHouseGo = 0;
    GameObject goChange;

    AudioSource Confirm;

    void Start()
    {

        if (GameObject.Find("Confirm"))
        {
            Confirm = GameObject.Find("Confirm").GetComponent<AudioSource>();
        }

    }
    
    private void OnMouseDown()
    {

        Debug.Log("OnMouseDown Piece: " + name);
        ChangeTo();
    }

    private void ChangeTo()
    {

        GameObject[] houses = GameObject.FindGameObjectsWithTag("Field");

        foreach (GameObject house in houses)
        {
            if (house.GetComponent<HousePicker>().Status)
            {
                Debug.Log("house.GetComponent<HousePicker>().Status.Index: " + house.GetComponent<HousePicker>().Index);
                iHouseGo = house.GetComponent<HousePicker>().Index;
                goChange = house.GetComponent<HousePicker>().BusyPiece;
            }
        }

        houses[iHouseGo].GetComponent<HousePicker>().BusyPiece = gameObject;
        transform.position = houses[iHouseGo].transform.position;        

        houses[iHouse].GetComponent<HousePicker>().BusyPiece = goChange;
        goChange.transform.position = houses[iHouse].transform.position;
        goChange.GetComponent<ChangePiece>().iHouse = houses[iHouse].GetComponent<HousePicker>().Index;

        iHouse = iHouseGo;

        houses[iHouse].GetComponent<HousePicker>().Disable();
        houses[iHouseGo].GetComponent<HousePicker>().Disable();

        DisableCollisionPieces();

        Confirm.Play();


    }

    public void SetHouse(int house)
    {
        iHouse = house;
    }

    void DisableCollisionPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject piece in pieces)
        {
            (piece.GetComponent(typeof(BoxCollider)) as Collider).enabled = false;
        }
    }

}
