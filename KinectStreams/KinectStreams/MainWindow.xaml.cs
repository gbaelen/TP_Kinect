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
using Microsoft.Kinect;

namespace KinectStreams
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor;

        private Mode _mode;

        private Skeleton[] _bodies = new Skeleton[6];
        private static int width;
        private static int height;

        public enum Mode
        {
            Color,
            Depth,
            Infrared
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (_sensor != null)
            {
                _sensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);
                _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _sensor.SkeletonStream.Enable();

                _sensor.AllFramesReady += Sensor_AllFrameReady;

                _sensor.Start();
            }
        }

        private void Sensor_AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            //ColorImageFrame
            ColorImageFrame colorImageFrame = e.OpenColorImageFrame();
            if (colorImageFrame == null)
                return;
            byte[] colorDataArray = new byte[colorImageFrame.PixelDataLength];
            colorImageFrame.CopyPixelDataTo(colorDataArray);
            //ColorImageFrame
            colorImageFrame.Dispose();
            //DepthImageFrame
            DepthImageFrame depthImageFrame = e.OpenDepthImageFrame();
            if (depthImageFrame == null)
                return;
            //(PixelDataLength = 640*480)
            DepthImagePixel[] depthImgPixArray = new DepthImagePixel[depthImageFrame.PixelDataLength];
            depthImageFrame.CopyDepthImagePixelDataTo(depthImgPixArray);
            //ColorImageFrame
            depthImageFrame.Dispose();
            byte[] userColorArray = new byte[_sensor.ColorStream.FramePixelDataLength];
            ColorImagePoint[] colImgPntArray
            = new ColorImagePoint[_sensor.DepthStream.FrameHeight *
           _sensor.DepthStream.FrameWidth];
            //colorImagePoints
            _sensor.CoordinateMapper.MapDepthFrameToColorFrame(depthImageFrame.Format,
           depthImgPixArray, colorImageFrame.Format, colImgPntArray);
            for (int i = 0; i < depthImgPixArray.Length; i++)
            {
                if (depthImgPixArray[i].PlayerIndex == 0)
                    continue;
                ColorImagePoint colorImagePoint = colImgPntArray[i];
                if (colorImagePoint.X >= _sensor.ColorStream.FrameWidth || colorImagePoint.X < 0
                || colorImagePoint.Y >= _sensor.ColorStream.FrameHeight || colorImagePoint.Y < 0)
                    continue;
                int colorDataIndex =
                ((colorImagePoint.Y * _sensor.ColorStream.FrameWidth) + colorImagePoint.X)
               * _sensor.ColorStream.FrameBytesPerPixel;
                userColorArray[colorDataIndex] = colorDataArray[colorDataIndex];
                userColorArray[colorDataIndex + 1] = colorDataArray[colorDataIndex + 1];
                userColorArray[colorDataIndex + 2] = colorDataArray[colorDataIndex + 2];
                userColorArray[colorDataIndex + 3] = 255;
                userColorArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel]
                = colorDataArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel];
                userColorArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel + 1]
                = colorDataArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel + 1];
                userColorArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel + 2]
                = colorDataArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel + 2];
                userColorArray[colorDataIndex - _sensor.ColorStream.FrameBytesPerPixel + 3] = 255;
            }
            BitmapSource bitmapSource = BitmapSource.Create(
            _sensor.ColorStream.FrameWidth,
           _sensor.ColorStream.FrameHeight,
           96,//dpi
           96,//dpi
           PixelFormats.Bgra32,//
           null,
           userColorArray,
            _sensor.ColorStream.FrameWidth *
           _sensor.ColorStream.FrameBytesPerPixel);
            image1.Source = bitmapSource;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }
        }


        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }
        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }
        private void Skeleton_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
