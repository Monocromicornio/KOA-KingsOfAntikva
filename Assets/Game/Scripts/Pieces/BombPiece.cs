public class BombPiece : InteractivePiece
{
    void Start()
    {
        force = int.MaxValue;
    }

    protected override void Notify(bool sucess, InteractivePiece target)
    {
        base.Notify(sucess, target);
        if(sucess) SendMessage("Destroy");
    }
}