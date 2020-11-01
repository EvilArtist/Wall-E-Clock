using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;

namespace WallEClock.Domain
{
    public class ConnectThread : Thread
    {
        private readonly BluetoothSocket socket;
        private readonly Handler handler;

        public ConnectThread(BluetoothSocket socket, Handler handler)
        {
            this.socket = socket;
            this.handler = handler;
        }

        public override async void Run()
        {
            byte[] buffer = new byte[1024];
                      
            while (true)
            {
                try
                {
                    int bytes = await socket.InputStream.ReadAsync(buffer);
                    if (bytes > 0)
                    {
                        await Task.Delay(100);
                        handler.ObtainMessage(2, bytes, -1, buffer)
                                .SendToTarget();
                    }
                }
                catch 
                {
                }
            }
        }

        public void Write(byte[] input)
        {
            try
            {
                socket.OutputStream.Write(input, 0, input.Length);
            }
            catch
            {
            }
        }

        public void Close()
        {
            try
            {
                socket.Close();
            }
            catch  { }
        }

        public async Task ConnectAsync() => await socket.ConnectAsync();
    }

    class ClockHandler : Handler
    {
        public Action<byte[]> OnMessageReceived { get; set; }
        public Action<bool, Java.Lang.Object> OnDeviceConnected { get; set; }
        public static int REQUEST_ENABLE_BT = 1; // used to identify adding bluetooth names
        public static int MESSAGE_READ = 2; // used in bluetooth handler to identify message update
        public static int CONNECTING_STATUS = 3; // used in bluetooth handler to identify message status
        public ClockHandler(Looper looper) : base(looper)
        {
        }

        public override void HandleMessage(Message msg)
        {
            if (msg.What == MESSAGE_READ)
            {
                var data = (byte[])msg.Obj;

                OnMessageReceived?.Invoke(data.Where((x,i) => (i < msg.Arg1)).ToArray());
            }
            if (msg.What == CONNECTING_STATUS)
            {
                OnDeviceConnected?.Invoke(msg.Arg1 == 1, msg.Obj);
            }
        }
    }
}