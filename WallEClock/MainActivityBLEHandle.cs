using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using Java.IO;
using Java.Lang;
using Java.Util;
using WallEClock.Common;
using WallEClock.Domain;

namespace WallEClock
{
    public partial class MainActivity
    {
        private BluetoothLeScanner scanner;
        private ScanCallback BLEScanCallback;
        private BluetoothGatt bluetoothGatt;
        private readonly BLEGattCallback bleGattCallback = new BLEGattCallback();
        private BluetoothDeviceAdapter listviewAdapter;
 
        private BluetoothAdapter bluetoothAdapter;
        public ObservableCollection<BluetoothDevice> Devices { get; set; }
        private byte[] password = { 0, 0, 0, 0 };
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

                if (scanner is null)
                {
                    CreateBLEObject();
                }
            }
        }

        private void BleDeviceList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var device = Devices[e.Position];
            ConnectDevice(device.Address, true);
        }

        private async void InitializeDevice()
        {
            InitializeDeviceList(); 
            Java.IO.File file = new Java.IO.File(FilesDir, dataFile);
            if (!file.Exists())
            {
                homePage.LayoutFadeout(150, 0);
                deviceListPage.LayoutComeFromRight(300, 150);
                requestScan = true;
                RequestPermissions(new string[]{
                     Android.Manifest.Permission.AccessCoarseLocation,
                     Android.Manifest.Permission.Bluetooth,
                     Android.Manifest.Permission.BluetoothAdmin,
                 }, 1);
            }
            else
            {
                string address = string.Empty;
                using Stream fos = OpenFileInput(dataFile);
                using StreamReader reader = new StreamReader(fos);
                address = await reader.ReadLineAsync();
                if (!reader.EndOfStream)
                {
                    string passwordLine = await reader.ReadLineAsync();
                    password = passwordLine.Split('-').Select(x=> Convert.ToByte(x, 16)).ToArray();
                }
                if (!string.IsNullOrEmpty(address))
                {
                    ConnectDevice(address, false);
                }
            }
        }

        private void CreateBLEObject()
        {
            scanner = bluetoothAdapter.BluetoothLeScanner;
            bluetoothAdapter.Enable();
            Devices.Clear();
            BLEScanCallback = new BLEScanCallback((device) =>
            {
                if (Devices.Any(x => x.Address == device.Address))
                {
                    return;
                }
                Devices.Add(device);

                listviewAdapter.NotifyDataSetChanged();
            });
            foreach (var device in bluetoothAdapter.BondedDevices.Where(x => x.Name.Contains(BluetoothConst.BluetoothName)))
            {
                Devices.Add(device);
                listviewAdapter.NotifyDataSetChanged();
            }
        }

        private void ScanBLEDevice()
        {
            ScanFilter filter = new ScanFilter.Builder()
                .SetDeviceName(BluetoothConst.BluetoothName)
                .Build();

            List<ScanFilter> filters = new List<ScanFilter>()
            {
                filter
            };

            ScanSettings settings = new ScanSettings.Builder()
                                        .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                                        .Build();
            scanner.StartScan(filters, settings, BLEScanCallback);
            //scanner.StartScan(filters, null, BLEScanCallback);
        }

        private void StopScanning()
        {
            scanner.StopScan(BLEScanCallback);
        }

        private void ConnectDevice(string address, bool saveAddress)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            var device = bluetoothAdapter.GetRemoteDevice(address);
            CancellationToken token = source.Token;
            bleGattCallback.DataReceived = BLEDataReceived;
            bleGattCallback.OnDeviceConnected = async (s) =>
            {
                if (saveAddress)
                {
                    try
                    {
                        using Stream fos = OpenFileOutput(dataFile, FileCreationMode.Private);
                        using StreamWriter sw = new StreamWriter(fos);
                        await sw.WriteLineAsync(address);
                        await sw.WriteLineAsync(string.Join("-", password.Select(x=>$"{x:X2}")));
                    }
                    catch
                    {

                    }
                }
                //bluetoothGatt = device.ConnectGatt(this, false, bleGattCallback);
                //bluetoothGatt.Connect();
                await Task.Delay(200);
                bluetoothGatt.DiscoverServices();
                await Task.Delay(200);
                BluetoothGattService service = bluetoothGatt.GetService(BluetoothConst.UartServiceId);
                for (int i = 0; i < 7; i++)
                {
                    if (service == null)
                    {
                        bluetoothGatt.DiscoverServices();
                        await Task.Delay(200);
                        service = bluetoothGatt.GetService(BluetoothConst.UartServiceId);
                    }
                    else
                    {
                        break;
                    }
                }

                if (service == null)
                {
                    RunOnUiThread(() =>
                    {
                        Toast toast = Toast.MakeText(this, Resource.String.cannot_connect, ToastLength.Long);
                        toast.Show();
                    });
                    return;
                }
                EnableTXNotification();
                await Task.Delay(200);
                UartWrite(FrameEncoder.Encode(FrameEncoder.GetInfoCommand, password));
            };
            bluetoothGatt = device.ConnectGatt(this, true, bleGattCallback);
        }

        private void EnableTXNotification()
        {
            BluetoothGattService service = bluetoothGatt.GetService(BluetoothConst.UartServiceId);
            BluetoothGattCharacteristic TxChar = service.GetCharacteristic(BluetoothConst.TxCharId);
            bluetoothGatt.SetCharacteristicNotification(TxChar, true);

            BluetoothGattDescriptor descriptor = TxChar.GetDescriptor(BluetoothConst.CCCD);
            descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
            bluetoothGatt.WriteDescriptor(descriptor);
        }
        private void UartWrite(byte[] data)
        {
            BluetoothGattService service = bluetoothGatt.GetService(BluetoothConst.UartServiceId);
            BluetoothGattCharacteristic RxChar = service.GetCharacteristic(BluetoothConst.RxCharId);
            RxChar.SetValue(data);
            bluetoothGatt.WriteCharacteristic(RxChar);
        }

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
                Log.Debug("Data", string.Join("-", receiveData.Select(x => $"{x:X2}")));
            }

        }
    }
}