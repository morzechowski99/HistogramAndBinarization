using Microsoft.Win32;
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
using LiveCharts;
using LiveCharts.Wpf;
using HistogramAndBinarization.Utils;
using HistogramAndBinarization.Extensions;
using System.Drawing;
using System.IO;

namespace HistogramAndBinarization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Orginal.LayoutTransform = new ScaleTransform(e.NewValue, e.NewValue);
        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AfterFiltering.LayoutTransform = new ScaleTransform(e.NewValue, e.NewValue);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                Orginal.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                AfterFiltering.Source = Orginal.Source;
                var histogramValues = BitmapConverter.CalculateHistogram(BitmapConverter.GetBitmapFromBitmapImage(Orginal.Source as BitmapImage));
                DrawHistogram(histogramValues);
            }
        }

        private void DrawHistogram(int[,] values)
        {
            SeriesCollection.Clear();
            SeriesCollection.Add(new LineSeries
            {
                Title = "R",
                Values = new ChartValues<int>(values.GetRow(0)),
                PointGeometry = null,
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.Red),
            }); 
            SeriesCollection.Add(new LineSeries
            {
                Title = "G",
                Values = new ChartValues<int>(values.GetRow(1)),
                PointGeometry = null,
                Fill = new SolidColorBrush(Colors.Green),
                Stroke = new SolidColorBrush(Colors.Green),
      
            }); 
            SeriesCollection.Add(new LineSeries
            {
                Title = "B",
                Values = new ChartValues<int>(values.GetRow(2)),
                PointGeometry = null,
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Blue),
            }); 
            SeriesCollection.Add(new LineSeries
            {
                Title = "GRAY",
                Values = new ChartValues<int>(values.GetRow(3)),
                PointGeometry = null,
                Fill = new SolidColorBrush(Colors.Gray),
                Stroke = new SolidColorBrush(Colors.Gray),
            });
            DataContext = this;
        }

        private void HistogramExtending(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = BitmapConverter.HistogramExtending(BitmapConverter.GetBitmapFromBitmapImage(Orginal.Source as BitmapImage));
            AfterFiltering.Source = ConvertBitmapToBitmapImage(bitmap);
            DrawHistogram(BitmapConverter.CalculateHistogram(bitmap));
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void HistogramEqualization(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = BitmapConverter.HistogramEqualization(BitmapConverter.GetBitmapFromBitmapImage(Orginal.Source as BitmapImage));
            AfterFiltering.Source = ConvertBitmapToBitmapImage(bitmap);
            DrawHistogram(BitmapConverter.CalculateHistogram(bitmap));
        }

        private void TreshholdByValue(object sender, RoutedEventArgs e)
        {
            byte value;
            if (!byte.TryParse(TreshholdValue.Text, out value))
            {
                MessageBox.Show("Niepoprawna wartość. Podaj wartość od 0 do 255");
                TreshholdValue.Text = "0";
                return;
            }
            Bitmap bitmap = BitmapConverter.ThresholdByValue(BitmapConverter.GetBitmapFromBitmapImage(Orginal.Source as BitmapImage),value);
            AfterFiltering.Source = ConvertBitmapToBitmapImage(bitmap);
            DrawHistogram(BitmapConverter.CalculateHistogram(bitmap));
        }

        private void TreshholdByPercent(object sender, RoutedEventArgs e)
        {
            byte value;
            if (!byte.TryParse(TreshholdPercent.Text, out value) || value > 100)
            {
                MessageBox.Show("Niepoprawna wartość. Podaj wartość od 0 do 100");
                TreshholdPercent.Text = "0";
                return;
            }
            Bitmap bitmap = BitmapConverter.ThresholdByPercent(BitmapConverter.GetBitmapFromBitmapImage(Orginal.Source as BitmapImage), value);
            AfterFiltering.Source = ConvertBitmapToBitmapImage(bitmap);
            DrawHistogram(BitmapConverter.CalculateHistogram(bitmap));
        }
    }
}
