using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoTMMSpoilerToTracker.SpoilerLog.Interfaces
{
    public interface ICreateFromLine<T>
    {
        T CreateFromLine(string line);
    }
}
