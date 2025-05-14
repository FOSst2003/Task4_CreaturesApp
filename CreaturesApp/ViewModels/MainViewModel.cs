// ViewModels/MainViewModel.cs
using System.ComponentModel;
using System.Windows.Input;

public class MainViewModel : INotifyPropertyChanged
{
    private Panther panther;
    private Dog dog;
    private Turtle turtle;
    private string soundText;

    public Panther Panther
    {
        get { return panther; }
        set
        {
            panther = value;
            OnPropertyChanged();
        }
    }

    public Dog Dog
    {
        get { return dog; }
        set
        {
            dog = value;
            OnPropertyChanged();
        }
    }

    public Turtle Turtle
    {
        get { return turtle; }
        set
        {
            turtle = value;
            OnPropertyChanged();
        }
    }

    public string SoundText
    {
        get { return soundText; }
        set
        {
            soundText = value;
            OnPropertyChanged();
        }
    }

    public ICommand MoveCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand MakeSoundCommand { get; }
    public ICommand ClimbTreeCommand { get; }

    public MainViewModel()
    {
        Panther = new Panther();
        Dog = new Dog();
        Turtle = new Turtle();

        MoveCommand = new RelayCommand(ExecuteMove);
        StopCommand = new RelayCommand(ExecuteStop);
        MakeSoundCommand = new RelayCommand(ExecuteMakeSound);
        ClimbTreeCommand = new RelayCommand(ExecuteClimbTree, CanExecuteClimbTree);

        Panther.Roar += (sender, e) => SoundText = "Рррр!";
        Dog.Bark += (sender, e) => SoundText = "Гав!";
    }

    private void ExecuteMove(object param)
    {
        if (param is LivingCreature creature)
            creature.Move();
    }

    private void ExecuteStop(object param)
    {
        if (param is LivingCreature creature)
            creature.Stop();
    }

    private void ExecuteMakeSound(object param)
    {
        if (param is Panther panther)
            panther.MakeSound();
        else if (param is Dog dog)
            dog.MakeSound();
    }

    private void ExecuteClimbTree(object param)
    {
        if (param is Panther panther)
            panther.ClimbTree();
    }

    private bool CanExecuteClimbTree(object param)
    {
        return param is Panther;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}