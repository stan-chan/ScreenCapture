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


        public static void GetCircleCount()
        {
            // 读取图像
            Mat image = Cv2.ImRead("Image/red6_white1.jpg", ImreadModes.Color);
            var width = image.Width;
            var height = image.Height;
            // 定义 ROI 区域
            OpenCvSharp.Rect roi = new OpenCvSharp.Rect(1800, 1100, 500, 500); // 起点 (50, 50)，宽 200，高 150

            // 提取 ROI 子区域
            Mat roiImg = new Mat(image, roi);
            // 将图像转换为灰度图
            Mat grayImage = new Mat();
            Cv2.CvtColor(roiImg, grayImage, ColorConversionCodes.BGR2GRAY);

            // 对图像进行高斯模糊，以减少噪声
            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(grayImage, blurredImage, new OpenCvSharp.Size(9, 9), 2, 2);

            // 使用霍夫圆变换检测圆形
            CircleSegment[] circles = Cv2.HoughCircles(
                blurredImage, 
                HoughModes.Gradient, 
                1, //累加器分辨率
                50, //圆心之间最小距离
                100, //Canny边缘检测高阈值
                20, //检测阈值
                5, //最小半径
                100//最大半径
                );

            // 绘制检测到的圆形并计数
            int circleCount = 0;
            if (circles.Length > 0)
            {
                foreach (var circle in circles)
                {
                    // 绘制圆圈
                    Cv2.Circle(blurredImage, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), (int)circle.Radius, new Scalar(0, 255, 0), 2);
                    // 绘制圆心
                    Cv2.Circle(blurredImage, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), 3, new Scalar(0, 0, 255), 3);
                    circleCount++;
                }
            }

            MainWindow.LogEvent.Add($"Detected Circles: {circleCount}");
            // 输出检测到的圆圈数量
            Console.WriteLine($"Detected Circles: {circleCount}");
            Cv2.ImWrite("Image/red6_white1_R.jpg",blurredImage);
            // 显示结果图像
            Cv2.ImShow($"Detected Circles Count: {circleCount}", blurredImage);


            //Cv2.ResizeWindow("Detected Circles", 1200,800);
        }
    }
}




