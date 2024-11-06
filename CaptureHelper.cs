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
        private static double ColorDifference(string image1,string image2)
        {
            Mat mat1 = new Mat(image1);
            Mat mat2 = new Mat(image2);
            if (mat1.Size() != mat2.Size() || mat1.Type() != mat2.Type())
            {
                throw new ArgumentException("Images must have the same size and type.");
            }
            Mat lab1 = new Mat();
            Mat lab2 = new Mat();
            Cv2.CvtColor(mat1, lab1, ColorConversionCodes.BGR2Lab);
            Cv2.CvtColor(mat2, lab2, ColorConversionCodes.BGR2Lab);
            double sum = 0.0;
            int rows = lab1.Rows;
            int cols = lab1.Cols * lab1.Channels();
            for(int i = 0;i<rows;i++)
            {
                var row1 = lab1.Row(i);
                var row2 = lab2.Row(i);
                for(int j = 0;i<cols;j+=3)
                {
                    double dL = row1.At<byte>(0,j)-row2.At<byte>(0,j);
                    double dA = row1.At<byte>(0, j + 1) - row2.At<byte>(0, j + 1);
                    double dB = row1.At<byte>(0, j + 2) - row2.At<byte>(0, j + 2);
                    sum += dL * dL + dA * dA + dB * dB;
                }
            }
            double mse = sum / (lab1.Rows * lab1.Cols);
            return Math.Sqrt(mse); //Lab空间的均方根色差
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




