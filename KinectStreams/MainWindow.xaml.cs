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

namespace KinectStreams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        int STATE = 0;

        Mode _mode = Mode.Color;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        bool _displayBody = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
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
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
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
                        exercise_1(closest_body);
                    }


                    /*
                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);
                                }
                            }
                        }
                    }
                    */

                }
            }
        }

        private void exercise_1(Body body)
        {
            switch(STATE)
            {
                case 0:
                    if (check_state_1_0(body) == "success")
                    {
                        STATE = 1;
                        Debug.WriteLine("Exercise started");
                    }

                    break;

                case 1:
                    if (check_state_1_1(body) == "success")
                    {
                        STATE = 2;
                        Debug.WriteLine("State 1 complete");
                    }
                    break;

                case 2:
                    if(check_state_1_2(body) == "success")
                    {
                        STATE = 3;
                        Debug.WriteLine("State 2 complete");
                    }
                    break;

                case 3:
                    if (check_state_1_3(body) == "success")
                    {
                        STATE = 4;
                        Debug.WriteLine("State 3 complete");
                    }
                    break;

                case 4:
                    if (check_state_1_2(body) == "success")
                    {
                        STATE = 5;
                        Debug.WriteLine("State 4 complete");
                    }
                    break;

                case 5:
                    if (check_state_1_1(body) == "success")
                    {
                        STATE = 6;
                        Debug.WriteLine("Exercise complete");
                    }
                    break;

            }
        }

        private String check_state_1_0(Body body)
        {
            var wristLeft = body.Joints[JointType.WristLeft];
            var wristRight = body.Joints[JointType.WristRight];

            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];

            if(wristLeft.Position.X < shoulderLeft.Position.X && wristRight.Position.X > shoulderRight.Position.X)
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
                if(almost180(elbowLeft.Angle(shoulderLeft, wristLeft)) && almost180(elbowRight.Angle(shoulderRight, wristRight)))
                {
                    if(almost90(shoulderLeft.Angle(spineShoulder, elbowLeft)) && almost90(shoulderRight.Angle(spineShoulder, elbowRight)))
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

            if(almost180(shoulderRight.Angle(spineShoulder, elbowRight)) && almost180(shoulderLeft.Angle(spineShoulder, elbowLeft)))
            {
                if(almost180(elbowRight.Angle(shoulderRight, wristRight)) && almost180(elbowLeft.Angle(shoulderLeft, wristLeft)))
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


        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;
        }

        #endregion
    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}
