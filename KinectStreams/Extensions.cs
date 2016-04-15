using Microsoft.Kinect;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using LightBuzz.Vitruvius;

namespace KinectStreams
{
    public static class Extensions
    {
        #region Camera

        public static ImageSource ToBitmap(this ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];

                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this InfraredFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort[] frameData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(frameData);

            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++)
            {
                ushort ir = frameData[infraredIndex];

                byte intensity = (byte)(ir >> 7);

                pixels[colorIndex++] = (byte)(intensity / 1); // Blue
                pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                pixels[colorIndex++] = (byte)(intensity / 0.4); // Red

                colorIndex++;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        #endregion

        #region Body

        public static Joint ScaleTo(this Joint joint, double width, double height, float skeletonMaxX, float skeletonMaxY)
        {
            joint.Position = new CameraSpacePoint
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            return joint;
        }

        public static Joint ScaleTo(this Joint joint, double width, double height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }

        private static float Scale(double maxPixel, double maxSkeleton, float position)
        {
            float value = (float)((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));

            if (value > maxPixel)
            {
                return (float)maxPixel;
            }

            if (value < 0)
            {
                return 0;
            }

            return value;
        }

        #endregion

        #region Drawing

        public static void DrawSkeleton(this Canvas canvas, Body body)
        {
            if (body == null) return;

            foreach (Joint joint in body.Joints.Values)
            {
                canvas.DrawPoint(joint);
            }


            var head = body.Joints[JointType.Head];
            var point = head.Position.ToPoint(Visualization.Color);

            var neck = body.Joints[JointType.Neck];
            var spineShoulder = body.Joints[JointType.SpineShoulder];
            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];
            var spineMid = body.Joints[JointType.SpineMid];
            var elbowLeft = body.Joints[JointType.ElbowLeft];
            var elbowRight = body.Joints[JointType.ElbowRight];
            var wristLeft = body.Joints[JointType.WristLeft];
            var wristRight = body.Joints[JointType.WristRight];
            var handLeft = body.Joints[JointType.HandLeft];
            var handRight = body.Joints[JointType.HandRight];
            var handTipLeft = body.Joints[JointType.HandTipLeft];
            var handTipRight = body.Joints[JointType.HandTipRight];
            var thumbLeft = body.Joints[JointType.ThumbLeft];
            var thumbRight = body.Joints[JointType.ThumbRight];
            var spineBase = body.Joints[JointType.SpineBase];
            var hipLeft = body.Joints[JointType.HipLeft];
            var hipRight = body.Joints[JointType.HipRight];
            var kneeLeft = body.Joints[JointType.KneeLeft];
            var kneeRight = body.Joints[JointType.KneeRight];
            var ankleLeft = body.Joints[JointType.AnkleLeft];
            var ankleRight = body.Joints[JointType.AnkleRight];
            var footLeft = body.Joints[JointType.FootLeft];
            var footRight = body.Joints[JointType.FootRight];

            var angle = elbowLeft.Angle(shoulderLeft, wristLeft);
            //Debug.WriteLine("Angle " + angle);


            canvas.DrawLine(head, neck);
            canvas.DrawLine(neck, spineShoulder);
            canvas.DrawLine(spineShoulder, shoulderLeft);
            canvas.DrawLine(spineShoulder, shoulderRight);
            canvas.DrawLine(spineShoulder, spineMid);
            canvas.DrawLine(shoulderLeft, elbowLeft);
            canvas.DrawLine(shoulderRight, elbowRight);
            canvas.DrawLine(elbowLeft, wristLeft);
            canvas.DrawLine(elbowRight, wristRight);
            canvas.DrawLine(wristLeft, handLeft);
            canvas.DrawLine(wristRight, handRight);
            canvas.DrawLine(handLeft, handTipLeft);
            canvas.DrawLine(handRight, handTipRight);
            canvas.DrawLine(handTipLeft, thumbLeft);
            canvas.DrawLine(handTipRight, thumbRight);
            canvas.DrawLine(spineMid, spineBase);
            canvas.DrawLine(spineBase, hipLeft);
            canvas.DrawLine(spineBase, hipRight);
            canvas.DrawLine(hipLeft, kneeLeft);
            canvas.DrawLine(hipRight, kneeRight);
            canvas.DrawLine(kneeLeft, ankleLeft);
            canvas.DrawLine(kneeRight, ankleRight);
            canvas.DrawLine(ankleRight, footLeft);
            canvas.DrawLine(ankleRight, footRight);
        }

        public static void DrawPoint(this Canvas canvas, Joint joint)
        {
            if (joint.TrackingState == TrackingState.NotTracked) return;

            joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
        
            Ellipse ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.LightBlue)
            };

            //var point = joint.Position.ToPoint(Visualization.Color);

            Canvas.SetLeft(ellipse, joint.Position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Position.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second)
        {
            if (first.TrackingState == TrackingState.NotTracked || second.TrackingState == TrackingState.NotTracked) return;

            first = first.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            second = second.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            
            Line line = new Line
            {
                X1 = first.Position.X,
                Y1 = first.Position.Y,
                X2 = second.Position.X,
                Y2 = second.Position.Y,
                StrokeThickness = 8,
                Stroke = new SolidColorBrush(Colors.LightBlue)
            };

            canvas.Children.Add(line);
        }

        #endregion
    }
}
