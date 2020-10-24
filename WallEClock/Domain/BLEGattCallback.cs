using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace WallEClock.Domain
{

    public delegate void NotifyDiscoveryCompleted();
    public delegate void OnDeviceConnected(State state);
    public delegate void DataReceived(byte[] receiveData);
    public class BLEGattCallback : BluetoothGattCallback
    {
        //private readonly IBleBroadcasrUpdate broadcast;
        public NotifyDiscoveryCompleted OnDiscoveryCompleted { get; set; }
        public DataReceived DataReceived { get; set; }
        public OnDeviceConnected OnDeviceConnected { get; set; }
        public State State { get; set; }
        public BLEGattCallback()
        {
            //this.broadcast = broadcastUpdate;
        }

        public BLEGattCallback(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt,
            [GeneratedEnum] GattStatus status,
            [GeneratedEnum] ProfileState newState)
        {
            //String intentAction = "";

            if (newState == ProfileState.Connected)
            {
                //intentAction = "CONNECTED";
                State = State.Connected;
                OnDeviceConnected?.Invoke(State);
            }
            else if (newState == ProfileState.Disconnected)
            {
                gatt.Close();
            }
            else
            {
                return;
            }
            //broadcast?.Update(intentAction);
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            if (status == GattStatus.Success)
            {
                OnDiscoveryCompleted?.Invoke();
                //broadcast?.Update(BluetoothLeService.ACTION_GATT_SERVICES_DISCOVERED);
            }
            else
            {
                Log.Debug("BLECallback", "onServicesDiscovered received: " + status);
            }
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt,
                                            BluetoothGattCharacteristic characteristic)
        {
            var data = characteristic.GetValue();
            DataReceived?.Invoke(data);
            //broadcast?.Update(BluetoothLeService.ACTION_DATA_AVAILABLE, characteristic);
        }

    }
}