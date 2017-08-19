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
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

namespace KinectEmergencySensor
{
    /// <summary>
    /// Interaction logic for SmpleWindow.xaml
    /// </summary>
    public partial class SimpleWindow : Window
    {
        /// <summary> KinectBodyView object which handles drawing the Kinect bodies to a View box in the UI </summary>
        private KinectBodyView kinectBodyView = null;
        Gesture swipeForwardGesture;
        KinectSensor sensor;
        Body[] bodies;
        BodyFrameReader bodyReader;
        VisualGestureBuilderFrameSource gestureSource;
        VisualGestureBuilderFrameReader gestureReader;

        public SimpleWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;
        }

        void OnLoadGestureFromDb(object sender, RoutedEventArgs e)
        {
            // we assume that this file exists and will load
            VisualGestureBuilderDatabase db = new VisualGestureBuilderDatabase(
              @"Gestures\Punch.gbd");

            // we assume that this gesture is in that database (it should be, it's the only
            // gesture in there).
            this.swipeForwardGesture = db.AvailableGestures.Where(g => g.Name == "Punch1").Single();
        }

        void OnOpenSensor(object sender, RoutedEventArgs e)
        {
            // assuming that the sensor is available and that it's not already open - 
            // i.e. assuming that the "user" will only press the buttons in a sensible
            // order which is not perhaps the best idea but then the user is primarily
            // me.
            this.sensor = KinectSensor.GetDefault();
            this.sensor.Open();
        }
        void OnCloseSensor(object sender, RoutedEventArgs e)
        {
            this.sensor.Close();
            this.sensor = null;
        }

        void OnOpenReaders(object sender, RoutedEventArgs e)
        {
            this.OpenBodyReader();
            this.OpenGestureReader();
        }
        void OpenBodyReader()
        {
            if (this.bodies == null)
            {
                this.bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
            }
            this.bodyReader = this.sensor.BodyFrameSource.OpenReader();
            this.bodyReader.FrameArrived += OnBodyFrameArrived;
        }

        void OpenGestureReader()
        {
            this.gestureSource = new VisualGestureBuilderFrameSource(this.sensor, 0);

            this.gestureSource.AddGesture(this.swipeForwardGesture);

            this.gestureSource.TrackingIdLost += OnTrackingIdLost;

            this.gestureReader = this.gestureSource.OpenReader();
            this.gestureReader.IsPaused = true;
            this.gestureReader.FrameArrived += OnGestureFrameArrived;
        }

        void OnCloseReaders(object sender, RoutedEventArgs e)
        {
            if (this.gestureReader != null)
            {
                this.gestureReader.FrameArrived -= this.OnGestureFrameArrived;
                this.gestureReader.Dispose();
                this.gestureReader = null;
            }
            if (this.gestureSource != null)
            {
                this.gestureSource.TrackingIdLost -= this.OnTrackingIdLost;
                this.gestureSource.Dispose();
            }
            this.bodyReader.Dispose();
            this.bodyReader = null;
        }
        void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            this.gestureReader.IsPaused = true;
            this.txtProgress.Text = string.Empty;
        }

        void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(this.bodies);

                    var trackedBody = this.bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (trackedBody != null)
                    {
                        if (this.gestureReader.IsPaused)
                        {
                            this.gestureSource.TrackingId = trackedBody.TrackingId;
                            this.gestureReader.IsPaused = false;
                        }
                    }
                    else
                    {
                        this.OnTrackingIdLost(null, null);
                    }
                }
            }
        }

        void OnGestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var continuousResults = frame.ContinuousGestureResults;

                    if ((continuousResults != null) &&
                      (continuousResults.ContainsKey(this.swipeForwardGesture)))
                    {
                        var result = continuousResults[this.swipeForwardGesture];

                        // change the gradient stop on the screen so that it reflects where
                        // we are in terms of progress of the gesture.
                        this.txtProgress.Text = Math.Max(result.Progress, 0.0f).ToString("G1");
                    }
                }
            }
        }
    }
}
