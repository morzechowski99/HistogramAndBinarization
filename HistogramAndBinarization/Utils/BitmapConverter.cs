using HistogramAndBinarization.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HistogramAndBinarization.Utils
{
    public static class BitmapConverter
    {
        public static int[,] CalculateHistogram(Bitmap bitmap)
        {
            int[,] histogramValues = new int[4, 256];
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    histogramValues[0, color.R]++;
                    histogramValues[1, color.G]++;
                    histogramValues[2, color.B]++;
                    histogramValues[3, (int)((color.R + color.G + color.B) / 3)]++;
                }
            return histogramValues;
        }

        public static Bitmap HistogramExtending(Bitmap bitmap)
        {
            int[] min;
            int[] max;
            GetMinAndMax(bitmap, out min, out max);
            Bitmap newBitmap = new(bitmap);
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var newR = (255 / (max[0] - min[0])) * (color.R - min[0]);
                    var newG = (255 / (max[1] - min[1])) * (color.G - min[1]);
                    var newB = (255 / (max[2] - min[2])) * (color.B - min[2]);
                    newBitmap.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            return newBitmap;
        }
        
        public static Bitmap HistogramEqualization(Bitmap bitmap)
        {
            var histogram = CalculateHistogram(bitmap);
            var distribution = CalculateDistribution(histogram);
            Bitmap newBitmap = new(bitmap);
            var pixels = bitmap.Width * bitmap.Height;
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var newR = (int)((distribution[0,color.R] - distribution[0,0]) / (double)(pixels - distribution[0,0]) * 255);
                    var newG = (int)((distribution[1,color.G] - distribution[1,0]) / (double)(pixels - distribution[1, 0]) * 255);
                    var newB = (int)((distribution[2,color.B] - distribution[2,0]) / (double)(pixels - distribution[2, 0]) * 255);
                    newBitmap.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            return newBitmap;
        } 
        
        public static Bitmap ThresholdByValue(Bitmap bitmap, byte value)
        {
            Bitmap newBitmap = new(bitmap);
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var newValue = (color.R + color.G + color.B) / 3 < value ? 0 : 255;
                    newBitmap.SetPixel(i, j, Color.FromArgb(newValue, newValue, newValue));
                }
            return newBitmap;
        }
        
        public static Bitmap ThresholdByPercent(Bitmap bitmap, byte percents)
        {
            Bitmap newBitmap = new(bitmap);
            var histogram = CalculateHistogram(bitmap);
            var treshold = GetTreshold(bitmap, percents,histogram.GetRow(3));
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var newValue = (color.R + color.G + color.B) / 3 < treshold ? 0 : 255;
                    newBitmap.SetPixel(i, j, Color.FromArgb(newValue, newValue, newValue));
                }
            return newBitmap;
        }

        private static int GetTreshold(Bitmap bitmap, byte percents,int[] histogram)
        {
            var treshold = bitmap.Width * bitmap.Height * (percents / 100.0);
            var counter = 0;
            for(int i = 0; i < 256; i++)
            {
                counter += histogram[i];
                if (counter > treshold)
                    return i;
            }
            return 255;
        }

        private static int[,] CalculateDistribution(int[,] histogram)
        {
            int r = 0, g = 0, b = 0;
            int[,] distribution = new int[3, 256];
            for(int i = 0; i < 256; i++)
            {
                r += histogram[0, i];
                g += histogram[1, i];
                b += histogram[2, i];
                distribution[0, i] = r;
                distribution[1, i] = g;
                distribution[2, i] = b;
            }
            return distribution;
        }

        private static void GetMinAndMax(Bitmap bitmap, out int[] min, out int[] max)
        {
            min = new int[3] { 255, 255, 255 };
            max = new int[3] { 0, 0, 0 };
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    min[0] = color.R < min[0] ? color.R : min[0];
                    min[1] = color.G < min[1] ? color.G : min[1];
                    min[2] = color.B < min[2] ? color.B : min[2];
                    max[0] = color.R > max[0] ? color.R : max[0];
                    max[1] = color.G > max[1] ? color.G : max[1];
                    max[2] = color.B > max[2] ? color.B : max[2];
                }
        }

        public static Bitmap GetBitmapFromBitmapImage(BitmapImage image)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(image));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
