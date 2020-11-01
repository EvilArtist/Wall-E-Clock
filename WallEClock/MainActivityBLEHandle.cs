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
        private readonly byte[] password = { 0, 0, 0, 0 };
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
                    OnMessageReceived = HandleMessage,
                    OnDeviceConnected = DeviceConnected
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
            if (File.Exists(DataFile))
            {
                using var reader = new StreamReader(DataFile, true);
                var jsonstring = await reader.ReadToEndAsync();
                applicationState = JsonSerializer.Deserialize<ApplicationState>(jsonstring);
                if (!string.IsNullOrEmpty(applicationState.DeviceAddress))
                {
                    await ConnectDevice(applicationState.DeviceAddress, false);
                    return;
                }
            }

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
        }

        private void ScanDevice()
        {
            if (!bluetoothAdapter.IsEnabled)
            {
                bluetoothAdapter.Enable();
            }
            bluetoothAdapter.StartDiscovery();
        }


        private async void BleDeviceList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var device = Devices[e.Position];
            await ConnectDevice(device.Address, false); //TODO update here
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
                await bluetoothSocket.ConnectAsync();
                if (saveAddress)
                {
                    await SaveConfigAsync();
                }

                handler.ObtainMessage(ClockHandler.CONNECTING_STATUS, 1, -1, address)
                        .SendToTarget();
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

        private async Task SaveConfigAsync()
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(applicationState);
                await File.WriteAllTextAsync(DataFile, jsonString);
            }
            catch { }
        }

        #region BluetoothEvent 
        private void BLEDataReceived(byte[] receiveData)
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

        public void HandleMessage(byte[] data)
        {
            BLEDataReceived(data);
        }

        public async void DeviceConnected(bool success, Java.Lang.Object device)
        {
            await Task.Delay(100);
            await bluetoothSocket.OutputStream.WriteAsync(FrameEncoder.Encode(FrameEncoder.GetInfoCommand, password));
            await Task.Delay(5000);
            byte[] buffer = new byte[1024];

            try
            {
                if (bluetoothSocket.InputStream.IsDataAvailable())
                {
                    int bytes = await bluetoothSocket.InputStream.ReadAsync(buffer);
                    handler.ObtainMessage(2, bytes, -1, buffer)
                            .SendToTarget();
                    deviceListPage.LayoutGoToRight(150, 0);
                    homePage.LayoutFadein(300, 150);
                }
            }
            catch
            {
                // break;
            }
            ////thread.Start();
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
    }

    public class ApplicationState
    {
        public string DeviceAddress { get; set; }
        public string Password { get; set; }
    }
}