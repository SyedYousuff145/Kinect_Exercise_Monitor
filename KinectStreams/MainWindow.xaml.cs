using Microsoft.Kinect;
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LightBuzz.Vitruvius;
using System.Windows.Threading;

namespace KinectStreams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        int STATE = 0;
        int Exercise = 1;
        int num_of_exercises = 2;

        int Started = 0;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        bool _displayBody = false;
        DispatcherTimer timer = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            
            mePlayer.MediaEnded += MePlayer_MediaEnded;
            timer = new DispatcherTimer();

        }

        private void MePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            mePlayer.Position = new System.TimeSpan(0);            
        }

        #endregion

       

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (Started == 0)
            {
                switch(Exercise)
                {
                    case 1:
                        mePlayer.Source = new System.Uri("file:///D:/Work/Projects/Kinect/KinectStreams/1.mp4");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(2);
                        timer.Tick += timer_Tick;
                        timer.Start();
                        break;
                    case 2:
                        mePlayer.Source = new System.Uri("file:///D:/Work/Projects/Kinect/KinectStreams/2.mp4");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(2);
                        timer.Tick += timer_Tick;
                        timer.Start();
                        break;
                }
                
                Started = 1;
                
            }

            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();

                }
            }

           

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    Body closest_body = null;
                    float min = 100000;
                
                    foreach(var body in _bodies)
                    {
                        if(body != null)
                        {
                            if (body.IsTracked)
                            {
                                if (body.Joints[JointType.Head].Position.Z < min)
                                {
                                    closest_body = body;
                                    min = body.Joints[JointType.Head].Position.Z;
                                    //Debug.WriteLine(body.Joints[JointType.Head].Position.Z);
                                }
                            }
                        }
                    }

                    if (closest_body != null)
                    {
                        canvas.DrawSkeleton(closest_body);
                        switch(Exercise)
                        {
                            case 1: exercise_1(closest_body); break;
                            case 2: exercise_1(closest_body); break;
                        }
                        
                    }

                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (mePlayer.Source != null)
            {
                mePlayer.Pause();
            }
        }

        #endregion

        private void exercise_1(Body body)
        {
            switch (STATE)
            {
                case 0:

                    if (check_state_1_0(body) == "success")
                    {
                        STATE = 1;
                        Debug.WriteLine("Exercise started");

                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }

                    break;

                case 1:
                    if (check_state_1_1(body) == "success")
                    {
                        STATE = 2;
                        Debug.WriteLine("State 1 complete");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                    break;

                case 2:
                    if (check_state_1_2(body) == "success")
                    {
                        STATE = 3;
                        Debug.WriteLine("State 2 complete");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                    break;

                case 3:
                    if (check_state_1_3(body) == "success")
                    {
                        STATE = 4;
                        Debug.WriteLine("State 3 complete");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                    break;

                case 4:
                    if (check_state_1_2(body) == "success")
                    {
                        STATE = 5;
                        Debug.WriteLine("State 4 complete");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                    break;

                case 5:
                    if (check_state_1_1(body) == "success")
                    {
                        STATE = 6;
                        Debug.WriteLine("Exercise complete");
                        mePlayer.Play();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                    break;

            }
        }

        private void exercise_2(Body body)
        {

        }

        private String check_state_1_0(Body body)
        {
            var wristLeft = body.Joints[JointType.WristLeft];
            var wristRight = body.Joints[JointType.WristRight];

            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];

            if (wristLeft.Position.X < shoulderLeft.Position.X && wristRight.Position.X > shoulderRight.Position.X)
            {
                if (wristLeft.Position.Y > shoulderLeft.Position.Y && wristRight.Position.Y > shoulderRight.Position.Y)
                    return "success";
            }

            return "failure";
        }

        private String check_state_1_1(Body body)
        {
            //Standing erect: Head, shoulderleft and shoulderright, spinemid in same plane
            //Hands and legs straight: Angles shoulder-elbow-wrist, hip-knee-ankle 180 degrees
            //Hands dropped down: Angles spineshoulder, shoulder, elbow 90 degrees and wrist below shoulder

            var head = body.Joints[JointType.Head];
            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];
            var spineMid = body.Joints[JointType.SpineMid];
            var spineShoulder = body.Joints[JointType.SpineShoulder];
            var elbowLeft = body.Joints[JointType.ElbowLeft];
            var elbowRight = body.Joints[JointType.ElbowRight];
            var wristLeft = body.Joints[JointType.WristLeft];
            var wristRight = body.Joints[JointType.WristRight];
            var hipLeft = body.Joints[JointType.HipLeft];
            var hipRight = body.Joints[JointType.HipRight];
            var kneeLeft = body.Joints[JointType.KneeLeft];
            var kneeRight = body.Joints[JointType.KneeRight];
            var ankleLeft = body.Joints[JointType.AnkleLeft];
            var ankleRight = body.Joints[JointType.AnkleRight];


            if (almostSame_dist(head.Position.Z, shoulderLeft.Position.Z) && almostSame_dist(shoulderLeft.Position.Z, shoulderRight.Position.Z) && almostSame_dist(head.Position.Z, spineMid.Position.Z))
            {
                if (almost180(elbowLeft.Angle(shoulderLeft, wristLeft)) && almost180(elbowRight.Angle(shoulderRight, wristRight)))
                {
                    if (almost90(shoulderLeft.Angle(spineShoulder, elbowLeft)) && almost90(shoulderRight.Angle(spineShoulder, elbowRight)))
                    {
                        //if(wristLeft.Position.Y < shoulderLeft.Position.Y && wristRight.Position.Y > shoulderRight.Position.Y)
                        return "success";
                    }

                }
            }


            return "failure";
        }

        private String check_state_1_2(Body body)
        {
            //Hands straight: angle shoulder-elbow-wrist 180 degrees.
            //Hands up: angle spinemid-shoulder-elbow 90 degrees and wrist above shoulder.

            var spineShoulder = body.Joints[JointType.SpineShoulder];
            var elbowLeft = body.Joints[JointType.ElbowLeft];
            var elbowRight = body.Joints[JointType.ElbowRight];
            var wristLeft = body.Joints[JointType.WristLeft];
            var wristRight = body.Joints[JointType.WristRight];
            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];

            if (almost180(shoulderRight.Angle(spineShoulder, elbowRight)) && almost180(shoulderLeft.Angle(spineShoulder, elbowLeft)))
            {
                if (almost180(elbowRight.Angle(shoulderRight, wristRight)) && almost180(elbowLeft.Angle(shoulderLeft, wristLeft)))
                {
                    //if (wristLeft.Position.Y > shoulderLeft.Position.Y && wristRight.Position.Y > shoulderRight.Position.Y)
                    return "success";
                }
            }

            return "failure";

        }

        private String check_state_1_3(Body body)
        {
            //Hands straight: angle shoulder-elbow-wrist 180 degrees.
            //Hands up: angle spinemid-shoulder-elbow 90 degrees and wrist above shoulder.

            var spineShoulder = body.Joints[JointType.SpineShoulder];
            var elbowLeft = body.Joints[JointType.ElbowLeft];
            var elbowRight = body.Joints[JointType.ElbowRight];
            var wristLeft = body.Joints[JointType.WristLeft];
            var wristRight = body.Joints[JointType.WristRight];
            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];

            if (almost90(shoulderRight.Angle(spineShoulder, elbowRight)) && almost90(shoulderLeft.Angle(spineShoulder, elbowLeft)))
            {
                if (almost180(elbowRight.Angle(shoulderRight, wristRight)) && almost180(elbowLeft.Angle(shoulderLeft, wristLeft)))
                {
                    if (wristLeft.Position.Y > shoulderLeft.Position.Y && wristRight.Position.Y > shoulderRight.Position.Y)
                        return "success";
                }
            }

            return "failure";

        }

        private bool almostSame_dist(float a, float b)
        {
            //Debug.WriteLine(Math.Abs(a - b));

            if (Math.Abs(a - b) > 0.05)
                return false;
            else
                return true;
        }

        private bool almost180(double a)
        {
            if (Math.Abs(a - 180) < 30)
                return true;
            else
                return false;
        }

        private bool almost90(double a)
        {
            if (Math.Abs(a - 100) < 30)
                return true;
            else
                return false;
        }


        #region Click Handlers

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
           
            STATE = 0;
            Started = 0;
            Exercise--;
            if(Exercise < 1)
            {
                Exercise = 1;
            }
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Started = 0;
            STATE = 0;

            //Restart the video
            mePlayer.Position = new System.TimeSpan(0);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            STATE = 0;
            Started = 0;

            //Go to next exercise.
            Exercise++;
            if(Exercise > num_of_exercises)
            {
                Exercise = num_of_exercises;
            }
        }

        #endregion
    }
}
