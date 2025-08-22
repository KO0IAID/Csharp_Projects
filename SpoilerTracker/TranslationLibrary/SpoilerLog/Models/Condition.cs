using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class Condition
    {
        public string? Type {  get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int? Amount { get; set; }

        public Condition(string? type, string? name, string? value, int? amount = null)
        {
            Type = type;
            Name = name;
            Value = value;
            Amount = amount;
        }
    }
}
