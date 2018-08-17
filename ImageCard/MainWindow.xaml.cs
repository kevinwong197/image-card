using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using System.IO;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace ImageCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _bFreeze;
        private const double Delta = 4;

        public MainWindow()
        {
            InitializeComponent();
            _bFreeze = false;
            string[] args = Environment.GetCommandLineArgs();
            BitmapImage bitmap;
            if (args.Length >= 2)
            {
                Uri uri = new Uri(Path.GetFullPath(args[1]));
                bitmap = new BitmapImage(uri);
                image.Source = bitmap;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_bFreeze)
                return;
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
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
            if (_bFreeze)
            {
                _bFreeze = false;
                Application.Current.MainWindow.ResizeMode = ResizeMode.CanResize;
                toggleFreeze.Header = "freeze";
            }
            else
            {
                _bFreeze = true;
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

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Height = viewbox.ActualHeight;
            Application.Current.MainWindow.Width = viewbox.ActualWidth;
        }

        private IntPtr _handle;
        private void SetBounds(int left, int top, int width, int height)
        {
            if (_handle == IntPtr.Zero)
                _handle = new WindowInteropHelper(this).Handle;
            IntPtr d = BeginDeferWindowPos(1);
            DeferWindowPos(d, _handle, IntPtr.Zero, left, top, width, height, 0x0008 | 0x0004 | 0x0010);
            EndDeferWindowPos(d);
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double tmpT = 0;
            double tmpH = 0;
            double tmpL = 0;
            double tmpW = 0;
            if (e.Delta == 0)
                return;
            if ((this.Top <= 0) && e.Delta > 0)
                return;

            double ct = Application.Current.MainWindow.Top;
            double cw = Application.Current.MainWindow.Width;
            double cl = Application.Current.MainWindow.Left;
            tmpT = ct - e.Delta / 2 / Delta;
            tmpH = viewbox.ActualHeight + e.Delta / Delta;

            tmpL = cl + ((cw - (image.ActualWidth / image.ActualHeight) * tmpH) / 2);
            tmpW = (image.ActualWidth / image.ActualHeight) * tmpH;
            SetBounds((int)tmpL, (int)tmpT, (int)tmpW, (int)tmpH);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        [DllImport("user32")]
        private static extern IntPtr DeferWindowPos(
            IntPtr hWinPosInfo,
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);
        [DllImport("user32")]
        private static extern IntPtr BeginDeferWindowPos(
            int nNumWindows);
        [DllImport("user32")]
        private static extern IntPtr EndDeferWindowPos(
            IntPtr hWinPosInfo);
    }
}
