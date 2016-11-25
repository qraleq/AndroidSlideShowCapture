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
        SDKHandler CameraHandler;
        int ErrCount;
        object ErrLock = new object();
        List<Camera> CamList;

        // 1 - Canon camera
        // 2 - Android camera
        int captureMode = 1;

        // global variables for controlling input and time period for image slide show
        string[] images = Directory.GetFiles(@"D:\Projects\C# Projects\SlideShowImageCapture\SlideShowImageCapture\bin\Debug\Playing Cards", "*.png");
        int i = 1;
        int timerPeriod = 2000;


        public SlideShowForm()
        {
            try
            {
                InitializeComponent();

                // show windows form on secondary screen(projector) by default, otherwise show it on pc monitor
                var secondaryScreen = Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();

                if (secondaryScreen != null)
                {
                    var area = secondaryScreen.WorkingArea;

                    if (!area.IsEmpty)
                    {
                        // windows form properties
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
                    // windows form properties
                    this.MaximizeBox = false;
                    this.MinimizeBox = false;
                    this.TopMost = false;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    Cursor.Hide();

                }


                switch (captureMode)
                {
                    case 1:
                        CameraHandler = new SDKHandler();
                        CameraHandler.CameraAdded += new SDKHandler.CameraAddedHandler(SDK_CameraAdded);

                        RefreshCameraList();

                        CameraHandler.OpenSession(CamList.FirstOrDefault());

                        break;


                    case 2:
                        // start ADB server and find connected devices
                        ADBstart adb = new ADBstart();
                        adb.startAdbServer();

                        adbData.androidDeviceData = adb.getFirstConnectedDevice();
                        adbData.receiver = new ConsoleOutputReceiver();

                        // start Open Camera Android App
                        OpenCamera androidOpenCamera = new OpenCamera();
                        androidOpenCamera.openCameraApp();
                        break;

                    default:
                        break;
                }





            }
            catch (DllNotFoundException) { ReportError("Canon DLLs not found!"); }
            catch (Exception ex) { ReportError(ex.Message); }
        }


        private void SDK_CameraAdded()
        {
            try { RefreshCameraList(); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void RefreshCameraList()
        {
            CamList = CameraHandler.GetCameraList();
        }






        public static class adbData
        {
            public static DeviceData androidDeviceData { get; set; }
            public static ConsoleOutputReceiver receiver { get; set; }
            public static CancellationToken cancelToken { get; set; }
        }


        public class ADBstart
        {

            public void startAdbServer()
            {
                AdbServer adbServer = new AdbServer();
                adbServer.StartServer(@"D:\Developement\android-sdk\platform-tools\adb.exe", true);
            }


            public DeviceData getFirstConnectedDevice()
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



        public class OpenCamera
        {
            public void openCameraApp()
            {
                // turn screen on if it is off and run OpenCamera app
                Task startCameraApp = Task.Run(async () =>
                {
                    AdbClient.Instance.ExecuteRemoteCommand("service call power 12", adbData.androidDeviceData, adbData.receiver);
                    var receiverData = adbData.receiver.ToString();

                    if (receiverData.Equals("Result: Parcel(00000000 00000000   '........')\r\n"))
                    {
                        AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_POWER", adbData.androidDeviceData, adbData.receiver);
                    }

                    await AdbClient.Instance.ExecuteRemoteCommandAsync("am start -n net.sourceforge.opencamera/net.sourceforge.opencamera.MainActivity", adbData.androidDeviceData, adbData.receiver, adbData.cancelToken, 5000);
                });
                startCameraApp.Wait();
            }
        }







        private void startSlideShow(object sender, EventArgs e)
        {
            textBox1.Visible = false;

            picBox.SizeMode = PictureBoxSizeMode.Zoom;
            picBox.Dock = DockStyle.Fill;

            picBox.Image = Image.FromFile(images.First());


            if (captureMode==1)
            {
                Thread.Sleep(400);
                CameraHandler.TakePhoto();
            }
            else
            {
                Task triggerAndroidCamera1 = Task.Run(async () =>
                {
                    Thread.Sleep(400);
                    await AdbClient.Instance.ExecuteRemoteCommandAsync("input keyevent 27", adbData.androidDeviceData, adbData.receiver, adbData.cancelToken, 4000);
                });
                triggerAndroidCamera1.Wait();
            }

            timer1.Interval = timerPeriod;
            timer1.Tick += new EventHandler(onSlideShowTimer);
            timer1.Start();
        }


        private void onSlideShowTimer(object sender, EventArgs e)
        {
            if (i < images.Length)
            {

                Task changeImage = Task.Run(() =>
                {
                    picBox.Image = Image.FromFile(images[i]);
                });
                changeImage.Wait();


                if (captureMode == 1)
                {
                    Thread.Sleep(400);
                    CameraHandler.TakePhoto();
                }
                else
                {
                    Task triggerAndroidCamera2 = Task.Run(async () =>
                    {
                        Thread.Sleep(400);
                        await AdbClient.Instance.ExecuteRemoteCommandAsync("input keyevent 27", adbData.androidDeviceData, adbData.receiver, adbData.cancelToken, 4000);
                    });
                    triggerAndroidCamera2.Wait();
                }


                i++;
            }
            else
            {
                Thread.Sleep(1000);

                if (captureMode==1)
                {
                    CameraHandler.CloseSession();
                    CameraHandler.Dispose();
                        
                }
                else
                {
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", adbData.androidDeviceData, adbData.receiver);

                }

                //Thread.Sleep(timePeriod);
                System.Windows.Forms.Application.Exit();
            }
        }



        private void ReportError(string message)
        {
            int errc;
            lock (ErrLock) { errc = ++ErrCount; }

            if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lock (ErrLock) { ErrCount--; }
        }

    }
}