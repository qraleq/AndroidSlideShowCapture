﻿using EDSDKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SlideShowImageCapture
{
    public class CanonCamera
    {

        static bool WaitForEvent;


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
            if (!canonData.CameraHandler.CameraSessionOpen)
            {
                canonData.CameraHandler.OpenSession(canonData.cameraList.FirstOrDefault());
            }

        }

        public void takePhoto()
        {
            // save to computer
            canonData.CameraHandler.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
            canonData.CameraHandler.ImageSaveDirectory = @"D:\Measurements\";

            canonData.CameraHandler.SetCapacity();

            try
            {
                canonData.CameraHandler.TakePhoto();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        
        private void refreshCameraList()
        {
            canonData.cameraList = canonData.CameraHandler.GetCameraList();
        }

        public void closeSession()
        {
            canonData.CameraHandler.UILock(false);
            canonData.CameraHandler.CloseSession();
            canonData.CameraHandler.Dispose();
        }

    }
}
