using UnityEngine;

public class ForceClearPlayerPrefs : MonoBehaviour
{
    public void Awake()
    {
        PlayerPrefs.DeleteAll();
    }
}
