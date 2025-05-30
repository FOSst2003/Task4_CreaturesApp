﻿using System;

namespace CreaturesLibrary
{
    public class Dog : LivingCreature
    {
        private const double MaxSpeed = 30;
        private const double Acceleration = 3;

        public event EventHandler? Bark;

        public override string Name => "Dog";

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
            Bark?.Invoke(this, EventArgs.Empty);
        }

        public void MakeSound()
        {
            Speak();
        }
    }
}
