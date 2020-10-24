using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class BluetoothDeviceAdapter : BaseAdapter<BluetoothDevice>
    {

        private readonly Activity activity;
        private readonly ObservableCollection<BluetoothDevice> bluetoothDevices;

        public BluetoothDeviceAdapter(Activity activity, ObservableCollection<BluetoothDevice> devices)
        {
            this.activity = activity;
            bluetoothDevices = devices;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
                view = activity.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);

            TextView text1 = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text1.Text = this[position].Name;

            TextView text2 = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            text2.Text = this[position].Address;

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count {
            get {
                return bluetoothDevices.Count;
            }
        }

        public override BluetoothDevice this[int position] => bluetoothDevices[position];
        public void AddDevice(BluetoothDevice device)
        {
            bluetoothDevices.Add(device);
            NotifyDataSetChanged();
        }
    }

    class BluetoothDeviceAdapterViewHolder : Java.Lang.Object
    {

        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }
}