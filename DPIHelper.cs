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
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace ScreenCapture
{
    public class DPIHelper
    {
        public static void DPITest()
        {
            Mat image = Cv2.ImRead("Image/清晰度.png");
            //600
            OpenCvSharp.Rect roi = new OpenCvSharp.Rect(540, 235, 430, 140);
            //800
            //OpenCvSharp.Rect roi = new OpenCvSharp.Rect(540, 580, 430, 145);
            Mat roiImg = new Mat(image, roi);
           // Cv2.ImShow("Test",roiImg);
            //return;
            Mat gray = roiImg.CvtColor(ColorConversionCodes.BGR2GRAY);

            Mat binary = new Mat();

            // 二值化处理
            Cv2.Threshold(gray, binary, 128, 255, ThresholdTypes.Binary);

            Mat edges = binary.Canny(50, 150);
            Cv2.ImShow("Edges", edges);

            // 查找轮廓
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // 筛选线条轮廓
            int lineCount = 0;
            foreach (var contour in contours)
            {
                // 计算轮廓的边界矩形
                Rect boundingBox = Cv2.BoundingRect(contour);

                // 筛选条件（例如：宽高比接近线条）
                double aspectRatio = (double)boundingBox.Width / boundingBox.Height;
                if (aspectRatio > 5 && boundingBox.Height < 20) // 调整条件以适应实际线条
                {
                    lineCount++;
                    Cv2.Rectangle(image, boundingBox, new Scalar(0, 255, 0), 2); // 可视化框选线条
                }
            }

            // 显示结果
            Cv2.ImShow("Detected Lines", image);
            Cv2.WaitKey();

            // 输出线条数量
            Console.WriteLine($"识别的线条数量：{lineCount}");
            return;
            double minVal, maxVal;
            Cv2.MinMaxLoc(gray, out minVal, out maxVal);
            double contrast = maxVal - minVal;
            Console.WriteLine($"对比度: {contrast}");

            Mat grayFloat = new Mat();
            gray.ConvertTo(grayFloat, MatType.CV_32F);
            Mat dftImage = new Mat();
            Cv2.Dft(grayFloat, dftImage, DftFlags.Scale | DftFlags.ComplexOutput);

            // 将频谱转换为可视化图像
            Mat magnitude = new Mat();
            Cv2.Magnitude(dftImage.Split()[0], dftImage.Split()[1], magnitude);
            Cv2.Normalize(magnitude, magnitude, 0, 255, NormTypes.MinMax);
            Cv2.ImShow("Frequency Spectrum", magnitude);


            int edgePixels = Cv2.CountNonZero(edges); // 边缘像素数量
            Console.WriteLine($"边缘像素数: {edgePixels}");
            //根据实际清晰的边缘像素数设置
            int threshold = 0;
            double contrastThreshold = 0.0;

            if (edgePixels > threshold && contrast > contrastThreshold)
            {
                Console.WriteLine("600线条清晰");
            }
            else
            {
                Console.WriteLine("600线条不清晰");
            }
        }
    }
}
