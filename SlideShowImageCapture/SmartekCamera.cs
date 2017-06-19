using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using static SlideShowImageCapture.ImageUtils;
using System.Windows.Forms;

namespace SlideShowImageCapture
{



    class SmartekCamera
    {
        public static class smartekData
        {
            public static smcs.ICameraAPI smcsApi;
            public static smcs.IDevice device;

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
                    if (smartekData.device.GetStringNodeValue("DeviceVendorName", out text))
                    {
                        Console.Out.WriteLine("Device Vendor: " + text);
                    }
                    if (smartekData.device.GetStringNodeValue("DeviceModelName", out text))
                    {
                        Console.Out.WriteLine("Device Model: " + text);
                    }
                    if (smartekData.device.GetIntegerNodeValue("Width", out int64Value))
                    {
                        Console.Out.WriteLine("Width: " + int64Value);
                    }
                    if (smartekData.device.GetIntegerNodeValue("Height", out int64Value))
                    {
                        Console.Out.WriteLine("Height: " + int64Value);
                    }

                    Int64 packetSize = 0;
                    smartekData.device.GetIntegerNodeValue("GevSCPSPacketSize", out packetSize);
                    packetSize = packetSize & 0xFFFF;
                    Console.Out.WriteLine("PacketSize: " + packetSize);


                    // disable trigger mode
                    bool status = smartekData.device.SetStringNodeValue("TriggerMode", "Off");
                    // set continuous acquisition mode
                    status = smartekData.device.SetStringNodeValue("AcquisitionMode", "Continuous");
                    // start acquisition
                    status = smartekData.device.SetIntegerNodeValue("TLParamsLocked", 1);
                    status = smartekData.device.CommandNodeExecute("AcquisitionStart");

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

            smartekData.smcsApi.SetHeartbeatTime(3);
            smartekData.smcsApi.RegisterCallback(eventHandler);



        }


        public void takePhoto()
        {

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

                    PixelFormat m_pixelFormat = PixelFormat.Format8bppIndexed;

                    Bitmap bitmap = new Bitmap((int)sizeX, (int)sizeY, m_pixelFormat);

                    Rectangle m_rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    smcs.CameraSuite.GvspGetBitsPerPixel((smcs.GVSP_PIXEL_TYPES)m_pixelType);

                    BitmapData bd = null;

                    ImageUtils.CopyToBitmap(imageInfo, ref bitmap, ref bd, ref m_pixelFormat, ref m_rect, ref m_pixelType);

                    string timeNow = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");

                    bitmap.Save("C:/Users/Ivan/Desktop/testimg/" + timeNow + ".png");

                }

                // remove (pop) image from image buffer
                smartekData.device.PopImage(imageInfo);
                // empty buffer
                smartekData.device.ClearImageBuffer();

            }
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
