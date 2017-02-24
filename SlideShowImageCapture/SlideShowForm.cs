using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SharpAdbClient;
using EDSDKLib;
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
        int captureMode = 2;

        // global variables for controlling input and time period for image slide show
        string[] images = Directory.GetFiles(@"D:\Projects\MATLAB Projects\Structured Light Compressive Sensing\data\1920x1080 Patterns\Measurement Masks\", "*.png");
        int i = 1;
        int timerPeriod = 3500;

        AndroidCamera androidCamera = new AndroidCamera();
        CanonCamera canonCamera = new CanonCamera();


        #endregion

        public SlideShowForm()
        {
            try
            {
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

                default:
                    MessageBox.Show("Non valid mode has been selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

     
        private void OnSlideshowStart(object sender, EventArgs e)
        {
            textBox1.Visible = false;

            // textbox containing current filename
            textBox2.Visible = true;
            textBox2.Text = Path.GetFileName(images.First());

            picBox.SizeMode = PictureBoxSizeMode.Zoom;
            picBox.Dock = DockStyle.Fill;

            Task showFirstSlide = Task.Run(() =>
            {
                picBox.Image = Image.FromFile(images.First());

            });

            showFirstSlide.Wait();


            if (captureMode == 1)
            {
                Thread.Sleep(600);

                // save to computer
                canonData.CameraHandler.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
                canonData.CameraHandler.ImageSaveDirectory = @"D:\Measurements\";
                canonData.CameraHandler.SetCapacity();
                canonData.CameraHandler.TakePhoto();
            }
            else if (captureMode == 2)
            {
                androidCamera.takePhoto();
            }

            timer1.Interval = timerPeriod;
            timer1.Tick += new EventHandler(OnSlideShowTimer);
            timer1.Start();
        }


        private void OnSlideShowTimer(object sender, EventArgs e)
        {
            if (i < images.Length)
            {
                picBox.Image = Image.FromFile(images[i]);

                textBox2.Text = Path.GetFileName(images[i]);

                if (captureMode == 1)
                {
                    Thread.Sleep(600);


                    // save to computer
                    canonData.CameraHandler.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
                    canonData.CameraHandler.ImageSaveDirectory = @"D:\Measurements\";

                    //canonData.CameraHandler.ImageSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    canonData.CameraHandler.SetCapacity();


                    canonData.CameraHandler.TakePhoto();
                }
                else if (captureMode == 2)
                {
                    androidCamera.takePhoto();
                }

                i++;
            }
            else
            {
                Thread.Sleep(1000);

                if (captureMode == 1)
                {
                    canonData.CameraHandler.UILock(false);
                    //canonData.CameraHandler.CloseSession();
                    //canonData.CameraHandler.Dispose();
                }
                else if (captureMode == 2)
                {
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidCameraData.androidDeviceData, androidCameraData.receiver);
                }

                Thread.Sleep(1000);
                System.Windows.Forms.Application.Exit();
            }
        }

    }
}