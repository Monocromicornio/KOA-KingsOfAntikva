using com.onlineobject.objectnet;
using UnityEngine;

public class PositionProvider : MonoBehaviour, IInformationProvider
{
    [Header("This attribute will return value of \"GetSpawnPosition\" method")]
    public Vector3 PositionToSpawn;

    public Vector3 GetSpawnPosition() {
        print("AAAAAAAAAAAAAAAAAAA");
        return PositionToSpawn;
    }

    public Vector3 GetSpawnPositonToGroup() {
        return Vector3.zero;
    }
}
