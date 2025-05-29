#nullable enable
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CreaturesApp.Models;

namespace CreaturesApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Существующие коллекции
        public ObservableCollection<string> ClassNames { get; } = new();
        public ObservableCollection<MethodModel> Methods { get; } = new();
        public ObservableCollection<ParameterModel> Parameters { get; } = new();
        public ObservableCollection<string> LogEntries { get; } = new();
        public ObservableCollection<CharacteristicModel> CurrentCharacteristics { get; } = new();

        private string? _selectedClass;
        public string SelectedClass
        {
            get => _selectedClass ?? string.Empty;
            set
            {
                if (_selectedClass != value)
                {
                    _selectedClass = value;
                    OnPropertyChanged();
                    LoadMethodsForSelectedClass();
                }
            }
        }

        private MethodModel? _selectedMethod;
        public MethodModel? SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (_selectedMethod != value)
                {
                    _selectedMethod = value;
                    OnPropertyChanged();
                    LoadParametersForSelectedMethod();
                }
            }
        }

        public string DllPath { get; set; } = string.Empty;
        private Assembly? loadedAssembly;
        private Type? selectedType;
        private Type? interfaceType;
        private object? currentInstance;

        public ICommand LoadDllCommand { get; }
        public ICommand InvokeMethodCommand { get; }

        public MainViewModel()
        {
            LoadDllCommand = new RelayCommand(_ => LoadDll());
            InvokeMethodCommand = new RelayCommand(
                _ => InvokeSelectedMethod(),
                _ => selectedType != null && SelectedMethod != null
            );
        }

        private void LoadDll()
        {
            try
            {
                var ofd = new OpenFileDialog { Filter = "DLL files (*.dll)|*.dll" };
                if (ofd.ShowDialog() != true)
                    return;

                DllPath = ofd.FileName;
                OnPropertyChanged(nameof(DllPath));

                loadedAssembly = Assembly.LoadFrom(DllPath);
                interfaceType = loadedAssembly.GetTypes()
                    .FirstOrDefault(t => t.IsInterface && t.Name.Equals("ILivingCreature", StringComparison.OrdinalIgnoreCase));

                if (interfaceType == null)
                {
                    MessageBox.Show("Интерфейс ILivingCreature не найден в сборке.");
                    return;
                }

                var types = loadedAssembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract
                                && t.GetInterfaces().Any(i => i.Name.Equals(interfaceType.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(t => t.FullName!)
                    .ToList();

                ClassNames.Clear();
                foreach (var name in types)
                    ClassNames.Add(name);

                SelectedClass = string.Empty;
                Methods.Clear();
                Parameters.Clear();
                CurrentCharacteristics.Clear();
                currentInstance = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки DLL: {ex.Message}");
            }
        }

        private void LoadMethodsForSelectedClass()
        {
            Methods.Clear();
            Parameters.Clear();
            CurrentCharacteristics.Clear();

            if (loadedAssembly == null || string.IsNullOrEmpty(SelectedClass))
                return;

            selectedType = loadedAssembly.GetType(SelectedClass!);
            if (selectedType == null)
            {
                MessageBox.Show("Не удалось найти выбранный тип в сборке.");
                return;
            }

            currentInstance = Activator.CreateInstance(selectedType);
            LoadCharacteristics();

            // Фильтрация методов по типу
            var methods = selectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(mi => !mi.IsSpecialName && !IsStandardMethod(mi.Name))
                .ToList();

            // Скрытие метода Speak для черепахи
            if (selectedType.Name == "Turtle")
                methods = methods.Where(m => m.Name != "Speak").ToList();

            foreach (var mi in methods)
                Methods.Add(new MethodModel { Name = mi.Name, MethodInfo = mi });

            SelectedMethod = null;
        }

        private void LoadParametersForSelectedMethod()
        {
            Parameters.Clear();
            if (SelectedMethod?.MethodInfo == null)
                return;

            foreach (var p in SelectedMethod.MethodInfo.GetParameters())
            {
                Parameters.Add(new ParameterModel
                {
                    Name = p.Name,
                    Type = p.ParameterType,
                    Value = string.Empty
                });
            }
        }

        private void LoadCharacteristics()
        {
            CurrentCharacteristics.Clear();
            if (currentInstance == null)
                return;

            // Получаем общие свойства
            var commonProperties = selectedType!.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => IsRelevantProperty(p.Name))
                .ToList();

            foreach (var prop in commonProperties)
            {
                try
                {
                    var value = prop.GetValue(currentInstance);
                    CurrentCharacteristics.Add(new CharacteristicModel
                    {
                        Name = prop.Name,
                        Value = value?.ToString() ?? "null"
                    });
                }
                catch (Exception ex)
                {
                    CurrentCharacteristics.Add(new CharacteristicModel
                    {
                        Name = prop.Name,
                        Value = $"Ошибка: {ex.Message}"
                    });
                }
            }

            // Добавляем специфичные характеристики
            if (selectedType.Name == "Panther")
            {
                CurrentCharacteristics.Add(new CharacteristicModel
                {
                    Name = "Climb Tree",
                    Value = "Доступен"
                });
            }
        }

        private static bool IsStandardMethod(string methodName)
        {
            return methodName is "ToString" or "GetType" or "Equals" or "GetHashCode";
        }

        private static bool IsRelevantProperty(string propertyName)
        {
            return propertyName is "Speed" or "Health" or "Energy" or "Type";
        }

        private void InvokeSelectedMethod()
        {
            try
            {
                if (selectedType == null || SelectedMethod == null || currentInstance == null)
                    return;

                var args = Parameters.Select(p => Convert.ChangeType(p.Value, p.Type!)).ToArray();
                var result = SelectedMethod.MethodInfo.Invoke(currentInstance, args);

                string logMessage = result != null
                    ? $"Метод '{SelectedMethod.Name}' выполнен успешно. Результат: {result}"
                    : $"Метод '{SelectedMethod.Name}' выполнен успешно.";

                LogEntries.Add(logMessage);

                // Отображение звука при вызове Speak или MakeSound
                if (SelectedMethod.Name == "Speak")
                {
                    string sound = selectedType!.Name switch
                    {
                        "Dog" => "Гав-гав",
                        "Panther" => "Ррррр",
                        _ => "Неизвестный звук"
                    };
                    ShowTemporarySound(sound);
                }
                else if (SelectedMethod.Name == "MakeSound")
                {
                    string sound = selectedType!.Name switch
                    {
                        "Dog" => "Гав-гав",
                        _ => "Неизвестный звук"
                    };
                    ShowTemporarySound(sound);
                }

                LoadCharacteristics(); // Обновляем основные характеристики
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при вызове метода '{SelectedMethod?.Name}': {ex.Message}";
                LogEntries.Add(errorMessage);
                MessageBox.Show(errorMessage + "\n" + ex.StackTrace);
            }
        }

        private void ShowTemporarySound(string sound)
        {
            var soundEntry = new CharacteristicModel
            {
                Name = "Sound",
                Value = sound,
                IsTemporary = true
            };

            CurrentCharacteristics.Add(soundEntry);

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                CurrentCharacteristics.Remove(soundEntry);
            };
            timer.Start();
        }
    }

    public class CharacteristicModel
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsTemporary { get; set; } = false;
    }
}