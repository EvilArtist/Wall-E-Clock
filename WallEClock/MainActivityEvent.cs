using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using WallEClock.Common;
using WallEClock.Domain;

namespace WallEClock
{
    public partial class MainActivity
    {
        #region ColorPicker
        private async void Picker_Click(object sender, EventArgs e)
        {
            View view = sender as View;
            Color color = (view.Background as ColorDrawable).Color;
            clockConfiguration.ClockColor = color;
            await SocketWriteAsync(FrameEncoder.SetColorCommand, color.R, color.G, color.B);
            await ReadMessageAsync();
        }
        #endregion

        #region NightMode
        private void ButtonSaveNightMode_Click(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Parse(nightModeStart.Text);
            clockConfiguration.NightModeStartHour = dateTime.Hour;
            clockConfiguration.NightModeStartMinute = dateTime.Minute;
            clockConfiguration.NightModeEndHour = dateTime.Hour;
            clockConfiguration.NightModeEndMinute = dateTime.Minute;
            clockConfiguration.NightModeEnable = switchNightMode.Checked;
        }

        private async void ButtonCancelNightMode_Click(object sender, EventArgs e)
        {
            nightModeSettingPage.LayoutGoToRight(150, 0);
            homePage.LayoutFadein(300, 150);
            await SocketWriteAsync(FrameEncoder.SetNightModeCommand, clockConfiguration.GetNightModeData());
            await ReadMessageAsync();
        }

        private void CardViewNightMode_Click(object sender, EventArgs e)
        {
            InitializeNightModeLayout();
            homePage.LayoutFadeout(150, 0);
            nightModeSettingPage.LayoutComeFromRight(300, 150);
        }

        private void NightModeTime_Click(object sender, EventArgs e)
        {
            TextView result = sender as TextView;
            void onTimeSelect(object s, TimePickerDialog.TimeSetEventArgs e1)
            {
                if (result.Id == Resource.Id.nightModeStart)
                {
                    clockConfiguration.NightModeStartHour = e1.HourOfDay;
                    clockConfiguration.NightModeStartMinute = e1.Minute;
                }
                else if (result.Id == Resource.Id.nightModeEnd)
                {

                    clockConfiguration.NightModeEndHour = e1.HourOfDay;
                    clockConfiguration.NightModeEndMinute = e1.Minute;
                }
                result.Text = $"{e1.HourOfDay:D2}:{e1.Minute:D2}";
                SetNighmode(clockConfiguration);
            }
            string[] spliters = result.Text.Split(':');
            int hour = Convert.ToInt32(spliters[0]);
            int minute = Convert.ToInt32(spliters[1]);
            TimePickerDialog diaglog = new TimePickerDialog(this, onTimeSelect, hour, minute, true);
            diaglog.Show();
        }
        #endregion

        #region Effect
        private async void SetClockEffect()
        {
            await SocketWriteAsync(FrameEncoder.SetEffectCommand, clockConfiguration.EffectEnable ? (byte)0x01 : (byte)0x00);
            await ReadMessageAsync();
        }
        #endregion

        #region Message Setting
        private void CardViewMessage_Click(object sender, EventArgs e)
        {
            homePage.LayoutFadeout(150, 0);
            messagePage.LayoutComeFromRight(300, 150);
        }

        private void ButtonCancelMessage_Click(object sender, EventArgs e)
        {
            messagePage.LayoutGoToRight(150, 0);
            homePage.LayoutFadein(300, 150);
        }

        private async void SwitchSolarNewyear_Click(object sender, EventArgs e)
        {
            if (switchSolarNewyear.Checked)
            {
                clockConfiguration.Hollidays |= Holliday.SolarNewYear;
            }
            else
            {
                clockConfiguration.Hollidays &= ~Holliday.SolarNewYear;
            }
            if (bluetoothSocket.IsConnected)
            {
                await SocketWriteAsync(FrameEncoder.SetNewYearCommand, (byte)(switchSolarNewyear.Checked ? 0x01 : 0x00));
                await ReadMessageAsync();
            }
        }

        private async void SwitchLunarNewyear_Click(object sender, EventArgs e)
        {
            if (switchLunarNewyear.Checked)
            {
                clockConfiguration.Hollidays |= Holliday.LunarNewYear;
            }
            else
            {
                clockConfiguration.Hollidays &= ~Holliday.LunarNewYear;
            }
            if (bluetoothSocket.IsConnected)
            {
                await SocketWriteAsync(FrameEncoder.SetTetCommand, (byte)(switchLunarNewyear.Checked ? 0x01 : 0x00));
                await ReadMessageAsync();
            }
        }

        private async void SwitchXmas_Click(object sender, EventArgs e)
        {
            if (switchXmas.Checked)
            {
                clockConfiguration.Hollidays |= Holliday.Chrismas;
            }
            else
            {
                clockConfiguration.Hollidays &= ~Holliday.Chrismas;
            }
            if (bluetoothSocket.IsConnected)
            {
                await SocketWriteAsync(FrameEncoder.SetXmasCommand, (byte)(switchXmas.Checked ? 0x01 : 0x00));
                await ReadMessageAsync();
            }
        }

        private async void SwitchDaily_Click(object sender, EventArgs e)
        {
            if (switchDaily.Checked)
            {
                clockConfiguration.Hollidays |= Holliday.DailyMessage;
            }
            else
            {
                clockConfiguration.Hollidays &= ~Holliday.DailyMessage;
            }
            await SendDailySetting();
        }

        private async Task SendDailySetting()
        {
            if (bluetoothSocket.IsConnected)
            {
                FontEncode fontEncode = new FontEncode();
                byte enable = (clockConfiguration.Hollidays & Holliday.DailyMessage) == Holliday.DailyMessage ? (byte)0x01 : (byte)0x00;
                List<byte> message = clockConfiguration.DailyMessage.Select(x => (byte)fontEncode.GetIndex(x)).ToList();
                message.Insert(0, enable);
                await SocketWriteAsync(FrameEncoder.SetDailyMessageCommand, message.ToArray());
                await ReadMessageAsync();
            }
        }

        private void TextDailyMessage_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle("Thông điệp hàng ngày");
            alert.SetView(LayoutInflater.Inflate(Resource.Layout.mesage_input_layout, null));
            EditText input = null;
            async void onOKClick(object s, DialogClickEventArgs ev)
            {
                clockConfiguration.DailyMessage = input.Text;
                await SendDailySetting();
            }
            alert.SetPositiveButton("Lưu", onOKClick);
            var digalog = alert.Show();
            input = digalog.FindViewById<EditText>(Resource.Id.input_daily_message);
            input.Text = clockConfiguration.DailyMessage;
        }

        #endregion

        #region ScanPage
        private async void BleDeviceList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            deviceListPage.LayoutGoToRight(150, 0);
            homePage.LayoutFadein(300, 150);
            var device = Devices[e.Position];
            applicationState.DeviceAddress = device.Address;
            await ConnectDevice(device.Address, true);
        }
        #endregion

        #region 

        private void CardViewBirthday_Click(object sender, EventArgs e)
        {
            InitializeBirthdaysSetting();
            homePage.LayoutFadeout(150, 0);
            birthdaySettingPage.LayoutComeFromRight(300, 150);
        }

        private async void BackToHome_Click(object sender, EventArgs e)
        {
            birthdaySettingPage.LayoutGoToRight(150, 0);
            homePage.LayoutFadein(300, 150);
            await SendBirthDayToClock();
        }

        private async Task SendBirthDayToClock()
        {
            for (int i = 0; i < clockConfiguration.Birthdays.Count; i++)
            {
                var data = clockConfiguration.Birthdays[i].GetBirthDayData(i);
                await SocketWriteAsync(FrameEncoder.SetBirthdayCommand, data);
                await ReadMessageAsync();
            }
        }

        #endregion

        #region PasswordInput
        List<EditText> digitsEditText = new List<EditText>();
        private void MaterialCardViewLock_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle("Cài đặt mật khẩu");
            alert.SetView(LayoutInflater.Inflate(Resource.Layout.password_input, null));
            async void onOKClick(object s, DialogClickEventArgs ev)
            {
                Regex regex = new Regex("^\\d$");
                byte[] data = { applicationState.Password[0], applicationState.Password[1], applicationState.Password[2], applicationState.Password[3] } ;
                bool isPasswordCorrect = true;
                for (int i = 0; i < digitsEditText.Count; i++)
                {
                    if (regex.IsMatch(digitsEditText[i].Text))
                    {
                        data[i] = Convert.ToByte(digitsEditText[i].Text);
                    }
                    else
                    {
                        isPasswordCorrect = false;
                    }
                }
                if (isPasswordCorrect)
                {
                    await SocketWriteAsync(FrameEncoder.SetPasswordCommand, data);
                    await ReadMessageAsync();
                    applicationState.Password = data;
                    await SaveConfigAsync();
                }
                else
                {
                    ShowSnackbar(Resource.String.password_incorect_format);
                }
            }
            alert.SetPositiveButton("Lưu", onOKClick);
            var digalog = alert.Show();
            
            InitializePassword(digalog);
        }

        private void InitializePassword(AlertDialog dialog)
        {
           digitsEditText = new List<EditText>() {
                dialog.FindViewById<EditText>(Resource.Id.pin_first_edittext),
                dialog.FindViewById<EditText>(Resource.Id.pin_second_edittext),
                dialog.FindViewById<EditText>(Resource.Id.pin_third_edittext),
                dialog.FindViewById<EditText>(Resource.Id.pin_forth_edittext),
            };
            for (int i = 0; i < digitsEditText.Count; i++)
            {
                var currentInput = digitsEditText[i];
                var nextInput = digitsEditText[(i + 1) % digitsEditText.Count];
                currentInput.TextChanged += (s, e) =>
                {
                    if (currentInput.Text.Length > 1)
                    {
                        currentInput.Text = currentInput.Text.Substring(currentInput.Text.Length - 1, 1);
                        nextInput.Focusable = true;
                        nextInput.FocusableInTouchMode = true;
                        nextInput.RequestFocus();
                    }
                    else if (currentInput.Text.Length == 1)
                    {
                        nextInput.Focusable = true;
                        nextInput.FocusableInTouchMode = true;
                        nextInput.RequestFocus();
                    }
                };
            }
        }

        #endregion
    }
}