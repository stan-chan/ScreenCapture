using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class LogHelper
    {
        private Action<string> _updateCallback;

        public LogHelper(Action<string> updateCallback)
        {
            _updateCallback = updateCallback;
        }

        public void TriggerUpdate()
        {
            // 调用回调方法通知主窗口
            _updateCallback?.Invoke("子类通过回调更新了标签内容");
        }
    }
}
