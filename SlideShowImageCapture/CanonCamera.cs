using EDSDKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlideShowImageCapture
{
    class CanonCamera
    {

        public static class canonData
        {
            public static SDKHandler CameraHandler;
            public static List<Camera> cameraList;
        }

        public void connectCanonCamera()
        {
            // SDK connection initialization
            initializeCanonSDK();
            // check for connected Canon cameras
            refreshCameraList();

            // open session with Canon camera and lock UI
            canonData.CameraHandler.OpenSession(canonData.cameraList.FirstOrDefault());
            canonData.CameraHandler.UILock(true);
        }

        private void initializeCanonSDK()
        {
            // SDK connection initialization
            canonData.CameraHandler = new SDKHandler();
            canonData.CameraHandler.CameraAdded += new SDKHandler.CameraAddedHandler(SDK_CameraAdded);
        }

        private void SDK_CameraAdded()
        {
            try { refreshCameraList(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }


        public void refreshCameraList()
        {
            canonData.cameraList = canonData.CameraHandler.GetCameraList();
        }
    }
}
