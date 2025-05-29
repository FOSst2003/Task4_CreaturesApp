#nullable enable
using System;
using System.Reflection;

namespace CreaturesApp.Models
{
    public class ParameterModel
    {
        public string? Name { get; set; }
        public Type? Type { get; set; }
        public string? Value { get; set; }
    }
}