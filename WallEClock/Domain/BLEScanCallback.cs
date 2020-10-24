using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WallEClock.Domain
{
    public class BLEScanCallback : ScanCallback
    {
        public Action<BluetoothDevice> OnDeviceScanned { get; set; }
        public BLEScanCallback(Action<BluetoothDevice> handler) : base()
        {
            OnDeviceScanned = handler;
        }
        public BLEScanCallback(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            OnDeviceScanned?.Invoke(result.Device);
            base.OnScanResult(callbackType, result);
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            Console.WriteLine(errorCode.ToString());
            base.OnScanFailed(errorCode);
        }
    }
}