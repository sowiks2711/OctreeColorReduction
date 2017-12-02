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
        public MainWindow()
        {
            InitializeComponent();
            var bmpOryg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/colorful-tree.jpg"));
            //var bmpOryg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/sky.jpg"));
            //var bmpOryg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/car.jpg"));
            OriginalBmp.Source = bmpOryg;
            reduceAfterConst.Source = bmpOryg;
            WriteableBitmap wbmp = new WriteableBitmap(bmpOryg);
            //for (int i = 0; i < wbmp.PixelHeight; i++)
            //{
            //    for (int j = 0; j < wbmp.PixelWidth; j++)
            //    {
            //        var c = wbmp.GetPixel(j, i);
            //        c.A = 0;
            //        wbmp.SetPixel(j, i, c );

            //    }

            //}
            reduceAlongConst.Source = new ColorReducer().ReduceColors(wbmp,50);
        }
    
    
    }
}
