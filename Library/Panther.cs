using System;

namespace CreaturesLibrary
{
    public class Panther : LivingCreature
    {
        private const double MaxSpeed = 50;
        private const double Acceleration = 5;

        public event EventHandler? Roar;
        public event EventHandler? ClimbedOnTree;

        public override string Name => "Panther";

        public override void Move()
        {
            Speed = Math.Min(Speed + Acceleration, MaxSpeed);
        }

        public override void Stop()
        {
            Speed = Math.Max(Speed - Acceleration, 0);
        }

        public override void Speak()
        {
            Roar?.Invoke(this, EventArgs.Empty);
        }

        public void ClimbTree()
        {
            ClimbedOnTree?.Invoke(this, EventArgs.Empty);
        }

        public void MakeSound()
        {
            Speak();
        }
    }
}
