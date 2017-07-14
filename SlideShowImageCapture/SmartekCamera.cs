using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using static SlideShowImageCapture.ImageUtils;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace SlideShowImageCapture
{

    public class SmartekCamera
    {
        public static class smartekData
        {
            public static smcs.ICameraAPI smcsApi;
            public static smcs.IDevice device = null;
            public static int numImages = 1;
        }



        public void connectSmartekCamera()
        {
            initializeSmartekAPI();

            //  discover all devices on network
            smartekData.smcsApi.FindAllDevices(1.0);
            smcs.IDevice[] devices = smartekData.smcsApi.GetAllDevices();

            if (devices.Length > 0)
            {

                // get the first device in the list
                smartekData.device = devices[0];

                // uncomment to use specific model
                //for (int i = 0; i < devices.Length; i++) 
                //{
                //    if (devices[i].GetModelName() == "GC652M") {
                //        m_device = devices[i];
                //    }
                //}


                // to change number of images in image buffer from default 10 images 
                // call SetImageBufferFrameCount() method before Connect() method
                //device.SetImageBufferFrameCount(20);

                if (smartekData.device != null && smartekData.device.Connect())
                {
                    string text;
                    Int64 int64Value;

                    Console.Out.WriteLine("Connected to first camera: " + smartekData.device.GetIpAddress());
                    smartekData.device.GetStringNodeValue("DeviceVendorName", out text);
                    Console.Out.WriteLine("Device Vendor: " + text);
                    smartekData.device.GetStringNodeValue("DeviceModelName", out text);
                    Console.Out.WriteLine("Device Model: " + text);
                    smartekData.device.GetIntegerNodeValue("Width", out int64Value);
                    Console.Out.WriteLine("Width: " + int64Value);
                    smartekData.device.GetIntegerNodeValue("Height", out int64Value);
                    Console.Out.WriteLine("Height: " + int64Value);
                    smartekData.device.GetIntegerNodeValue("GevSCPSPacketSize", out int64Value);
                    int64Value = int64Value & 0xFFFF;
                    Console.Out.WriteLine("PacketSize: " + int64Value);

                    smartekData.device.SetStringNodeValue("ExposureTime", "8333,34");

                    smartekData.device.SetStringNodeValue("GainAutoBalance", "Off");
                    smartekData.device.SetStringNodeValue("Gain", "12");


                    smartekData.device.GetStringNodeValue("ExposureTime", out text);
                    Console.Out.WriteLine("Exposure Time: " + text);

                    smartekData.device.GetStringNodeValue("GainAutoBalance", out text);
                    Console.Out.WriteLine("GainAutoBalance: " + text);

                    smartekData.device.GetStringNodeValue("Gain", out text);
                    Console.Out.WriteLine("Gain: " + text);

                    // disable trigger mode
                    bool status = smartekData.device.SetStringNodeValue("TriggerMode", "Off");
                    ////status = smartekData.device.SetStringNodeValue("TriggerSource", "Software");

                    // set continuous acquisition mode
                    status = smartekData.device.SetStringNodeValue("AcquisitionMode", "Single");
                    // start acquisition
                    status = smartekData.device.SetIntegerNodeValue("TLParamsLocked", 1);

                    //status = smartekData.device.SetStringNodeValue("TriggerSelector", "FrameStart");
                    status = smartekData.device.SetImageBufferFrameCount(1);
                }
            }
        }


        public void initializeSmartekAPI()
        {

            CallbackHandler eventHandler = new CallbackHandler();

            smcs.CameraSuite.InitCameraAPI();
            smartekData.smcsApi = smcs.CameraSuite.GetCameraAPI();

            if (!smartekData.smcsApi.IsUsingKernelDriver())
            {
                Console.Out.WriteLine("Warning: Smartek Filter Driver not loaded");
            }

            smartekData.smcsApi.SetHeartbeatTime(1);
            smartekData.smcsApi.RegisterCallback(eventHandler);
        }

        public void closeSession()
        {
            smartekData.device.CommandNodeExecute("AcquisitionStop");
            smartekData.device.SetIntegerNodeValue("TLParamsLocked", 0);
            smartekData.device.Disconnect();

            smcs.CameraSuite.ExitCameraAPI();

        }



        public void takePhoto()
        {

            smartekData.device.CommandNodeExecute("AcquisitionStart");

            smcs.IImageInfo imageInfo = null;

            //Console.Out.WriteLine("Acquisition Start, press any key to exit loop...");
            smartekData.device.GetImageInfo(ref imageInfo);

            if (!smartekData.device.IsBufferEmpty())
            {
                if (imageInfo != null)
                {
                    uint sizeX, sizeY;

                    imageInfo.GetSize(out sizeX, out sizeY);

                    uint m_pixelType = 0;

                    PixelFormat m_pixelFormat = PixelFormat.Format16bppGrayScale;
                    //PixelFormat m_pixelFormat = PixelFormat.Format24bppRgb;

                    Bitmap bitmap = new Bitmap((int)sizeX, (int)sizeY, m_pixelFormat);

                    Rectangle m_rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    
                    smcs.CameraSuite.GvspGetBitsPerPixel((smcs.GVSP_PIXEL_TYPES)17825797);
                    
                    //smcs.CameraSuite.GvspGetBitsPerPixel((smcs.GVSP_PIXEL_TYPES)17825838);

                    BitmapData bd = null;

                    ImageUtils.CopyToBitmap(imageInfo, ref bitmap, ref bd, ref m_pixelFormat, ref m_rect, ref m_pixelType);

                    string timeNow = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");

                    bitmap.Save("C:/Users/Ivan/Desktop/testimg/" + timeNow + ".tiff", ImageFormat.Tiff);


                    bitmap.Dispose();
                }
            }
            // remove (pop) image from image buffer
            smartekData.device.PopImage(imageInfo);

            // empty buffer
            smartekData.device.ClearImageBuffer();
        }



        class CallbackHandler : smcs.ICallbackEvent
        {
            #region ICallbackEvent Members
            // Warning! Callback handler is called in context of API thread and for real GUI app need to be synchronised to GUI thread. 	    
            public void OnConnect(smcs.IDevice device)
            {
                System.Console.WriteLine("Connected!");
            }

            public void OnDisconnect(smcs.IDevice device)
            {
                System.Console.WriteLine("Disconnected!");
            }

            public void OnLog(smcs.IDevice device, smcs.EventMessage message)
            {
                System.Console.WriteLine("Log: " + message.messageString);
            }

            #endregion
        }
    }
}
