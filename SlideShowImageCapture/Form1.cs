using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SharpAdbClient;

namespace SlideShowImageCapture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // show windows form on secondary screen(projector) by default, otherwise show it on pc monitor
            var secondaryScreen = Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();

            if (secondaryScreen != null)
            {
                var area = secondaryScreen.WorkingArea;

                if (!area.IsEmpty)
                {
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

            // start ADB server and find connected devices
            ADBstart adb = new ADBstart();

            adbData.androidDeviceData = adb.getFirstConnectedDevices();
            adbData.receiver = new ConsoleOutputReceiver();

            // get number of files in OpenCameraFolder
            AdbClient.Instance.ExecuteRemoteCommand("cd storage/emulated/legacy/DCIM/OpenCamera/ && ls -l | wc -l", adbData.androidDeviceData, adbData.receiver);
            noOfFilesOld = Int32.Parse(adbData.receiver.ToString());

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


        public static class adbData
        {
            public static DeviceData androidDeviceData { get; set; }
            public static ConsoleOutputReceiver receiver { get; set; }
            public static CancellationToken cancelToken { get; set; }
        }


        public class ADBstart
        {

            public DeviceData getFirstConnectedDevices()
            {
                AdbServer adbServer = new AdbServer();
                adbServer.StartServer(@"D:\Developement\android-sdk\platform-tools\adb.exe", true);

                if (AdbClient.Instance.GetDevices().Count != 0)
                {
                    return AdbClient.Instance.GetDevices().First();
                }
                else
                {
                    throw new System.ArgumentException("No Android device connected!");
                }
            }
        }



        // global variables for controlling input and time period for image slide show
        string[] images = Directory.GetFiles(@"D:\Projects\C# Projects\SlideShowImageCapture\SlideShowImageCapture\bin\Debug\Playing Cards", "*.png");
        int i = 1;
        int timePeriod = 1500;
        int noOfFilesOld, noOfFilesNew;


        private void startSlideShow(object sender, EventArgs e)
        {
            textBox1.Visible = false;

            picBox.SizeMode = PictureBoxSizeMode.Zoom;
            picBox.Dock = DockStyle.Fill;

            picBox.Image = Image.FromFile(images.First());

            Task triggerCamera1 = Task.Run(async () =>
            {
                Thread.Sleep(400);
                await AdbClient.Instance.ExecuteRemoteCommandAsync("input keyevent 27", adbData.androidDeviceData, adbData.receiver, adbData.cancelToken, 4000);
            });
            triggerCamera1.Wait();

            timer1.Interval = timePeriod;
            timer1.Tick += new EventHandler(onSlideShowTimer);
            timer1.Start();
        }


        private void onSlideShowTimer(object sender, EventArgs e)
        {
            //AdbClient.Instance.ExecuteRemoteCommand("pwd", adbData.androidDeviceData, adbData.receiver);
            //AdbClient.Instance.ExecuteRemoteCommand("ls -l | wc -l", adbData.androidDeviceData, adbData.receiver);
            //noOfFilesNew = Int32.Parse(adbData.receiver.ToString());

            //if (noOfFilesNew == noOfFilesOld + 1)
            //{

                if (i < images.Length)
                {

                    Task changeImage = Task.Run(() =>
                    {
                        picBox.Image = Image.FromFile(images[i]);
                    });
                    changeImage.Wait();

                    Task triggerCamera2 = Task.Run(async () =>
                    {
                        Thread.Sleep(400);
                        await AdbClient.Instance.ExecuteRemoteCommandAsync("input keyevent 27", adbData.androidDeviceData, adbData.receiver, adbData.cancelToken, 4000);
                    });
                    triggerCamera2.Wait();


                    i++;
                }
                else
                {
                    Thread.Sleep(1000);
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop net.sourceforge.opencamera", adbData.androidDeviceData, adbData.receiver);

                    //Thread.Sleep(timePeriod);
                    System.Windows.Forms.Application.Exit();
                }
            //}
        }
    }
}