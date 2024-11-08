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

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                SaveBitmapSourceAsImage(bitmapSource, "test.jpg");
                Mat mat = BitmapSourceConverter.ToMat(bitmapSource);
                Mat mat1 = new Mat("test.jpg");
                Mat grayImage = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Scalar scalar = Cv2.Mean(grayImage);
                double ttt = scalar[0];
                Mat mat2 = new Mat("test2.jpg");

                Cv2.ImShow("1", mat1);
                //System.Windows.Clipboard.SetImage(bitmapSource);
            }
        }
        /// <summary>
        /// 求两张图片色差
        /// </summary>
        /// <param name="image1">图片1路径</param>
        /// <param name="image2">图片2路径</param>
        /// <returns>均方根色差</returns>
        public static double ColorDifference(string image1,string image2)
        {
            Mat mat1 = new Mat(image1);
            Mat mat2 = new Mat(image2);
            if (mat1.Size() != mat2.Size() || mat1.Type() != mat2.Type())
            {
                throw new ArgumentException("Images must have the same size and type.");
            }
            Mat diff = new Mat();
            Cv2.Absdiff(mat1,mat2,diff);
            Scalar mean = Cv2.Mean(diff);
            double va = (mean[0]+mean[1]+mean[2])/3.0;
            return va;
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




