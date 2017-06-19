using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SharpAdbClient;
using static SlideShowImageCapture.CanonCamera;
using static SlideShowImageCapture.AndroidCamera;


namespace SlideShowImageCapture
{
    public partial class SlideShowForm : Form
    {

        #region global

        // capture mode options
        // 1 - Canon camera
        // 2 - Android camera
        // 3 - Canon and Android camera simultaneously
        // 4 - Smartek camera
        int captureMode = 4;

        // global variables for controlling input and time period for image slide show
        //string[] images = Directory.GetFiles(@"D:\Playing Cards\", "*.png");
        string[] images = Directory.GetFiles(@"Y:\Projects\C# Projects\SlideShowCameraCapture\SlideShowImageCapture\bin\Debug\Playing Cards\", "*.png");
        

        int i = 1;
        int timerPeriod = 500;

        AndroidCamera androidCamera = new AndroidCamera();
        CanonCamera canonCamera = new CanonCamera();
        SmartekCamera smartekCamera = new SmartekCamera();


        ThreadStart cameraTS = new ThreadStart(CaptureImage);
        

        #endregion

        public SlideShowForm()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            try
            {
                images = images.OrderBy(x => x).ToArray();

                // initialize Windows Form
                initializeComponent();

                // set projection screen and windows form properties
                setScreenAndFormProperties();

                // initialize ADB or Canon PTP communication interfaces
                initializeCommunicationInterfaces();

            }
            catch (DllNotFoundException) { MessageBox.Show("Canon DLLs not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }


        private void initializeCommunicationInterfaces()
        {
            // depending on which capture mode has been selected, initialize interfaces
            switch (captureMode)
            {
                case 1:
                    canonCamera.connectCanonCamera();
                    break;

                case 2:
                    androidCamera.connectAndroidCamera();
                    break;

                case 3:
                    canonCamera.connectCanonCamera();
                    androidCamera.connectAndroidCamera();
                    break;

                case 4:
                    smartekCamera.connectSmartekCamera();
                    break;

                default:
                    MessageBox.Show("Non valid mode has been selected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }


        private void OnSlideshowStart(object sender, EventArgs e)
        {
            Thread cameraThread = new Thread(cameraTS);


            textBox1.Visible = false;
            // textbox containing current filename
            textBox2.Visible = false;


            ChangeSlide();
            CaptureImage();

            timer1.Interval = timerPeriod;
            timer1.Tick += new EventHandler(OnSlideShowTimer);
            timer1.Start();
        }


        private void OnSlideShowTimer(object sender, EventArgs e)
        {
            if (i < images.Length)
            {

                textBox2.Text = Path.GetFileName(images[i]);
                picBox.Image = Image.FromFile(images[i]);


                CaptureImage(canonCamera, androidCamera, smartekCamera, captureMode);

                
                i++;
            }
            else
            {
                Thread.Sleep(1000);

                if (captureMode == 1)
                {
                    canonData.CameraHandler.UILock(false);
                    canonData.CameraHandler.CloseSession();
                    canonData.CameraHandler.Dispose();
                }
                else
                if (captureMode == 2)
                {
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidCameraData.androidDeviceData, androidCameraData.receiver);
                }

                if (captureMode == 3)
                {
                    canonData.CameraHandler.UILock(false);
                    canonData.CameraHandler.CloseSession();
                    canonData.CameraHandler.Dispose();
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidCameraData.androidDeviceData, androidCameraData.receiver);
                }

                if (captureMode == 4)
                {
                    smcs.CameraSuite.ExitCameraAPI();
                }

                Thread.Sleep(1000);
                System.Windows.Forms.Application.Exit();
            }
        }


        public static void ChangeSlide()
        {
            textBox2.Text = Path.GetFileName(images.First());
            picBox.Image = Image.FromFile(images.First()); ;
        }


        public static void CaptureImage(CanonCamera canonCamera, AndroidCamera androidCamera, SmartekCamera smartekCamera, int captureMode)
        {

            if (captureMode == 1)
            {
                Thread.Sleep(1000);
                canonCamera.takePhoto();
            }

            else if (captureMode == 2)
            {
                androidCamera.takePhoto();
            }

            else if (captureMode == 3)
            {
                Thread.Sleep(1000);
                androidCamera.takePhoto();
                //canonCamera.takePhoto();
            }

            else if (captureMode == 4)
            {
                Thread.Sleep(1000);
                smartekCamera.takePhoto();
            }

        }


        static void OnProcessExit(object sender, EventArgs e)
        {

            //canonData.CameraHandler.UILock(false);
            //canonData.CameraHandler.CloseSession();
            //canonData.CameraHandler.Dispose();

            //AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidCameraData.androidDeviceData, androidCameraData.receiver);
            //AdbClient.Instance.ExecuteRemoteCommand("am force-stop com.oneplus.camera", androidCameraData.androidDeviceData, androidCameraData.receiver);

            //smcs.CameraSuite.ExitCameraAPI();

        }
    }
}