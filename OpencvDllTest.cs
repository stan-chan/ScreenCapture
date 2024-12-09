using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class OpencvDllTest
    {
        [DllImport("Tuya.Vision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Add(int a, int b);

        [DllImport("Tuya.Vision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ImageHeight(string path);
        public static void Test()
        {
            var test = ImageHeight("./Image/white1.jpg");
            var tes = Add(1,4);
        }
    }
}
