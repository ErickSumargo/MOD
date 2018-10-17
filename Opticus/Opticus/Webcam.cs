using System;
using System.Drawing;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading;

namespace Opticus
{
    class Webcam
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        private int frameRate;

        public bool cameraReady;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        public Image RF;

        private Size frameSize;

        private FilterInfoCollection videoDevices = null;

        private VideoCaptureDevice videoSource = null;

        /*----------------------------------------------------------------------------------------------------------*/

        public Webcam(Size framesize, int framerate)
        {
            cameraReady = false;

            RF = null;
            videoDevices = null;
            videoSource = null;

            frameSize = framesize;
            frameRate = framerate;
        }

        private FilterInfoCollection getCamList()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            return videoDevices;
        }

        public void Start()
        {
            if (getCamList().Count == 0)
            {
                throw new Exception("Video device not found");
            }

            else
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

                videoSource.Start();
            }
        }

        private bool imageconvertcallback()
        {
            return false;
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            eventArgs.Frame.RotateFlip(RotateFlipType.Rotate180FlipY);

            RF = (Bitmap)eventArgs.Frame.GetThumbnailImage(frameSize.Width, frameSize.Height, new Image.GetThumbnailImageAbort(imageconvertcallback), IntPtr.Zero);

            cameraReady = true;

            Thread.Sleep((int)1000.0 / frameRate);
        }

        public void Stop()
        {
            if (!(videoSource == null))
            {
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();

                    videoSource = null;
                }
            }
        }
    }
}