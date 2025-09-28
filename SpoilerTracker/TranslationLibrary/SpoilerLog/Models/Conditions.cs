using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationLibrary.SpoilerLog.Interfaces;

namespace TranslationLibrary.SpoilerLog.Models
{
    // Named to Conditions Instead of Condition to avoid ambiguity of System.Windows.Condition
    public class Conditions : INameValue
    {
        public string? Type {  get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int? Amount { get; set; }

        public Conditions(string? type, string? name, string? value, int? amount = null)
        {
            Type = type;
            Name = name;
            Value = value;
            Amount = amount;
        }
    }
}
