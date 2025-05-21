using UnityEngine;

public class FakePiece : Piece
{
    [SerializeField]
    GameObject fake;

    void Start()
    {
        fake.SetActive(false);
        Vector3 vector3 = new Vector3(anim.transform.position.x, 0, anim.transform.position.z);
        fake = Instantiate(fake, vector3, transform.rotation);
        fake.transform.parent = transform;

        anim.gameObject.SetActive(false);
        fake.SetActive(true);
        fake.transform.rotation = transform.rotation;

        anim = fake.GetComponent<Animator>();
        if (gChest != null) gChest.SetActive(false);
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

        anim.gameObject.SetActive(true);
        fake.SetActive(false);
    }

    void Attack()
    {
        Reveal();
    }

    void Hitted()
    {
        Reveal();
    }

    void Die()
    {
        Reveal();
    }

    public override void SetFirstField(GameField field)
    {
        base.SetFirstField(field);
        transform.Rotate(0, 180, 0, Space.Self);
    }
}