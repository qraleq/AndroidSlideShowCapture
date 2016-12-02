using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using SharpAdbClient;
using EDSDKLib;


namespace SlideShowImageCapture
{
    public partial class SlideShowForm : Form
    {
        
        #region global

        // capture mode options
        // 1 - Canon camera
        // 2 - Android camera
        int captureMode = 1;

        // global variables for controlling input and time period for image slide show
        string[] images = Directory.GetFiles(@"D:\Projects\MATLAB Projects\Structured Light Compressive Sensing\data\1920x1080 Patterns\Calibration For Black Paper\", "*.png");
        int i = 1;
        int timerPeriod = 3500;

        AndroidOpenCamera androidOpenCamera = new AndroidOpenCamera();

        public static class androidData
        {
            public static DeviceData androidDeviceData { get; set; }
            public static ConsoleOutputReceiver receiver { get; set; }
            public static CancellationToken cancelToken { get; set; }
        }

        public static class canonData
        {
            public static SDKHandler CameraHandler;
            public static List<Camera> cameraList;
        }

        int errorCount;
        object errorLock = new object();

        #endregion


        public SlideShowForm()
        {
            try
            {
                InitializeComponent();

                // sets projection screen and windows form properties
                SetScreenAndFormProperties();

                // initializes ADB or Canon PTP communication
                InitializeCommunicationInterfaces();

            }
            catch (DllNotFoundException) { ReportError("Canon DLLs not found!"); }
            catch (Exception ex) { ReportError(ex.Message); }
        }



        private void InitializeCommunicationInterfaces()
        {
            // depending on which capture mode has been selected, initialize interfaces
            switch (captureMode)
            {
                case 1:
                    // SDK connection initialization
                    canonData.CameraHandler = new SDKHandler();
                    canonData.CameraHandler.CameraAdded += new SDKHandler.CameraAddedHandler(SDK_CameraAdded);

                    // check for connected Canon cameras
                    RefreshCameraList();
                    canonData.CameraHandler.OpenSession(canonData.cameraList.FirstOrDefault());
                    canonData.CameraHandler.UILock(true);
                    break;


                case 2:
                    // start ADB server and find connected devices
                    ADB adb = new ADB();
                    adb.StartAdbServer();

                    androidData.androidDeviceData = adb.GetFirstConnectedDevice();
                    androidData.receiver = new ConsoleOutputReceiver();

                    // start Open Camera Android App
                    androidOpenCamera.OpenCameraApp();
                    break;


                default:
                    ReportError("Non valid mode has been selected!");
                    break;
            }
        }

        private void SetScreenAndFormProperties()
        {
            // show windows form on secondary screen(projector) by default, otherwise show it on pc monitor
            var secondaryScreen = Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();

            if (secondaryScreen != null)
            {
                var area = secondaryScreen.WorkingArea;

                if (!area.IsEmpty)
                {
                    // windows form properties for secondary screen
                    this.Show();
                    this.Left = area.X;
                    this.Top = area.Y;
                    this.Width = area.Width;
                    this.Height = area.Height;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    Cursor.Hide();
                }
            }
            else
            {
                // windows form properties for primary screen
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                Cursor.Hide();
            }
        }


        private void SDK_CameraAdded()
        {
            try { RefreshCameraList(); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void RefreshCameraList()
        {
            canonData.cameraList = canonData.CameraHandler.GetCameraList();
        }


        public class ADB
        {
            public void StartAdbServer()
            {
                AdbServer adbServer = new AdbServer();
                adbServer.StartServer(@"D:\Developement\android-sdk\platform-tools\adb.exe", true);
            }

            
            public DeviceData GetFirstConnectedDevice()
            {
                if (AdbClient.Instance.GetDevices().Count != 0)
                {
                    return AdbClient.Instance.GetDevices().First();
                }
                else
                {
                    MessageBox.Show("No Android device connected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new ArgumentException("No Android device connected!");
                }
            }
        }

        
        public class AndroidOpenCamera
        {
            public void OpenCameraApp()
            {
                // turn screen on if it is off and run OpenCamera app
                Task startCameraApp = Task.Run(async () =>
                {
                    AdbClient.Instance.ExecuteRemoteCommand("service call power 12", androidData.androidDeviceData, androidData.receiver);
                    var receiverData = androidData.receiver.ToString();

                    if (receiverData.Equals("Result: Parcel(00000000 00000000   '........')\r\n"))
                    {
                        AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_POWER", androidData.androidDeviceData, androidData.receiver);
                    }

                    await AdbClient.Instance.ExecuteRemoteCommandAsync("am start -n net.sourceforge.opencamera/net.sourceforge.opencamera.MainActivity", androidData.androidDeviceData, androidData.receiver, androidData.cancelToken, 5000);
                });
                startCameraApp.Wait();
            }

            public void TakePhoto()
            {
                Thread.Sleep(600);
                Task triggerAndroidCamera = Task.Run(async () =>
                {
                    await AdbClient.Instance.ExecuteRemoteCommandAsync("input keyevent 27", androidData.androidDeviceData, androidData.receiver, androidData.cancelToken, 4000);
                });
                triggerAndroidCamera.Wait();
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
                canonData.CameraHandler.ImageSaveDirectory = @"D:\Measurements";
                canonData.CameraHandler.SetCapacity();
                canonData.CameraHandler.TakePhoto();
            }
            else if (captureMode == 2)
            {
                androidOpenCamera.TakePhoto();
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
                
                textBox2.Text =Path.GetFileName(images[i]);

                if (captureMode == 1)
                {
                    Thread.Sleep(600);


                    // save to computer
                    canonData.CameraHandler.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
                    canonData.CameraHandler.ImageSaveDirectory = @"D:\Test";

                    //canonData.CameraHandler.ImageSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    canonData.CameraHandler.SetCapacity();


                    canonData.CameraHandler.TakePhoto();
                }
                else if (captureMode == 2)
                {
                    androidOpenCamera.TakePhoto();
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
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", androidData.androidDeviceData, androidData.receiver);
                }

                Thread.Sleep(1000);
                System.Windows.Forms.Application.Exit();
            }
        }



        private void ReportError(string message)
        {
            int errc;
            lock (errorLock) { errc = ++errorCount; }

            if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lock (errorLock) { errorCount--; }
        }

    }
}