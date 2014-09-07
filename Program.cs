﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpRift
{
    class Program
    {
        static void Main(string[] args)
        {
            VR.Initialize();

            HMDisplay vrDevice;

            if (VR.Detect() > 0)
            {
                // Create a link to the first VR device
                vrDevice = VR.Create(0);
            }
            else
            {
                // Create Fake VR Device
                vrDevice = VR.CreateDebug(HMDisplayType.DK2);
            }

            // Start Sensor
            if (VR.ConfigureTracking(vrDevice, SensorCaps.Orientation | SensorCaps.Position | SensorCaps.YawCorrection, 0))
            {
                Console.WriteLine("Sensor has started");
            }

            VR.Destroy(vrDevice);
            VR.Shutdown();
        }
    }
}
