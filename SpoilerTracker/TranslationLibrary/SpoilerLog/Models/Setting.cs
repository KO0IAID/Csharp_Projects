using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationLibrary.SpoilerLog.Interfaces;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class Setting : ICreateFromLine<Setting>
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int LogOrder { get; set; } = 0;

        public Setting CreateFromLine(string line)
        {
            string[] parts = line.Split(':');
            return new Setting
            {
                Name = parts[0].Trim(),
                Value = parts[1].Trim(),
                LogOrder = LogOrder++
            };
        }
    }
}
