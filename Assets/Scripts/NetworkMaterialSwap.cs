using com.onlineobject.objectnet;
using UnityEngine;
using System.Collections;
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

    [SerializeField]
    Piece piece;

    private void Awake()
    {
        NetworkScriptsReference.IgnoreType(typeof(NetworkMaterialSwap));
    }

    private void Start()
    {
        UpdateMaterials();
    }

    //private void Start()
    //{
    //    piece = GetComponentInParent<Piece>();
    //    bool isOnline = MatchController.instance != null && MatchController.instance.hasConnection;

    //    if (!isOnline) return;

    //    Renderer renderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();

    //    if (renderer == null) return;

    //    NetworkManager nm = NetworkManager.Instance();

    //    Material[] materialsToApply = nm.IsServerConnection() ? hostMaterials : clientMaterials;

    //    if (materialsToApply == null || materialsToApply.Length == 0) return;

    //    renderer.materials = materialsToApply;
    //}

    private void UpdateMaterials()
    {
        // Pega a referência da Peça para saber se ela é nossa ou do inimigo
        Piece piece = GetComponentInParent<Piece>();
        if (piece == null) return;

        Renderer renderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        // IMPORTANTE: Aqui usamos a lógica de "Dono da Peça" em vez de "Papel da Rede"
        // Se isMyPiece for true (minha peça), usa o primeiro grupo de materiais
        // Se isMyPiece for false (peça inimiga), usa o segundo grupo
        //Material[] materialsToApply = piece.isMyPiece ? hostMaterials : clientMaterials;

        NetworkManager nm = NetworkManager.Instance();

        if (!nm.IsServerConnection() && piece.isMyPiece == true)
        {
            renderer.materials = clientMaterials;
        }
        else
        {
            renderer.materials = hostMaterials;
        }      

       
    }
}
