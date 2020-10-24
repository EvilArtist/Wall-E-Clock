using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace WallEClock.Domain
{
    public class BluetoothConst
    {
        public static UUID TX_POWER_UUID = UUID.FromString("00001804-0000-1000-8000-00805f9b34fb");
        public static UUID TX_POWER_LEVEL_UUID = UUID.FromString("00002a07-0000-1000-8000-00805f9b34fb");
        public static UUID CCCD = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
        public static UUID FIRMWARE_REVISON_UUID = UUID.FromString("00002a26-0000-1000-8000-00805f9b34fb");
        public static UUID DIS_UUID = UUID.FromString("0000180a-0000-1000-8000-00805f9b34fb");
        public static UUID UartServiceId = UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        public static UUID RxCharId = UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
        public static UUID TxCharId = UUID.FromString("6e400003-b5a3-f393-e0a9-e50e24dcca9e");

        public static string BluetoothName = "ESpaceClock";


        public static readonly long ScanPeriod = 60000;
    }
}