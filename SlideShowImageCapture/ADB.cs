using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAdbClient;
using System.Windows.Forms;

namespace SlideShowImageCapture
{
    class ADB
    {
        // start Android Debugging Bridge
        public void startAdbServer()
        {
            try
            {
                AdbServer adbServer = new AdbServer();
                adbServer.StartServer(@"D:\Developement\android-sdk\platform-tools\adb.exe", true);
                AdbServerStatus adbStatus = adbServer.GetStatus();

                if (!adbStatus.IsRunning)
                {
                    MessageBox.Show("ADB Server not running! Check if ADB tools installed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

        // check for connected devices
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
}
