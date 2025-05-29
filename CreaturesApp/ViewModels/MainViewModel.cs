#nullable enable
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CreaturesApp.Models;

namespace CreaturesApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<string> ClassNames { get; } = new();
        public ObservableCollection<MethodModel> Methods { get; } = new();
        public ObservableCollection<ParameterModel> Parameters { get; } = new();

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
                // Находим интерфейс по имени внутри сборки
                interfaceType = loadedAssembly.GetTypes()
                    .FirstOrDefault(t => t.IsInterface && t.Name.Equals("ILivingCreature", StringComparison.OrdinalIgnoreCase));
                if (interfaceType == null)
                {
                    MessageBox.Show("Интерфейс ILivingCreature не найден в сборке.");
                    return;
                }

                // Ищем классы, реализующие этот интерфейс
                var types = loadedAssembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract
                                && t.GetInterfaces().Any(i => i.Name.Equals(interfaceType.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(t => t.FullName!)
                    .ToList();

                if (!types.Any())
                {
                    var all = string.Join("\n", loadedAssembly.GetTypes().Select(t => t.FullName));
                    MessageBox.Show($"В сборке нет классов, реализующих ILivingCreature. Типы:\n{all}");
                }

                ClassNames.Clear();
                foreach (var name in types)
                    ClassNames.Add(name);

                // Сброс выбора
                SelectedClass = string.Empty;
                Methods.Clear();
                Parameters.Clear();
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

            if (loadedAssembly == null || string.IsNullOrEmpty(SelectedClass))
                return;

            selectedType = loadedAssembly.GetType(SelectedClass!);
            if (selectedType == null)
            {
                MessageBox.Show("Не удалось найти выбранный тип в сборке.");
                return;
            }

            var methods = selectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(mi => !mi.IsSpecialName)
                .ToList();

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

        private void InvokeSelectedMethod()
        {
            try
            {
                if (selectedType == null || SelectedMethod == null)
                    return;

                var instance = Activator.CreateInstance(selectedType!);
                var args = Parameters.Select(p => Convert.ChangeType(p.Value, p.Type!)).ToArray();
                var result = SelectedMethod.MethodInfo.Invoke(instance, args);
                MessageBox.Show(result != null ? result.ToString()! : "Выполнено успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при вызове метода: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
