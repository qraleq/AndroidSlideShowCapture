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
using static SlideShowImageCapture.SmartekCamera;
using EDSDKLib;

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
        //string[] images = Directory.GetFiles(@"Y:\Misc\Playing Cards - PNG\", "*.png");



        //string[] images = Directory.GetFiles(@"Y:\Projects\C# Projects\SlideShowCameraCapture\Test Images\", "*.png");
        string[] images = Directory.GetFiles(@"Y:\Projects\MATLAB Projects\Structured Light Compressive Sensing\data\1920x1080 Patterns\Complete Measurement Process\Gray\", "*.png");
        //string[] images = Directory.GetFiles(@"Y:\Projects\MATLAB Projects\Structured Light Compressive Sensing\data\1920x1080 Patterns\Measurement Masks With Frame\Hadamard\Upscaled\Measurement Masks\", "*.png");
        //string[] images = Directory.GetFiles(@"Y:\Projects\MATLAB Projects\Structured Light Compressive Sensing\data\1920x1080 Patterns\Measurement Masks With Frame\Hadamard\Upscaled\Misc\", "*.png");



        int i = 0;
        //int timerPeriod = 300;

        AndroidCamera androidCamera = new AndroidCamera();
        CanonCamera canonCamera = new CanonCamera();
        SmartekCamera smartekCamera = new SmartekCamera();

        #endregion

        public SlideShowForm()
        {
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


        public void OnSlideShowChange(object sender, EventArgs e)
        {

            textBox1.Visible = false;
            textBox2.Visible = false;

            //Thread.Sleep(150);

            if (i < images.Length)
            {
                //textBox2.Text = Path.GetFileName(images[i]);

                picBox.Image = Image.FromFile(images[i]);
                picBox.Refresh();

                Thread.Sleep(300);

                switch (captureMode)
                {
                    case 1:
                        canonCamera.takePhoto();
                        Thread.Sleep(500);

                        if (i == 0)
                        {
                            timer1.Interval = 4000;
                            timer1.Tick += new EventHandler(OnSlideShowChange);
                            timer1.Start();
                        }
                        break;

                    case 2:
                        androidCamera.takePhoto();
                        break;

                    case 3:
                        Thread.Sleep(1000);
                        androidCamera.takePhoto();
                        canonCamera.takePhoto();
                        break;

                    case 4:


                        ThreadStart threadS = new ThreadStart(smartekCamera.takePhoto);
                        Thread thread = new Thread(threadS);

                        thread.Start();
                        thread.Join();

                        //thread.Abort();
                        //smartekCamera.takePhoto();

                        break;
                }

                i++;

                //picBox.Dispose();
                picBox.Image.Dispose();
                OnSlideShowChange(button1, EventArgs.Empty);

            }
            else
            {
                Thread.Sleep(1000);

                switch (captureMode)
                {
                    case 1:
                        canonCamera.closeSession();
                        break;
                    case 2:
                        AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidCameraData.androidDeviceData, androidCameraData.receiver);
                        break;
                    case 3:
                        canonCamera.closeSession();
                        AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidCameraData.androidDeviceData, androidCameraData.receiver);
                        break;
                    case 4:
                        smartekCamera.takePhoto();
                        smartekCamera.closeSession();
                        break;
                }

                System.Windows.Forms.Application.Exit();
            }
        }
    }
}