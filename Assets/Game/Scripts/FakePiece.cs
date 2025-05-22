using UnityEngine;

public class FakePiece : Piece
{
    [Header("Fake Piece")]
    [SerializeField]
    GameObject obj;

    [SerializeField]
    GameObject fake;

    void Start()
    {
        fake.SetActive(false);
        Vector3 vector3 = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
        fake = Instantiate(fake, vector3, transform.rotation);
        fake.transform.parent = transform;

        obj.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        AnimPiece anim = GetComponent<AnimPiece>();
        if(anim) anim.ChangeAnim(fake);

        if (gChest != null) gChest.SetActive(false);
    }

    protected override void OnMouseDown()
    {
        if (!turn.isPlayerTurn) return;

        if (turn.currentePiece != null)
        {
            print("ME ATAQUE " + gameObject.name);
            turn.currentePiece.SelectedAField(field);
        }
    }

    public void Reveal()
    {
        switch (gameType)
        {
            //In normal mode only exclude soldier
            case GameMode.GameType.Normal:
                if (Types == ItemType.Soldado) return;
                break;
            //in hard mode include only bandeira e bomba
            case GameMode.GameType.Hard:
                if (Types != ItemType.Bandeira && Types != ItemType.Bomba) return;
                break;
        }

        if (Types == ItemType.Bandeira)
        {
            gChest.SetActive(true);
        }

        obj.gameObject.SetActive(true);
        fake.SetActive(false);
    }

    public void Attack()
    {
        Reveal();
    }

    public void Hitted()
    {
        Reveal();
    }

    public void Die()
    {
        Reveal();
    }

    public override void SetFirstField(GameField field)
    {
        base.SetFirstField(field);
        transform.Rotate(0, 180, 0, Space.Self);
    }
}