using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationLibrary.SpoilerLog.Interfaces
{
    public interface INameValueCount
    {
        string? Name { get; }
        string? Value { get; }
        int? Count { get; }
    }
}
