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
using Java.Util;

namespace WallEClock.Domain
{
    public class BluetoothConnection
    {
        public BluetoothAdapter Adapter => BluetoothAdapter.DefaultAdapter;
        public IEnumerable<BluetoothDevice> Devices => (from d in Adapter.BondedDevices select d);
        public BluetoothSocket Socket { get; set; }
        private static readonly UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        public void StartDiscovery()
        {
            Adapter.StartDiscovery();
        }

        public void ConnectToDevice(string address)
        {
            var device = Adapter.GetRemoteDevice(address);
            StopDiscovery();
            try
            {
                Socket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                Socket.Connect();
            }
            catch
            {
                try
                {
                    Socket?.Close();
                }
                catch { }
            }
        }

        public void Send(byte[] data)
        {
            Socket.OutputStream.Write(data, 0, data.Length);
        }

        public void CheckStatus(Context context)
        {
            //asignamos el sensor bluetooth con el que vamos a trabajar

            //Verificamos que este habilitado
            if (!Adapter.Enable())
            {
                Toast.MakeText(context, "Bluetooth Deactivate",
                    ToastLength.Short).Show();

            }
            //verificamos que no sea nulo el sensor
            if (Adapter == null)
            {
                Toast.MakeText(context,
                    "Bluetooth Adaptor Not found", ToastLength.Short)
                    .Show();
            }
        }

        public void StopDiscovery()
        {
            Adapter.CancelDiscovery();
        }

    }
}