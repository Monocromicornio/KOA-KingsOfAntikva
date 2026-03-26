using com.onlineobject.objectnet;
using UnityEngine;

public class NetworkMaterialSwap : MonoBehaviour
{
    [SerializeField]
    private Renderer targetRenderer;

    [SerializeField]
    private Material[] hostMaterials; // Renomeie no Inspector para "Minhas Peças (Azul)"

    [SerializeField]
    private Material[] clientMaterials; // Renomeie no Inspector para "Inimigos (Vermelho)"

    private void Awake()
    {
        // Impede que o ObjectNet desative este script automaticamente
        NetworkScriptsReference.IgnoreType(typeof(NetworkMaterialSwap));
    }

    private void Start()
    {
        UpdateMaterials();
    }

    private void OnEnable()
    {
        UpdateMaterials();
    }

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
        Material[] materialsToApply = piece.isMyPiece ? clientMaterials : hostMaterials;

        if (materialsToApply == null || materialsToApply.Length == 0) return;

        renderer.materials = materialsToApply;
    }
}
