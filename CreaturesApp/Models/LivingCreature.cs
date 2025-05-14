// Models/LivingCreature.cs
using System.ComponentModel;

public abstract class LivingCreature : INotifyPropertyChanged
{
    private double speed;

    public double Speed
    {
        get { return speed; }
        protected set
        {
            if (speed != value)
            {
                speed = value;
                OnPropertyChanged();
            }
        }
    }

    public abstract void Move();
    public abstract void Stop();

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}