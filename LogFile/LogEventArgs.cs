using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class LogEventArgs
    {
        public string Message { get; set; }

        public LogEventArgs(string msg)
        {
            this.Message = msg;
        }
    }
}
