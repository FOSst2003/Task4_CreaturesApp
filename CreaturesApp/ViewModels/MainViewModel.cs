using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using CreaturesApp.Models;
using CreaturesLibrary; // Теперь работает корректно

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
                _selectedClass = value;
                OnPropertyChanged();
                LoadMethodsForSelectedClass();
            }
        }

        private MethodModel? _selectedMethod;
        public MethodModel SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                _selectedMethod = value;
                OnPropertyChanged();
                LoadParametersForSelectedMethod();
            }
        }

        public string DllPath { get; set; } = string.Empty;
        private Assembly? loadedAssembly;
        private Type? selectedType;

        // Используем интерфейс из CreaturesLibrary
        private Type interfaceType = typeof(ILivingCreature);

        public ICommand LoadDllCommand { get; }
        public ICommand InvokeMethodCommand { get; }

        public MainViewModel()
        {
            LoadDllCommand = new RelayCommand(_ => LoadDll());
            InvokeMethodCommand = new RelayCommand(_ => InvokeSelectedMethod());
        }

        private void LoadDll()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "DLL files (*.dll)|*.dll" };
                if (ofd.ShowDialog() == true)
                {
                    DllPath = ofd.FileName;
                    loadedAssembly = Assembly.LoadFrom(DllPath);

                    var types = loadedAssembly.GetTypes()
                        .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract)
                        .ToList();

                    ClassNames.Clear();
                    foreach (var type in types)
                        ClassNames.Add(type.FullName);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки DLL: {ex.Message}");
            }
        }

        private void LoadMethodsForSelectedClass()
        {
            Parameters.Clear();
            if (loadedAssembly == null || string.IsNullOrEmpty(SelectedClass))
                return;

            selectedType = loadedAssembly.GetType(SelectedClass);
            if (selectedType == null)
            {
                System.Windows.MessageBox.Show("Не удалось найти тип");
                return;
            }

            Methods.Clear();
            foreach (var method in selectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (!method.IsSpecialName)
                    Methods.Add(new MethodModel { Name = method.Name, MethodInfo = method });
            }
        }

        private void LoadParametersForSelectedMethod()
        {
            Parameters.Clear();
            if (SelectedMethod?.MethodInfo == null)
                return;

            foreach (var param in SelectedMethod.MethodInfo.GetParameters())
            {
                Parameters.Add(new ParameterModel
                {
                    Name = param.Name,
                    Type = param.ParameterType,
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

                var instance = Activator.CreateInstance(selectedType);
                var parameterValues = Parameters.Select(p =>
                    Convert.ChangeType(p.Value, p.Type!)).ToArray(); // Type не может быть null здесь

                var result = SelectedMethod.MethodInfo.Invoke(instance, parameterValues);
                System.Windows.MessageBox.Show(
                    result != null ? result.ToString() : "Метод выполнен успешно.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка вызова метода: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}