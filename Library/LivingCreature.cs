using System.ComponentModel;

namespace CreaturesLibrary
{
    public abstract class LivingCreature : ILivingCreature, INotifyPropertyChanged
    {
        private double speed;

        public abstract string Name { get; }

        public double Speed
        {
            get => speed;
            protected set
            {
                if (speed != value)
                {
                    speed = value;
                    OnPropertyChanged();
                }
            }
        }

        public abstract void Speak();
        public abstract void Move();
        public abstract void Stop();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
