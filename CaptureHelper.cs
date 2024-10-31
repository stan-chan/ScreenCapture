using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System;
using System.Windows.Forms;
using System.IO;
using OpenCvSharp;
using System.Drawing;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;

namespace ScreenCapture
{
    internal class CaptureHelper
    {
        public static void CaptureFullScreen()
        {
            var screen = Screen.PrimaryScreen.Bounds;
            using (var bitmap = new Bitmap(screen.Width, screen.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(screen.X, screen.Y, 0, 0, bitmap.Size);
                }

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                SaveBitmapSourceAsImage(bitmapSource,"test.jpg");
                Mat mat = BitmapSourceConverter.ToMat(bitmapSource);
                Mat mat1 = new Mat("test.jpg");
                Mat grayImage = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Scalar scalar = Cv2.Mean(grayImage);
                double ttt = scalar[0];
                Mat mat2 = new Mat("test2.jpg");

                Cv2.ImShow("1",mat1);
                //System.Windows.Clipboard.SetImage(bitmapSource);
            }
        }

        public static void SaveBitmapSourceAsImage(BitmapSource bitmapSource, string filePath)
        {
            // 创建编码器，例如PngBitmapEncoder
            BitmapEncoder encoder = new PngBitmapEncoder();

            // 将BitmapSource添加到编码器的帧
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            // 保存到文件
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}




