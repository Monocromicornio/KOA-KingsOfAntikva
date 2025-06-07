using com.onlineobject.objectnet;
using UnityEngine;

public class Teste : NetworkBehaviour
{
    NetworkManager networkManager => NetworkManager.Instance();
    public GameObject cube;
    public GameObject sphere;

    public override void OnNetworkStarted()
    {
        Debug.Log("Network is Initialized");
    }

    public void ActiveStart()
    {
        Debug.Log("This is an Active Object");
    }

    public void PassiveStart()
    {
        Debug.Log("This is a Passive Object");
    }

    public void ActiveUpdate()
    {
        cube.SetActive(true);
        sphere.SetActive(false);

        if (networkManager.IsClientConnection() && Input.GetKey(KeyCode.C))
        {
            print("CHANGE pass");
            ReleaseControl();
        }
    }

    public void PassiveUpdate()
    {
        cube.SetActive(false);
        sphere.SetActive(true);

        if (networkManager.IsClientConnection() && Input.GetKey(KeyCode.V))
        {
            print("CHANGE pass");
            TakeControl();
        }
    }
}
