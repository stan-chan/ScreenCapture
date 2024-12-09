using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace ScreenCapture
{
    public class LogEvent
    {
        public event EventHandler<LogEventArgs> AddEventHandler;
        //public event EventHandler<LogEventArgs> ClearEventHandler;

        public void Add(string message)
        {
            var arg = new LogEventArgs(message);
            if (AddEventHandler != null)
            {
                AddEventHandler(null, arg);
            }
        }
    }
}
