using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WallEClock.Domain
{
    public class ClockBroadcastReceiver : BroadcastReceiver
    {
        public EventHandler<BluetoothDevice> OnDeviceDiscoveried;       
        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            if (action == BluetoothDevice.ActionFound)
            {
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                OnDeviceDiscoveried?.Invoke(context, device);
            }
        }
    }
}