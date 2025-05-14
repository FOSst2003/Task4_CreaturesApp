// Models/Turtle.cs
public class Turtle : LivingCreature
{
    private const double MaxSpeed = 5;
    private const double Acceleration = 1;

    public override void Move()
    {
        Speed = Math.Min(Speed + Acceleration, MaxSpeed);
    }

    public override void Stop()
    {
        Speed = Math.Max(Speed - Acceleration, 0);
    }
}