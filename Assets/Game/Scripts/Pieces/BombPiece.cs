public class BombPiece : InteractivePiece
{
    void Start()
    {
        force = int.MaxValue;
    }

    public override void Hitted(InteractivePiece target, int force)
    {
        base.Hitted(target, force);

        if (this.force < force) return;
        
        piece.SetDie();
    }
}