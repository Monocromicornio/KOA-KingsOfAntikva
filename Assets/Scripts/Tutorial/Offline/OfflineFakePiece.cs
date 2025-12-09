using UnityEngine;

[RequireComponent(typeof(OfflinePiece))]
public class OfflineFakePiece : MonoBehaviour
{
    private OfflineAnimPiece anim;
    public OfflinePiece piece { get; private set; }
    private GameObject body => piece.body;

    [Header("Fake Piece")]
    [SerializeField]
    private GameObject fake;

    [SerializeField]
    private GameObject fakePiecePrefab;

    private void Awake()
    {
        piece = GetComponent<OfflinePiece>();
        anim = GetComponent<OfflineAnimPiece>();
    }

    private void OnEnable()
    {
        if (fake == null && fakePiecePrefab != null)
        {
            fake = Instantiate(fakePiecePrefab, transform.position, transform.rotation, transform);
        }
        ActiveFakePiece();
    }

    private void OnDisable()
    {
        ReturnToNormal();
    }

    private void ActiveFakePiece()
    {
        if (body == null || fake == null) return;
        if (!body.activeSelf) return;

        Vector3 vector3 = new Vector3(body.transform.position.x, 0, body.transform.position.z);
        fake.transform.position = vector3;
        fake.transform.rotation = transform.rotation;

        body.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        if (anim) anim.ChangeAnim(fake);
    }

    private void ReturnToNormal()
    {
        if (body == null || fake == null) return;
        if (body.activeSelf && !fake.activeSelf) return;

        body?.SetActive(true);
        fake?.SetActive(false);

        if (anim)
        {
            anim.ChangetoOld();
        }
    }

    public void Reveal()
    {
        ReturnToNormal();
    }

    private void OnDestroy()
    {
        ReturnToNormal();
    }

    private void Destroy()
    {
        Reveal();
    }

    private void Win()
    {
        Reveal();
    }
}