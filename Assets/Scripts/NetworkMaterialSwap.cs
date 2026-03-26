using com.onlineobject.objectnet;
using UnityEngine;

/// <summary>
/// Swaps the materials of the target Renderer based on whether the local player is host or client.
/// Attach to the root piece GameObject. If targetRenderer is not set, uses the first Renderer found in children.
/// Registers itself as ignored by ObjectNet's NetworkScriptsReference so it is never disabled by the network layer.
/// </summary>
public class NetworkMaterialSwap : MonoBehaviour
{
    [SerializeField]
    private Renderer targetRenderer;

    [SerializeField]
    private Material[] hostMaterials;

    [SerializeField]
    private Material[] clientMaterials;

    private void Awake()
    {
        // Prevent ObjectNet's NetworkScriptsReference from disabling this component.
        NetworkScriptsReference.IgnoreType(typeof(NetworkMaterialSwap));
    }

    private void Start()
    {
        bool isOnline = MatchController.instance != null && MatchController.instance.hasConnection;

        if (!isOnline) return;

        Renderer renderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();

        if (renderer == null) return;

        NetworkManager nm = NetworkManager.Instance();
        Material[] materialsToApply = nm.IsServerConnection() ? hostMaterials : clientMaterials;

        if (materialsToApply == null || materialsToApply.Length == 0) return;

        renderer.materials = materialsToApply;
    }
}
