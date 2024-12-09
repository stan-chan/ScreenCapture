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
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp.Aruco;

namespace ScreenCapture
{
    public struct ImageLight
    {
        public Color Color;

        public double Radius;

        public Scalar Scalar;
    }
    internal class CVHelper
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
        public static double ColorDifference(string image1, string image2)
        {
            Mat mat1 = new Mat(image1);
            Mat mat2 = new Mat(image2);
            if (mat1.Size() != mat2.Size() || mat1.Type() != mat2.Type())
            {
                throw new ArgumentException("Images must have the same size and type.");
            }
            Mat diff = new Mat();
            Cv2.Absdiff(mat1, mat2, diff);
            Scalar mean = Cv2.Mean(diff);
            double va = (mean[0] + mean[1] + mean[2]) / 3.0;
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


        public static bool ClassifyColorByBGR(string path, out List<ImageLight> lights)
        {
            lights = new List<ImageLight>();
            //count = 0;
            //color = Color.Empty;
            MainWindow.LogEvent.Add($"Image: {path}");
            Mat src = ReadImage(path);
            //Mat redMask = GetColorMask(src, KnownColor.Red);
            // 将图像转换为灰度图
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            // 对图像进行高斯模糊，以减少噪声
            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(gray, blurredImage, new OpenCvSharp.Size(9, 9), 2, 2);

            // 使用霍夫圆变换检测圆形
            CircleSegment[] circles = Cv2.HoughCircles(
                blurredImage,
                HoughModes.Gradient,
                1, //累加器分辨率
                50, //圆心之间最小距离
                100, //Canny边缘检测高阈值
                15, //检测阈值
                3, //最小半径
                100//最大半径
                );

            foreach (var circle in circles)
            {
                int centerX = (int)circle.Center.X;
                int centerY = (int)circle.Center.Y;
                int radius = (int)circle.Radius;

                // 创建掩膜，仅选中圆内像素
                Mat mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
                Cv2.Circle(mask, new OpenCvSharp.Point(centerX, centerY), radius, Scalar.White, -1);

                // 提取圆区域
                Mat circleRegion = new Mat();
                Cv2.BitwiseAnd(src, src, circleRegion, mask);

                // 转换为 HSV 色彩空间
                Mat hsv = new Mat();
                Cv2.CvtColor(circleRegion, hsv, ColorConversionCodes.BGR2HSV);

                // 计算圆区域的平均颜色
                Scalar meanColor = Cv2.Mean(circleRegion, mask);

                //// BGR 通道
                //double b = meanColor.Val0; 
                //double g = meanColor.Val1;
                //double r = meanColor.Val2; 
                var color = ClassifyColorByBGR(meanColor);

                // 绘制圆圈
                Cv2.Circle(src, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), (int)circle.Radius, new Scalar(0, 0, 255), 2);
                // 绘制圆心
                Cv2.Circle(src, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), 3, new Scalar(0, 255, 0), 2);
                
                lights.Add( new ImageLight() { Color = color, Radius = radius, Scalar = meanColor});
                MainWindow.LogEvent.Add($"圆心:({centerX},{centerY}),半径:{radius}, 颜色[{color.ToString()}],BGR[{meanColor.Val0:0.00}-{meanColor.Val1:0.00}-{meanColor.Val1:0.00}]");
            }

            Cv2.ImShow($"{path}", src);
            return true;
            // 绘制检测到的圆形并计数
            int circleCount = 0;
            if (circles.Length > 0)
            {
                foreach (var circle in circles)
                {
                    // 绘制圆圈
                    Cv2.Circle(blurredImage, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), (int)circle.Radius, new Scalar(0, 255, 0), 2);
                    // 绘制圆心
                    Cv2.Circle(blurredImage, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), 3, new Scalar(0, 0, 255), -1);
                    circleCount++;
                }
            }

            MainWindow.LogEvent.Add($"Detected Circles: {circleCount}");
            Cv2.ImWrite("Image/red6_white1_R.jpg", blurredImage);
            // 显示结果图像
            Cv2.ImShow($"Detected Circles Count: {circleCount}", blurredImage);
        }


        private static Color ClassifyColorByBGR(Scalar scalar)
        {
            double blue = Math.Round(scalar.Val0,2);
            double green = Math.Round(scalar.Val1, 2);
            double red = Math.Round(scalar.Val2, 2);
            Dictionary<Color, double> BGR = new Dictionary<Color, double>() { { Color.Blue,blue}, { Color.Green, green }, { Color.Red, red } };

            double max = BGR.Values.Max();
            double min = BGR.Values.Min();
            //BGR三通道之间色差值小于10，且均大于230时，视为白色
            if (max - min < 10 && min > 170)
            {
                return Color.White;
            }
            else if(max - min < 10 && max <50)
            {
                return Color.Black;
            }
            else if(max - min < 10)
            {
                return Color.Gray;
            }
            //色值最大的通道即视为该通道色
            return BGR.OrderByDescending(o => o.Value).First().Key;
        }


        private static string DetermineColor(double h, double s, double v)
        {
            if (s < 50) return "Gray/White"; // 饱和度低，可能是灰白色
            if (h < 10 || h > 160) return "Red";
            if (h >= 10 && h <= 25) return "Orange";
            if (h > 25 && h <= 35) return "Yellow";
            if (h > 35 && h <= 85) return "Green";
            if (h > 85 && h <= 130) return "Blue";
            if (h > 130 && h <= 160) return "Purple";
            return "Unknown";
        }


        public static Mat ReadImage(string path)
        {
            // 读取图像
            Mat image = Cv2.ImRead(path);
            // 定义 ROI 区域
            OpenCvSharp.Rect roi = new OpenCvSharp.Rect(1800, 1100, 500, 500);
            Mat roiImg = new Mat(image, roi);
            return roiImg;

        }
        public static Mat GetColorMask(Mat hsv, KnownColor color)
        {
            if (color == KnownColor.Red)
            {
                // 红色的 HSV 范围
                Scalar lowerRed1 = new Scalar(0, 100, 100);   // 红色范围1
                Scalar upperRed1 = new Scalar(10, 255, 255);
                Scalar lowerRed2 = new Scalar(160, 100, 100); // 红色范围2
                Scalar upperRed2 = new Scalar(179, 255, 255);

                // 创建红色掩膜
                Mat mask1 = new Mat();
                Mat mask2 = new Mat();
                Cv2.InRange(hsv, lowerRed1, upperRed1, mask1);
                Cv2.InRange(hsv, lowerRed2, upperRed2, mask2);
                Mat redMask = mask1 | mask2;
                return redMask;
            }
            return new Mat();
        }
    }
}





