using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenCapture
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static LogEvent LogEvent = new LogEvent();
        public MainWindow()
        {
            InitializeComponent();
            LogEvent.AddEventHandler += AddLog;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
           //var value =  CaptureHelper.ColorDifference("Image1.png","Image2.png");
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            CaptureHelper.GetCircleCount();
        }

        private void AddLog(object sender, LogEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lvLog.Items.Add(e.Message);
            }));
        }
    }
}
