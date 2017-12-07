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

namespace octree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage bmp;
        private bool isFirstFinished = false;
        private bool isSecondFinished = false;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/colorful-tree.jpg"));
                //bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/sky.jpg"));
                //sourceUri = new Uri(@"pack://application:,,,/Resources/car.jpg");
                //bmp = new BitmapImage(sourceUri);
                OriginalBmp.Source = bmp;
                WriteableBitmap wbmp = new WriteableBitmap(bmp);
                //ColorsCount.Maximum = countColors(wbmp);
            } catch
            {

            }
        }

        private double countColors(WriteableBitmap wbmp)
        {
            HashSet<Color> colorsSet = new HashSet<Color>();
            for(int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    colorsSet.Add(wbmp.GetPixel(j, i));
                }
            }
            return colorsSet.Count;
        }


        private async void ReduceButtonClickAsync(object sender, RoutedEventArgs e)
        {
            var progress1 = new Progress<int>(value => FirstImagePB.Value = value);
            int reducedColorsNr = (int)ColorsCount.Value;
            ReduceToNR.IsEnabled = false;
            WriteableBitmap copy = new WriteableBitmap(bmp);
            isFirstFinished = false;
            isSecondFinished = false;
            copy.Freeze();
            Task.Run(() =>
            {
                WriteableBitmap res1 = new ColorReducer().ReduceColorsAfterConst(copy, reducedColorsNr, progress1);
                res1.Freeze();
                this.Dispatcher.Invoke(() => reduceAfterConst.Source = res1 );
                this.Dispatcher.Invoke(() => { isFirstFinished = true; TryToEnableButton(); } );

            });
            var progress2 = new Progress<int>(value => SecondImagePB.Value = value);

            await Task.Run(() =>
            {
                WriteableBitmap res2 = new ColorReducer().ReduceColorsAlongConst(copy, reducedColorsNr, progress2);
                res2.Freeze();
                this.Dispatcher.Invoke(() => reduceAlongConst.Source = res2 );
                this.Dispatcher.Invoke(() => { isSecondFinished = true; TryToEnableButton(); } );
            });

        }

        private void TryToEnableButton()
        {
            if (isFirstFinished && isSecondFinished)
                ReduceToNR.IsEnabled = true;
        }
    }
}
