// ViewModels/MainViewModel.cs
using System;
using System.ComponentModel;
using System.Windows.Input;


namespace CreaturesApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Panther _panther;
        private Dog _dog;
        private Turtle _turtle;
        private string _soundText;
        private string _actionLog;

        public Panther Panther
        {
            get => _panther;
            set
            {
                _panther = value;
                OnPropertyChanged();
            }
        }

        public Dog Dog
        {
            get => _dog;
            set
            {
                _dog = value;
                OnPropertyChanged();
            }
        }

        public Turtle Turtle
        {
            get => _turtle;
            set
            {
                _turtle = value;
                OnPropertyChanged();
            }
        }

        public string SoundText
        {
            get => _soundText;
            set
            {
                _soundText = value;
                OnPropertyChanged();
            }
        }

        public string ActionLog
        {
            get => _actionLog;
            set
            {
                _actionLog = value;
                OnPropertyChanged();
            }
        }

        public ICommand MoveCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand MakeSoundCommand { get; private set; }
        public ICommand ClimbTreeCommand { get; private set; }
        public ICommand ClearLogCommand { get; private set; }

        public MainViewModel()
        {
            Panther = new Panther();
            Dog = new Dog();
            Turtle = new Turtle();
            ActionLog = string.Empty;

            InitializeCommands();
            SubscribeToEvents();
        }

        private void InitializeCommands()
        {
            MoveCommand = new RelayCommand(ExecuteMove);
            StopCommand = new RelayCommand(ExecuteStop);
            MakeSoundCommand = new RelayCommand(ExecuteMakeSound);
            ClimbTreeCommand = new RelayCommand(ExecuteClimbTree, CanExecuteClimbTree);
            ClearLogCommand = new RelayCommand(ExecuteClearLog);
        }

        private void SubscribeToEvents()
        {
            Panther.Roar += (sender, e) =>
            {
                SoundText = "Рррр!";
                AddToLog("Пантера рычит: Рррр!");
            };

            Dog.Bark += (sender, e) =>
            {
                SoundText = "Гав!";
                AddToLog("Собака лает: Гав!");
            };

            Panther.ClimbedOnTree += (sender, e) =>
            {
                AddToLog("Пантера успешно взобралась на дерево!");
            };
        }

        private void ExecuteMove(object parameter)
        {
            if (parameter is LivingCreature creature)
            {
                creature.Move();
                AddToLog($"{GetCreatureName(creature)} двигается (скорость: {creature.Speed} км/ч)");
            }
        }

        private void ExecuteStop(object parameter)
        {
            if (parameter is LivingCreature creature)
            {
                creature.Stop();
                AddToLog($"{GetCreatureName(creature)} останавливается (скорость: {creature.Speed} км/ч)");
            }
        }

        private void ExecuteMakeSound(object parameter)
        {
            if (parameter is Panther panther)
            {
                panther.MakeSound();
            }
            else if (parameter is Dog dog)
            {
                dog.MakeSound();
            }
        }

        private void ExecuteClimbTree(object parameter)
        {
            if (parameter is Panther panther)
            {
                panther.ClimbTree();
            }
        }

        private bool CanExecuteClimbTree(object parameter)
        {
            return parameter is Panther;
        }

        private void ExecuteClearLog(object parameter)
        {
            ActionLog = string.Empty;
        }

        private void AddToLog(string message)
        {
            ActionLog += $"{DateTime.Now:T}: {message}\n";
        }

        private string GetCreatureName(LivingCreature creature)
        {
            if (creature is Panther) return "Пантера";
            if (creature is Dog) return "Собака";
            if (creature is Turtle) return "Черепаха";
            return "Существо";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}