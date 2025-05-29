#nullable enable
using System.Reflection;

namespace CreaturesApp.Models
{
    public class MethodModel
    {
        public string? Name { get; set; }
        public MethodInfo? MethodInfo { get; set; }
    }
}