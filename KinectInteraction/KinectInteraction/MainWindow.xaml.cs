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
using Microsoft.Kinect.Toolkit;

namespace KinectInteraction
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Microsoft.Kinect.Toolkit.KinectSensorChooser sensorChooser;
 
        public enum KinectStatus
        {
            Undefined,
            Disconnected,
            Connected,
            Initializing,
            Error,
            NotPowered,
            NotReady,
            DeviceNotGenuine,
            DeviceNotSupported,
            InsufficientBandwith,
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.sensorChooser = new Microsoft.Kinect.Toolkit.KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUI.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };

            BindingOperations.SetBinding(this.kinectRegion, Microsoft.Kinect.Toolkit.Controls.KinectRegion.KinectSensorProperty, regionSensorBinding);
            GetTileButtonsForStackPanel();
        }

        private void GetTileButtonsForStackPanel()
        {
            for (int i=1; i<=35; i++)
            {
                Microsoft.Kinect.Toolkit.Controls.KinectCircleButton ObjTileButton = new Microsoft.Kinect.Toolkit.Controls.KinectCircleButton();
                ObjTileButton.Height = 250;
                ObjTileButton.Label = i;
                int j = i;
                ObjTileButton.Click += (o, e) => MessageBox.Show("You clicked button #" + j);
                StackPanelWithButton.Children.Add(ObjTileButton);
            }
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs e)
        {
            bool error = false;
            if (e.OldSensor != null)
            {
                try
                {
                    e.OldSensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Default;
                    e.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    e.OldSensor.DepthStream.Disable();
                    e.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }

            if (e.NewSensor != null)
            {
                try
                {
                    e.NewSensor.DepthStream.Enable(Microsoft.Kinect.DepthImageFormat.Resolution640x480Fps30);
                    e.NewSensor.SkeletonStream.Enable();
                    try
                    {
                        e.NewSensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Near;
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        e.NewSensor.SkeletonStream.TrackingMode = Microsoft.Kinect.SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        e.NewSensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Default;
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }

            if (!error)
            {
                kinectRegion.KinectSensor = e.NewSensor;
            }
        }

        private void ClickMe_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
