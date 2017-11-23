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

namespace TPDotPuzzle
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectDevice;
        private Skeleton[] frameSkeletons = new Skeleton[6];
        DotPuzzle puzzle = new DotPuzzle();        private int puzzleDotIndex;
        public MainWindow()
        {
            InitializeComponent();

            this.puzzle.Dots.Add(new Point(200, 300));
            this.puzzle.Dots.Add(new Point(1600, 300));
            this.puzzle.Dots.Add(new Point(1650, 400));
            this.puzzle.Dots.Add(new Point(1600, 500));
            this.puzzle.Dots.Add(new Point(1000, 500));
            this.puzzle.Dots.Add(new Point(1000, 600));
            this.puzzle.Dots.Add(new Point(1200, 700));
            this.puzzle.Dots.Add(new Point(1150, 800));
            this.puzzle.Dots.Add(new Point(750, 800));
            this.puzzle.Dots.Add(new Point(700, 700));
            this.puzzle.Dots.Add(new Point(900, 600));
            this.puzzle.Dots.Add(new Point(900, 500));
            this.puzzle.Dots.Add(new Point(200, 500));
            this.puzzle.Dots.Add(new Point(150, 400));
            this.puzzleDotIndex = -1;
            this.Loaded += (s, e) =>
            {
                this.kinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                this.kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
                this.kinectDevice.SkeletonStream.Enable();
                this.kinectDevice.Start();
                DrawPuzzle(this.puzzle);
            };
        }

        private void DrawPuzzle(DotPuzzle puzzle)
        {
            PuzzleBoardElement.Children.Clear();
            if (puzzle != null)
            {
                for (int i = 0; i < puzzle.Dots.Count; i++)
                {
                    Grid dotContainer = new Grid();
                    dotContainer.Width = 50;
                    dotContainer.Height = 50;
                    dotContainer.Children.Add(new Ellipse { Fill = Brushes.Gray });
                    TextBlock dotLabel = new TextBlock();
                    dotLabel.Text = (i + 1).ToString();
                    dotLabel.Foreground = Brushes.White;
                    dotLabel.FontSize = 24;
                    dotLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    dotLabel.VerticalAlignment = VerticalAlignment.Center;
                    dotContainer.Children.Add(dotLabel);
                    Canvas.SetTop(dotContainer, puzzle.Dots[i].Y - (dotContainer.Height / 2));
                    Canvas.SetLeft(dotContainer, puzzle.Dots[i].X - (dotContainer.Width / 2));
                    PuzzleBoardElement.Children.Add(dotContainer);
                }
            }
        }        private void TrackPuzzle(SkeletonPoint position)
        {
            if (this.puzzleDotIndex == this.puzzle.Dots.Count)
            {
            }
            else
            {
                Point dot;
                if (this.puzzleDotIndex + 1 < this.puzzle.Dots.Count)
                {
                    dot = this.puzzle.Dots[this.puzzleDotIndex + 1];
                }
                else
                {
                    dot = this.puzzle.Dots[0];
                }
                DepthImagePoint point = this.kinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(position,
               this.kinectDevice.DepthStream.Format);
                //DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(position,
                point.X = (int)(point.X * LayoutRoot.ActualWidth / kinectDevice.DepthStream.FrameWidth);
                point.Y = (int)(point.Y * LayoutRoot.ActualHeight / kinectDevice.DepthStream.FrameHeight);
                Point handPoint = new Point(point.X, point.Y);
                Point dotDiff = new Point(dot.X - handPoint.X, dot.Y - handPoint.Y);
                double length = Math.Sqrt(dotDiff.X * dotDiff.X + dotDiff.Y * dotDiff.Y);
                int lastPoint = this.CrayonElement.Points.Count - 1;
                if (length < 25)
                {
                    if (lastPoint > 0)
                    {
                        this.CrayonElement.Points.RemoveAt(lastPoint);
                    }
                    this.CrayonElement.Points.Add(new Point(dot.X, dot.Y));
                    this.CrayonElement.Points.Add(new Point(dot.X, dot.Y));
                    this.puzzleDotIndex++;
                    if (this.puzzleDotIndex == this.puzzle.Dots.Count)
                    {
                    }
                }
                else
                {
                    if (lastPoint > 0)
                    {

                        Point lineEndpoint = this.CrayonElement.Points[lastPoint];
                        this.CrayonElement.Points.RemoveAt(lastPoint);
                        lineEndpoint.X = handPoint.X;
                        lineEndpoint.Y = handPoint.Y;
                        this.CrayonElement.Points.Add(lineEndpoint);
                    }
                }
            }
        }        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(this.frameSkeletons);
                    Skeleton skeleton = GetPrimarySkeleton(this.frameSkeletons);
                    Skeleton[] dataSet2 = new Skeleton[this.frameSkeletons.Length];
                    frame.CopySkeletonDataTo(dataSet2);
                    if (skeleton == null)
                    {
                        HandCursorElement.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Joint primaryHand = GetPrimaryHand(skeleton);
                        TrackHand(primaryHand);
                        TrackPuzzle(primaryHand.Position);
                    }
                }
            }
        }        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;
            if (skeletons != null)
            {
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }
            return skeleton;
        }        private static Joint GetPrimaryHand(Skeleton skeleton)
        {
            Joint primaryHand = new Joint();
            if (skeleton != null)
            {
                primaryHand = skeleton.Joints[JointType.HandLeft];
                Joint righHand = skeleton.Joints[JointType.HandRight];
                if (righHand.TrackingState != JointTrackingState.NotTracked)
                {
                    if (primaryHand.TrackingState == JointTrackingState.NotTracked)
                    {
                        primaryHand = righHand;
                    }
                    else
                    {
                        if (primaryHand.Position.Z > righHand.Position.Z)
                        {
                            primaryHand = righHand;
                        }
                    }
                }
            }
            return primaryHand;
        }        private void TrackHand(Joint hand)
        {
            if (hand.TrackingState == JointTrackingState.NotTracked)
            {
                HandCursorElement.Visibility = Visibility.Collapsed;
            }
            else
            {
                HandCursorElement.Visibility = Visibility.Visible;
                DepthImagePoint point =
               this.kinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(hand.Position,
               this.kinectDevice.DepthStream.Format);
                point.X = (int)((point.X * LayoutRoot.ActualWidth / kinectDevice.DepthStream.FrameWidth) -
               (HandCursorElement.ActualWidth / 2.0));
                point.Y = (int)((point.Y * LayoutRoot.ActualHeight / kinectDevice.DepthStream.FrameHeight) -
               (HandCursorElement.ActualHeight / 2.0));
                Canvas.SetLeft(HandCursorElement, point.X);
                Canvas.SetTop(HandCursorElement, point.Y);
                if (hand.JointType == JointType.HandRight)
                {
                    HandCursorScale.ScaleX = 1;
                }
                else
                {
                    HandCursorScale.ScaleX = -1;
                }
            }
        }
    }
}
