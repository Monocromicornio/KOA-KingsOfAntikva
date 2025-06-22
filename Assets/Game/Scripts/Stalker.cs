using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stalker : MonoBehaviour
{
    // Start is called before the first frame update
    //[SerializeField]
    Transform tTarget;

    [SerializeField]
    Transform tPlayerVision;

    [SerializeField]
    float fDistancePlayer;

    [SerializeField]
    float fLookSpeed;

    [SerializeField]
    int order = 0;

    bool bAreaEnter = false;

    GameObject[] Enemys;

    Transform tPlayer;

    //List<float> EnemysDistance;



    // Use this for initialization
    void Start()
    {

        tPlayer = GameObject.FindGameObjectWithTag("Player").transform;

        Enemys = GameObject.FindGameObjectsWithTag("Enemy");

        int[] numeros = new int[5];

        float[] EnemysDistance = new float[Enemys.Length];

        int iEnemy = 0;

        if (Enemys.Length > 0)
        {
            foreach (GameObject enemy in Enemys)
            {
                //Debug.Log("Enemy distance = " + Distance(enemy.transform));                
                EnemysDistance[iEnemy] = Distance(enemy.transform);
                iEnemy++;
                //Debug.Log("Enemy distance = " + EnemysDistance[iEnemy]);
            }
        }

        //for(int i = 0; i < EnemysDistance.Length; i++)
        //{
        //    Debug.Log("Enemy distance = " + EnemysDistance[i]);
        //}


        if (order == 0)
        {
            if (Enemys.Length > 0)
            {
                tTarget = Enemys[0].transform;
            }

        }

        if (order > 0)
        {
            if (Enemys.Length > order)
            {
                if (Enemys[order])
                {
                    tTarget = Enemys[order].transform;
                }
                else
                {
                    tTarget = GameObject.FindGameObjectWithTag("Enemy").transform;
                }
                //Debug.Log("Enemy " + Enemys[order]);
            }
            else
            {
                Enemys = GameObject.FindGameObjectsWithTag("Enemy");

                if (Enemys.Length > 0)
                {
                    tTarget = Enemys[0].transform;
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

        OnAreaEnter();


    }

    void FixedUpdate()
    {
        if (bAreaEnter)
        {
            InDirection();
            LookFor(tPlayerVision);

        }
    }

    float Distance(Transform Target)
    {
        float fDist = Vector3.Distance(transform.position, Target.position);

        //float fDist = (transform.position.x - tTarget.position.x);

        return fDist;
    }

    void OnAreaEnter()
    {

        //Debug.Log("Distance(tPlayer) = " + Distance(tPlayer));

        if (Distance(tPlayer) >= fDistancePlayer)
        {
            bAreaEnter = true;
            //Debug.Log("Player Enter = " + bAreaEnter);
        }

        //if (Distance(tTarget) <= fDistanceLimit)
        //{
        //    Instantiate(gReleaseXP, transform.position, Quaternion.identity);

        //    XPpoints xp = GameObject.Find("eXPeriencePoints").GetComponent<XPpoints>();

        //    xp.AddPoint(iXPValue);

        //    Destroy(gameObject);
        //}
    }

    void InDirection()
    {
        tPlayerVision.LookAt(tTarget);
    }

    bool LookFor(Transform vTarget)
    {

        var targetPosition = vTarget.position;
        targetPosition.y = transform.position.y;

        bool bAlign = false;


        transform.rotation = Quaternion.Lerp(transform.rotation, vTarget.rotation, fLookSpeed);

        if (transform.rotation == Quaternion.Lerp(transform.rotation, vTarget.rotation, fLookSpeed))
        {

            //Debug.Log(name + " is align whith " + vTarget.name );
            bAlign = true;
        }

        return bAlign;
    }
}
