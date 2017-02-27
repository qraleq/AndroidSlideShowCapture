using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAdbClient;
using System.Threading;

namespace SlideShowImageCapture
{
    class AndroidCamera
    {

        public static class androidCameraData
        {
            public static DeviceData androidDeviceData { get; set; }
            public static ConsoleOutputReceiver receiver { get; set; }
            public static CancellationToken cancelToken { get; set; }
        }

        public void connectAndroidCamera()
        {
            // start ADB server and find connected devices
            ADB adb = new ADB();
            adb.startAdbServer();

            androidCameraData.androidDeviceData = adb.getFirstConnectedDevice();
            androidCameraData.receiver = new ConsoleOutputReceiver();

            // start Open Camera Android App
            openCameraApp();
        }


        private void openCameraApp()
        {
            // turn screen on if it is off and run OpenCamera app
            Task startCameraApp = Task.Run(() =>
            {
                AdbClient.Instance.ExecuteRemoteCommand("service call power 12", androidCameraData.androidDeviceData, androidCameraData.receiver);
                var receiverData = androidCameraData.receiver.ToString();

                if (receiverData.Equals("Result: Parcel(00000000 00000000   '........')\r\n"))
                {
                    AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_POWER", androidCameraData.androidDeviceData, androidCameraData.receiver);
                    AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_POWER", androidCameraData.androidDeviceData, androidCameraData.receiver);

                }
                //await AdbClient.Instance.ExecuteRemoteCommandAsync("am start -n com.oneplus.camera/com.oneplus.camera", androidCameraData.androidDeviceData, androidCameraData.receiver, androidCameraData.cancelToken, 5000);
                //await AdbClient.Instance.ExecuteRemoteCommandAsync("am start -n com.oneplus.camera/.MainActivity", androidCameraData.androidDeviceData, androidCameraData.receiver, androidCameraData.cancelToken, 5000);
                Thread.Sleep(3000);
                AdbClient.Instance.ExecuteRemoteCommand("input tap 80 80", androidCameraData.androidDeviceData, androidCameraData.receiver);
                Thread.Sleep(3000);
                AdbClient.Instance.ExecuteRemoteCommand("input tap 240 410", androidCameraData.androidDeviceData, androidCameraData.receiver);

                //AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_POWER", androidCameraData.androidDeviceData, androidCameraData.receiver);
                //AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_POWER", androidCameraData.androidDeviceData, androidCameraData.receiver);
                //await AdbClient.Instance.ExecuteRemoteCommandAsync("am start -n net.sourceforge.opencamera/net.sourceforge.opencamera.MainActivity", androidCameraData.androidDeviceData, androidCameraData.receiver, androidCameraData.cancelToken, 5000);
                //await AdbClient.Instance.ExecuteRemoteCommandAsync("am start -a android.media.action.IMAGE_CAPTURE", androidData.androidDeviceData, androidData.receiver, androidData.cancelToken, 5000);

            });
            startCameraApp.Wait();
        }

        public void takePhoto()
        {
            Thread.Sleep(600);
            Task triggerAndroidCamera = Task.Run(async () =>
            {
                await AdbClient.Instance.ExecuteRemoteCommandAsync("input keyevent 27", androidCameraData.androidDeviceData, androidCameraData.receiver, androidCameraData.cancelToken, 4000);
            });
            triggerAndroidCamera.Wait();
        }
    }
}
