using UnityEngine;
using Steamworks;

public class SteamInitializer : MonoBehaviour
{
    public static bool Initialized { get; private set; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        try
        {
            if (!Initialized)
            {
                if (!Packsize.Test()) Debug.LogWarning("Steamworks Packsize mismatch.");
                if (!DllCheck.Test()) Debug.LogWarning("Steamworks DllCheck failed.");

                if (!SteamAPI.Init())
                {
                    Debug.LogError("SteamAPI.Init() falhou");
                    return;
                }
                Initialized = true;
                Debug.Log("Steam API inicializada ✅");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao iniciar SteamAPI: " + e.Message);
        }
    }

    void Update()
    {
        if (Initialized) SteamAPI.RunCallbacks();
    }

    void OnApplicationQuit()
    {
        if (Initialized) SteamAPI.Shutdown();
        Initialized = false;
    }
}
