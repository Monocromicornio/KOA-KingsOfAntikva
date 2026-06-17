using UnityEngine;
#if UNITY_EDITOR
using System.IO;
#endif
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using Steamworks;
#endif

/// <summary>
/// Initializes Steam via Steamworks.NET and manages callback pumping.
/// Other scripts should check SteamInitializer.Initialized before calling any Steam API.
/// In the Editor, ensures steam_appid.txt exists before initialization.
/// </summary>
public class SteamInitializer : MonoBehaviour
{
    private const string STEAM_APP_ID = "4800480";
    private const uint STEAM_APP_ID_UINT = 4800480;

    /// <summary>
    /// Returns true when the Steam API has been successfully initialized.
    /// </summary>
    public static bool Initialized { get; private set; }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSteamAppIdFile()
    {
        string projectRoot = new DirectoryInfo(Application.dataPath).Parent.FullName;
        string appIdPath = Path.Combine(projectRoot, "steam_appid.txt");

        if (!File.Exists(appIdPath))
        {
            File.WriteAllText(appIdPath, STEAM_APP_ID);
            Debug.Log($"[SteamInitializer] Criado steam_appid.txt com App ID {STEAM_APP_ID} em: {appIdPath}");
        }
        else
        {
            string content = File.ReadAllText(appIdPath).Trim();
            if (string.IsNullOrEmpty(content) || content != STEAM_APP_ID)
            {
                File.WriteAllText(appIdPath, STEAM_APP_ID);
                Debug.Log($"[SteamInitializer] steam_appid.txt atualizado de '{content}' para App ID {STEAM_APP_ID}");
            }
        }
    }
#endif

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

#if !DISABLESTEAMWORKS && STEAMWORKS_NET
        if (Initialized)
        {
            return;
        }

        if (!Packsize.Test())
        {
            Debug.LogError("[SteamInitializer] Steamworks Packsize test falhou. Verifique a versao do Steamworks.NET.");
            return;
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[SteamInitializer] Steamworks DllCheck test falhou. Verifique as DLLs nativas do Steam.");
            return;
        }

        try
        {
#if !UNITY_EDITOR
            // In builds, verify the game was launched through Steam with the correct AppID.
            // If not, Steam client will relaunch the game properly.
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(STEAM_APP_ID_UINT)))
            {
                Debug.Log($"[SteamInitializer] RestartAppIfNecessary retornou true para AppID {STEAM_APP_ID_UINT}. Reiniciando via Steam...");
                Application.Quit();
                return;
            }
#endif
            string errMsg;
            ESteamAPIInitResult initResult = SteamAPI.InitEx(out errMsg);

            if (initResult == ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
            {
                Initialized = true;
                Debug.Log($"[SteamInitializer] Steam inicializado com sucesso. AppID reportado: {SteamUtils.GetAppID()}");

                // Validate that the reported AppID matches the expected one
                AppId_t reportedAppId = SteamUtils.GetAppID();
                if (reportedAppId.m_AppId != STEAM_APP_ID_UINT)
                {
                    Debug.LogError($"[SteamInitializer] DIVERGENCIA DE APPID! Esperado: {STEAM_APP_ID_UINT}, Reportado: {reportedAppId.m_AppId}. Verifique se steam_appid.txt esta correto.");
                }
            }
            else
            {
                Debug.LogError($"[SteamInitializer] Steam falhou ao inicializar: {initResult} - {errMsg}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SteamInitializer] Excecao ao inicializar Steam: {ex.Message}");
        }
#else
        Debug.LogWarning("[SteamInitializer] Steamworks desabilitado ou STEAMWORKS_NET nao definido.");
#endif
    }

#if !DISABLESTEAMWORKS && STEAMWORKS_NET
    void Update()
    {
        if (Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    void OnApplicationQuit()
    {
        if (Initialized)
        {
            SteamAPI.Shutdown();
            Initialized = false;
            Debug.Log("[SteamInitializer] Steam desligado.");
        }
    }
#endif
}
