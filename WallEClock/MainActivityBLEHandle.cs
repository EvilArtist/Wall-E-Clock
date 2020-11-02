using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;
using WallEClock.Common;
using WallEClock.Domain;

namespace WallEClock
{
    public partial class MainActivity
    {
        private BluetoothDeviceAdapter listviewAdapter;

        private BluetoothAdapter bluetoothAdapter;
        private BluetoothSocket bluetoothSocket;
        private BroadcastReceiver receiver;
        public ObservableCollection<BluetoothDevice> Devices { get; set; }
        private ApplicationState applicationState;

        private static readonly UUID bluetoothUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        ClockHandler handler;
        private void InitializeDeviceList()
        {
            if (deviceListPage is null)
            {
                deviceListPage = FindViewById(Resource.Id.layout_device_list);
                Devices = new ObservableCollection<BluetoothDevice>();
                bleDeviceList = FindViewById<ListView>(Resource.Id.list_view);
                listviewAdapter = new BluetoothDeviceAdapter(this, Devices);
                bleDeviceList.Adapter = listviewAdapter;
                bleDeviceList.ItemClick += BleDeviceList_ItemClick;

                BluetoothManager bluetoothManager = (BluetoothManager)GetSystemService(Context.BluetoothService);
                bluetoothAdapter = bluetoothManager.Adapter;

                receiver = new ClockBroadcastReceiver()
                {
                    OnDeviceDiscoveried = OnDeviceDiscoveried
                };
                IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
                RegisterReceiver(receiver, filter);

                handler = new ClockHandler(MainLooper)
                {
                    OnMessageReceived = OnDataReceived,
                    OnDeviceConnected = OnDeviceConnected
                };

                Devices.Clear();
                foreach (var device in bluetoothAdapter.BondedDevices.Where(x => x.Name.Contains(BluetoothConst.BluetoothName)))
                {
                    Devices.Add(device);
                    listviewAdapter.NotifyDataSetChanged();
                }
            }
        }

        private async void InitializeDevice()
        {
            InitializeDeviceList();
            await ReadConfigAsync();

            if (!string.IsNullOrEmpty(applicationState.DeviceAddress))
            {
                await ConnectDevice(applicationState.DeviceAddress, false);
            }
            else
            {
                RequestToScanDevice();
            }
        }

        private void RequestToScanDevice()
        {
            homePage.LayoutFadeout(150, 0);
            deviceListPage.LayoutComeFromRight(300, 150);
            requestScan = true;
            if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                RequestPermissions(new string[]{
                    Android.Manifest.Permission.AccessCoarseLocation,
                    Android.Manifest.Permission.Bluetooth,
                    Android.Manifest.Permission.BluetoothAdmin,
                }, 1);
            }
            else
            {
                ScanDevice();
            }
        }

        private void ScanDevice()
        {
            if (!bluetoothAdapter.IsEnabled)
            {
                bluetoothAdapter.Enable();
            }
            bluetoothAdapter.StartDiscovery();
        }

        private async Task ConnectDevice(string address, bool saveAddress)
        {
            if (bluetoothSocket?.IsConnected ?? false)
            {
                return;
            }
            bluetoothAdapter.CancelDiscovery();
            var device = bluetoothAdapter.GetRemoteDevice(address);
            try
            {
                bluetoothSocket = device.CreateRfcommSocketToServiceRecord(bluetoothUUID);
                Task timeout = Task.Delay(5000);
                Task connectTask =bluetoothSocket.ConnectAsync();
                await Task.WhenAny(timeout, connectTask);
                
                if (saveAddress)
                {
                    await SaveConfigAsync();
                }
                if (bluetoothSocket.IsConnected)
                {
                    View connectView = FindViewById(Resource.Id.connecting_view);
                    connectView.Visibility = ViewStates.Gone;

                    handler.ObtainMessage(ClockHandler.CONNECTING_STATUS, 1, -1, address)
                            .SendToTarget();
                }
                else
                {
                    handler.ObtainMessage(ClockHandler.CONNECTING_STATUS, 1, -1, address)
                           .SendToTarget();
                }
            }
            catch
            {
                try
                {
                    bluetoothSocket?.Close();
                }
                catch { }
            }
        }

        #region BluetoothIO

        private async Task SocketWriteAsync(byte command)
        {
            await SocketWriteAsync(command, new byte[] { });
        }

        private async Task SocketWriteAsync(byte command, params byte[] data)
        {
            if (bluetoothSocket == null)
            {
                RunOnUiThread(() =>
                {
                    Toast toast = Toast.MakeText(this, Resource.String.bluetooth_not_available, ToastLength.Short);
                    toast.Show();
                });
                return;
            }
            if (!bluetoothSocket.IsConnected)
            {
                if (string.IsNullOrEmpty(applicationState.DeviceAddress))
                {
                    await ConnectDevice(applicationState.DeviceAddress, false);
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        Toast toast = Toast.MakeText(this, Resource.String.bluetooth_not_available, ToastLength.Short);
                        toast.Show();
                    });
                    return;
                }
            }
            try
            {
                if (data.Length > 0)
                {
                    await bluetoothSocket.OutputStream.WriteAsync(FrameEncoder.Encode(command, applicationState.Password, data));
                }
                else
                {
                    await bluetoothSocket.OutputStream.WriteAsync(FrameEncoder.Encode(command, applicationState.Password));
                }
            }
            catch
            {
                RunOnUiThread(() =>
                {
                    Toast toast = Toast.MakeText(this, Resource.String.bluetooth_disconnected, ToastLength.Short);
                    toast.Show();
                });
            }
        }

        #endregion


        #region Config
        private async Task SaveConfigAsync()
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(applicationState);
                await File.WriteAllTextAsync(DataFile, jsonString);
            }
            catch { }
        }

        private async Task ReadConfigAsync()
        {
            if (File.Exists(DataFile))
            {
                using var reader = new StreamReader(DataFile, true);
                var jsonstring = await reader.ReadToEndAsync();
                applicationState = JsonSerializer.Deserialize<ApplicationState>(jsonstring);
            }
            else
            {
                applicationState = new ApplicationState
                {
                    Password = new byte[4] { 0, 0, 0, 0 }
                };
            }
        }
        #endregion

        #region BluetoothEvent 
        private void OnDataReceived(byte[] receiveData)
        {
            if (receiveData.Length < 4 || receiveData[0] != FrameEncoder.StartFrame || receiveData[^1] != FrameEncoder.EndFrame)
            {
                RunOnUiThread(() =>
                {
                    Toast toast = Toast.MakeText(this, Resource.String.invalid_message, ToastLength.Long);
                    toast.Show();
                });
                return;
            }

            if (receiveData[2] == FrameEncoder.PasswordIncorrect)
            {
                RunOnUiThread(() => { Toast toast = Toast.MakeText(this, Resource.String.password_incorrect, ToastLength.Long); toast.Show(); });
                return;
            }
            if (receiveData[2] == FrameEncoder.Failed)
            {
                RunOnUiThread(() => { Toast toast = Toast.MakeText(this, Resource.String.something_wrong, ToastLength.Long); toast.Show(); });
                return;
            }

            if (receiveData[1] == 'G' || receiveData[1] == 'g')
            {
                clockConfiguration.ParseFromData(receiveData);
            }

        }

        public async void OnDeviceConnected(bool success, Java.Lang.Object device)
        {
            try
            {
                await SocketWriteAsync(FrameEncoder.GetInfoCommand);
                await Task.Delay(200);
                await ReadMessage();

                await SocketWriteAsync(FrameEncoder.SetTimeCommand, GetTimeData());
                await Task.Delay(200);
                await ReadMessage();
            }
            catch
            {
            }
        }

        private async Task ReadMessage()
        {
            byte[] buffer = new byte[1024];
            if (bluetoothSocket.InputStream.IsDataAvailable())
            {
                int bytes = await bluetoothSocket.InputStream.ReadAsync(buffer);
                handler.ObtainMessage(2, bytes, -1, buffer)
                        .SendToTarget();
            }
        }

        private void OnDeviceDiscoveried(object sender, BluetoothDevice device)
        {
            if (Devices.Any(x => x.Address == device.Address))
            {
                return;
            }
            Devices.Add(device);
            listviewAdapter.NotifyDataSetChanged();
        }

        #endregion

        private byte[] GetTimeData()
        {
            DateTime time = DateTime.Now;
            return new byte[]
            {
                (byte)time.Hour, (byte)time.Minute, (byte)time.Second,
                (byte)time.Day, (byte)time.Month, (byte)(time.Year - 2000)
            };
        }
    }

    public class ApplicationState
    {
        public string DeviceAddress { get; set; }
        public byte[] Password { get; set; }
    }
}