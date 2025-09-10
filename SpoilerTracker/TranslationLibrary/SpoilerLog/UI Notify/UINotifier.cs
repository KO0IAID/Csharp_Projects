using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationLibrary.SpoilerLog.UI_Notify
{
    public class UINotifier
    {

        public event Action? OnNotify;

        public void NotifyAll()
        {
            OnNotify?.Invoke();
        }
        
    }
}
