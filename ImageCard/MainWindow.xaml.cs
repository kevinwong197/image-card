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
using System.Windows.Media.Animation;

using System.IO;
using System.Windows.Interop;
using System.Runtime.InteropServices;

// apng, gif, lock to bottom

namespace ImageCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool bFreeze;
        private double delta = 4;
        public MainWindow()
        {
            InitializeComponent();
            bFreeze = false;
            string[] args = Environment.GetCommandLineArgs();
            BitmapImage bitmap;
            Console.WriteLine("ok");
            if (args.Length >= 2)
            {
                Uri uri = new Uri(Path.GetFullPath(args[1]));
                bitmap = new BitmapImage(uri);
            }
            else
            {
                bitmap = new BitmapImage(new Uri(@"images\init.png", UriKind.Relative));
            }
            image.Source = bitmap;

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (bFreeze)
                return;
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }

            if (e.ChangedButton == MouseButton.Middle)
            {
                Point position = viewbox.PointToScreen(new Point(0d, 0d));
                Application.Current.MainWindow.Top = position.Y;
                Application.Current.MainWindow.Left = position.X;
                Application.Current.MainWindow.Height = viewbox.ActualHeight;
                Application.Current.MainWindow.Width = viewbox.ActualWidth;
            }
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Click_toggleFreeze(object sender, RoutedEventArgs e)
        {
            if (bFreeze)
            {
                bFreeze = false;
                Application.Current.MainWindow.ResizeMode = ResizeMode.CanResize;
                toggleFreeze.Header = "freeze";
            }
            else
            {
                bFreeze = true;
                Application.Current.MainWindow.ResizeMode = ResizeMode.NoResize;
                toggleFreeze.Header = "unfreeze";
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                Uri uri = new Uri(files[0]);
                BitmapImage bitmap = new BitmapImage(uri);
                image.Source = bitmap;

                Application.Current.MainWindow.Height = bitmap.Height;
                Application.Current.MainWindow.Width = bitmap.Width;

            }
        }

        private void image_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Height = viewbox.ActualHeight;
            Application.Current.MainWindow.Width = viewbox.ActualWidth;

        }


        private IntPtr _handle;
        private void SetBounds(int left, int top, int width, int height)
        {
            if (_handle == IntPtr.Zero)
                _handle = new WindowInteropHelper(this).Handle;

            SetWindowPos(_handle, IntPtr.Zero, left, top, width, height, 0);
        }

        [DllImport("user32")]
        static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double tmpT = 0;
            double tmpH = 0;
            double tmpL = 0;
            double tmpW = 0;
            if (e.Delta == 0)// should never occur
                return;
            if ((this.Top <= 0) && e.Delta > 0)
                return;

            tmpT = Application.Current.MainWindow.Top - e.Delta / 2 / delta;
            tmpH = viewbox.ActualHeight + e.Delta / delta;

            tmpL = Application.Current.MainWindow.Left + ((Application.Current.MainWindow.Width - (image.ActualWidth / image.ActualHeight) * tmpH) / 2);
            tmpW = (image.ActualWidth / image.ActualHeight) * tmpH;
            SetBounds((int)tmpL, (int)tmpT, (int)tmpW, (int)tmpH);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
