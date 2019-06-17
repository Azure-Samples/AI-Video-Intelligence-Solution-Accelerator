using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CameraServer
{
    class OpenCVDeviceEnumerator
    {
        static List<CapDriver> drivers;

        static OpenCVDeviceEnumerator()
        {
            // list of all CAP drivers (see highgui_c.h)
            drivers = new List<CapDriver>
            {
                //  drivers.Add(new CapDriver { enumValue = CaptureDevice., "CV_CAP_MIL", "MIL proprietary drivers" });
                new CapDriver { enumValue = CaptureDevice.VFW, enumName = "VFW", comment = "platform native" },
                new CapDriver { enumValue = CaptureDevice.V4L, enumName = "V4L", comment = "platform native" },
                new CapDriver { enumValue = CaptureDevice.Firewire, enumName = "FireWire", comment = "IEEE 1394 drivers" },
                new CapDriver { enumValue = CaptureDevice.Fireware, enumName = "Fireware", comment = "IEEE 1394 drivers" },
                new CapDriver { enumValue = CaptureDevice.Qt, enumName = "Qt", comment = "Quicktime" },
                new CapDriver { enumValue = CaptureDevice.Unicap, enumName = "Unicap", comment = "Unicap drivers" },
                new CapDriver { enumValue = CaptureDevice.DShow, enumName = "DSHOW", comment = "DirectShow (via videoInput)" },
                new CapDriver { enumValue = CaptureDevice.PVAPI, enumName = "PVAPI", comment = "PvAPI, Prosilica GigE SDK" },
                new CapDriver { enumValue = CaptureDevice.OpenNI, enumName = "OpenNI", comment = "OpenNI(for Kinect) " },
                new CapDriver { enumValue = CaptureDevice.OpenNI_ASUS, enumName = "OpenNI_ASUS", comment = "OpenNI(for Asus Xtion) " },
                new CapDriver { enumValue = CaptureDevice.Android, enumName = "Android", comment = "Android" },
                new CapDriver { enumValue = CaptureDevice.XIAPI, enumName = "XIAPI", comment = "XIMEA Camera API" },
                new CapDriver { enumValue = CaptureDevice.AVFoundation, enumName = "AVFoundation", comment = "AVFoundation framework for iOS (OS X Lion will have the same API)" },
                new CapDriver { enumValue = CaptureDevice.Giganetix, enumName = "Giganetix", comment = "Smartek Giganetix GigEVisionSDK" },
                new CapDriver { enumValue = CaptureDevice.MSMF, enumName = "MSMF", comment = "Microsoft Media Foundation (via videoInput)" },
                new CapDriver { enumValue = CaptureDevice.WinRT, enumName = "WinRT", comment = "Microsoft Windows Runtime using Media Foundation" },
                new CapDriver { enumValue = CaptureDevice.IntelPERC, enumName = "IntelPERC", comment = "Intel Perceptual Computing SDK" },
                new CapDriver { enumValue = CaptureDevice.OpenNI2, enumName = "OpenNI2", comment = "OpenNI2 (for Kinect)" },
                new CapDriver { enumValue = CaptureDevice.OpenNI2_ASUS, enumName = "OpenNI2_ASUS", comment = "OpenNI2 (for Asus Xtion and Occipital Structure sensors)" },
                new CapDriver { enumValue = CaptureDevice.GPhoto2, enumName = "GPhoto2", comment = "gPhoto2 connection" },
                new CapDriver { enumValue = CaptureDevice.GStreamer, enumName = "GStreamer", comment = "GStreamer" },
                new CapDriver { enumValue = CaptureDevice.FFMPEG, enumName = "FFMPEG", comment = "Open and record video file or stream using the FFMPEG library" },
                new CapDriver { enumValue = CaptureDevice.Images, enumName = "Images", comment = "OpenCV Image Sequence (e.g. img_%02d.jpg)" },
                new CapDriver { enumValue = CaptureDevice.Aravis, enumName = "Aravis", comment = "Aravis SDK" }
            };
        }

        public struct CapDriver
        {
            public CaptureDevice enumValue;
            public string enumName;
            public string comment;
        };

        public List<int> EnumerateCameras()
        {
            List<int> cameraIndices = new List<int>();

            string driverName, driverComment;
            int driverEnumBase;
            Mat frame = new Mat();
            Console.WriteLine("Searching for cameras IDs...");
            for (int drv = 0; drv < drivers.Count; drv++)
            {
                driverName = drivers[drv].enumName;
                driverEnumBase = (int)drivers[drv].enumValue;
                driverComment = drivers[drv].comment;

                int maxID = 100; //100 IDs between drivers
                if (driverEnumBase == (int)CaptureDevice.VFW)
                    maxID = 10; //VWF opens same camera after 10 ?!?

                for (int idx = 0; idx < maxID; idx++)
                {

                    VideoCapture cap = new VideoCapture(driverEnumBase + idx);  // open the camera
                    if (cap.IsOpened())   // check if we succeeded
                    {
                        cap.Read(frame);
                        if (frame.Empty())
                        {
                            Console.WriteLine($"{driverName}: {driverEnumBase + idx} fails to grab");
                        }
                        else
                        {
                            cameraIndices.Add(driverEnumBase + idx);  // vector of all available cameras
                            Console.WriteLine($"{driverName}: {driverEnumBase + idx} working");
                        }
                    }
                    cap.Release();
                }

            }
            Console.WriteLine(cameraIndices.Count() + " cameras found ");

            return cameraIndices;
        }
    }
}
