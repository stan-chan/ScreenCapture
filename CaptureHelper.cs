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
using System.Windows.Media;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using OpenCvSharp.Internal.Vectors;

namespace ScreenCapture
{
    internal class CaptureHelper
    {
        public static void ChatGPT()
        {
            // 读取图像
            Mat src = ReadImage("Image/red6_white1.jpg");

            // 转换为 HSV 色彩空间
            Mat hsv = new Mat();
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

            // 定义不同颜色的 HSV 范围
            Scalar lowerRed1 = new Scalar(0, 100, 100);
            Scalar upperRed1 = new Scalar(10, 255, 255);
            Scalar lowerRed2 = new Scalar(160, 100, 100);
            Scalar upperRed2 = new Scalar(179, 255, 255);

            Scalar lowerGreen = new Scalar(35, 100, 100);
            Scalar upperGreen = new Scalar(85, 255, 255);

            Scalar lowerBlue = new Scalar(100, 100, 100);
            Scalar upperBlue = new Scalar(130, 255, 255);

            // 创建颜色掩膜
            Mat redMask1 = new Mat();
            Mat redMask2 = new Mat();
            Mat greenMask = new Mat();
            Mat blueMask = new Mat();

            Cv2.InRange(hsv, lowerRed1, upperRed1, redMask1);
            Cv2.InRange(hsv, lowerRed2, upperRed2, redMask2);
            Cv2.InRange(hsv, lowerGreen, upperGreen, greenMask);
            Cv2.InRange(hsv, lowerBlue, upperBlue, blueMask);

            // 合并红色掩膜
            Mat redMask = redMask1 | redMask2;

            // 形态学操作：去除中心的白色区域（腐蚀操作）
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(1, 1));
            Mat redMaskWithoutCenter = new Mat();
            Mat greenMaskWithoutCenter = new Mat();
            Mat blueMaskWithoutCenter = new Mat();

            Cv2.Erode(redMask, redMaskWithoutCenter, kernel, iterations: 1);
            Cv2.Erode(greenMask, greenMaskWithoutCenter, kernel, iterations: 1);
            Cv2.Erode(blueMask, blueMaskWithoutCenter, kernel, iterations: 1);

            // 检测红色、绿色和蓝色区域的圆形
            DetectLights(redMaskWithoutCenter, src, "Red");
            DetectLights(greenMaskWithoutCenter, src, "Green");
            DetectLights(blueMaskWithoutCenter, src, "Blue");

            // 显示结果
            Cv2.ImShow("Detected Lights", src);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

        // 检测圆形并绘制
        static void DetectLights(Mat mask, Mat src, string colorName)
        {
            // 使用 Canny 边缘检测
            Mat edges = new Mat();
            Cv2.Canny(mask, edges, 50, 150);

            //// 霍夫圆变换检测圆
            //CircleSegment[] circles = Cv2.HoughCircles(
            //    edges,
            //    HoughModes.Gradient,
            //    dp: 1,             // 累加器分辨率
            //    minDist: 20,       // 圆心之间的最小距离
            //    param1: 100,       // Canny 高阈值
            //    param2: 20,        // 圆心检测阈值
            //    minRadius: 2,     // 最小半径
            //    maxRadius: 100     // 最大半径
            //);

            CircleSegment[] circles = Cv2.HoughCircles(
                edges,
                HoughModes.Gradient,
                1, //累加器分辨率
                50, //圆心之间最小距离
                100, //Canny边缘检测高阈值
                20, //检测阈值
                5, //最小半径
                100//最大半径
            );
            MainWindow.LogEvent.Add($"检测到圆数量：{circles.Length}");
            // 绘制检测到的圆并标记颜色
            foreach (var circle in circles)
            {
                int centerX = (int)circle.Center.X;
                int centerY = (int)circle.Center.Y;
                int radius = (int)circle.Radius;

                // 根据颜色绘制圆
                Scalar circleColor = colorName == "Red" ? Scalar.Red :
                                     colorName == "Green" ? Scalar.Green :
                                     Scalar.Blue;

                Cv2.Circle(src, new OpenCvSharp.Point(centerX, centerY), radius, circleColor, 2); // 绘制圆
                Cv2.PutText(src, $"{colorName} Light", new OpenCvSharp.Point(centerX - radius, centerY - radius - 10),
                            HersheyFonts.HersheySimplex, 0.5, circleColor, 1);
                MainWindow.LogEvent.Add($"圆心: ({centerX}, {centerY}), 半径: {radius}, 颜色: {circleColor}");

            }
        }



        public static void GetCircleCount()
        {
            Mat src = ReadImage("Image/red6_white1.jpg");
            //hsv
            //Mat hsv = new Mat();
            //Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

            Mat redMask = GetColorMask(src, KnownColor.Red);
            //灰度
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            //// 反转灰度图，白色区域变黑，其他区域变白
            //Mat invertedGray = new Mat();
            //Cv2.BitwiseNot(gray, invertedGray);

            //// 去除中心的白色区域：腐蚀操作
            //Mat maskWithoutCenter = new Mat();
            //Cv2.Erode(redMask, maskWithoutCenter, new Mat(), iterations: 1);

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
                20, //检测阈值
                5, //最小半径
                100//最大半径
                );

            foreach(var circle in circles)
            {
                Cv2.Circle(src, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), (int)circle.Radius, new Scalar(0, 0, 255), 1);
                // 绘制圆心
                Cv2.Circle(src, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), 3, new Scalar(0, 0, 255), 3);

                int centerX = (int)circle.Center.X;
                int centerY = (int)circle.Center.Y;
                int radius = (int)circle.Radius;

                // 创建掩膜，仅选中圆内像素
                Mat mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
                Cv2.Circle(mask, new OpenCvSharp.Point(centerX, centerY), radius, Scalar.White, -1);

                // 提取圆区域
                Mat circleRegion = new Mat();
                Cv2.BitwiseAnd(src, src, circleRegion, mask);



                //// 转换为 HSV 色彩空间
                //Mat hsv = new Mat();
                //Cv2.CvtColor(circleRegion, hsv, ColorConversionCodes.BGR2HSV);

                ////Cv2.ImShow($"Detected Circle", circleRegion);
                ////return;
                // 计算圆区域的平均颜色
                Scalar meanColor = Cv2.Mean(circleRegion, mask);

                // HSV 通道
                double h = meanColor.Val0; // 色调
                double s = meanColor.Val1; // 饱和度
                double v = meanColor.Val2; // 明度

                //// 判断颜色
                //string color = DetermineColor(h, s, v);

                MainWindow.LogEvent.Add($"圆心: ({centerX}, {centerY}), 半径: {radius}, 颜色: {circleRegion},BGR:{h:0.00},{s:0.00},{v:0.00}");
                //// 计算圆内平均颜色
                //Scalar meanColor = Cv2.Mean(roiImg, mask);
                //MainWindow.LogEvent.Add($"圆心: ({centerX}, {centerY}), 半径: {radius}, 平均颜色: BGR({meanColor.Val0}, {meanColor.Val1}, {meanColor.Val2})");
            }
            //return;
            //// 绘制检测到的圆形并计数
            //int circleCount = 0;
            //if (circles.Length > 0)
            //{
            //    foreach (var circle in circles)
            //    {
            //        // 绘制圆圈
            //        Cv2.Circle(blurredImage, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), (int)circle.Radius, new Scalar(0, 255, 0), 2);
            //        // 绘制圆心
            //        Cv2.Circle(blurredImage, new OpenCvSharp.Point(circle.Center.X, circle.Center.Y), 3, new Scalar(0, 0, 255), 3);
            //        circleCount++;
            //    }
            //}

            //MainWindow.LogEvent.Add($"Detected Circles: {circleCount}");
            //Cv2.ImWrite("Image/red6_white1_R.jpg",blurredImage);
            // 显示结果图像
            Cv2.ImShow($"Detected Circles", src);
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


        #region Next
        public static void ChatGPT1()
        {
            // 读取图像
            Mat src = ReadImage("Image/red6_white1.jpg");
            //Cv2.ImShow("12", src);
            // 转换为灰度图
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            // 二值化处理，提取白色区域
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, thresh: 200, 255, ThresholdTypes.Binary);
            Cv2.ImShow("12", binary);
            var chane = binary.Channels();
            var depth = binary.Depth();
            if (binary.Channels() != 1 || binary.Depth() !=0)
            {
                Console.WriteLine("Error: Binary image should have 1 channel.");
                return;
            }
            // 检测轮廓
            //VectorOfVectorPoint contours = new VectorOfVectorPoint();
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // 转换为 HSV 色彩空间
            Mat hsv = new Mat();
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

            // 颜色分类
            foreach (var contour in contours)
            {
                // 忽略小的噪声
                if (Cv2.ContourArea(contour) < 50)
                    continue;

                // 获取轮廓的外接圆
                Point2f center;
                float radius;
                Cv2.MinEnclosingCircle(contour, out center, out radius);

                // 定义感兴趣区域（ROI）
                Rect roi = new Rect(
                    x: Math.Max(0, (int)(center.X - radius)),
                    y: Math.Max(0, (int)(center.Y - radius)),
                    width: Math.Min(src.Width - (int)(center.X - radius), (int)(2 * radius)),
                    height: Math.Min(src.Height - (int)(center.Y - radius), (int)(2 * radius))
                );

                // 提取 ROI 中的边缘颜色
                Mat roiMask = new Mat(binary, roi); // 使用二值图的 ROI 掩膜
                Mat roiHsv = new Mat(hsv, roi);    // 使用 HSV 图像的 ROI 区域

                // 计算边缘的主颜色
                Scalar averageColor = GetAverageColor(roiMask, roiHsv);

                // 判断颜色类型
                string colorName = ClassifyColor(averageColor);

                // 绘制结果
                Cv2.Circle(src, (Point)center, (int)radius, Scalar.Green, 2);
                Cv2.PutText(src, colorName, (Point)center, HersheyFonts.HersheySimplex, 0.6, Scalar.White, 2);
            }

            // 显示结果
            Cv2.ImShow("Detected Lights", src);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

        // 计算边缘的平均颜色
        static Scalar GetAverageColor(Mat mask, Mat hsv)
        {
            // 对掩膜图和 HSV 图像按位操作，提取有效区域
            Mat maskedHsv = new Mat();
            Cv2.BitwiseAnd(hsv, hsv, maskedHsv, mask);

            // 计算非零像素的平均颜色
            var meanColor = Cv2.Mean(maskedHsv, mask);
            return meanColor;
        }

        // 判断颜色类型
        static string ClassifyColor(Scalar color)
        {
            double hue = color[0]; // H 通道
            double saturation = color[1]; // S 通道
            double value = color[2]; // V 通道

            if (saturation < 50)
                return "White"; // 去掉饱和度低的颜色（避免误判）

            if (hue >= 0 && hue <= 10 || hue >= 160 && hue <= 179)
                return "Red";
            else if (hue > 10 && hue <= 35)
                return "Yellow";
            else if (hue > 35 && hue <= 85)
                return "Green";
            else if (hue > 85 && hue <= 130)
                return "Blue";
            else if (hue > 130 && hue <= 160)
                return "Purple";

            return "Unknown";
        }
        #endregion
    }
}




