using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace WallEClock
{
    public partial class MainActivity
    {
        private void Picker_Click(object sender, EventArgs e)
        {
            View view = sender as View;
            Color color = (view.Background as ColorDrawable).Color;
            clockConfiguration.ClockColor = color;
            textClock.SetTextColor(color);
            int index = RedefinedColor.GetColorIndex(color);
            var scrollView = FindViewById<HorizontalScrollView>(Resource.Id.scrollview1);
            
            if(index > 1)
            {
                var scroll = UinitConverter.Unit2Px(this, (index -1)* (RedefinedColor.PickerWidth + RedefinedColor.PickeMargin));
                scrollView.ScrollTo(scroll, 0);
            }

        }

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

        private void ButtonCancelNightMode_Click(object sender, EventArgs e)
        {
            nightModeSettingPage.LayoutGoToRight(150, 0);
            homePage.LayoutFadein(300, 150);
        }

        private void CardViewNightMode_Click(object sender, EventArgs e)
        {
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

        private void SwitchNightMode_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            clockConfiguration.NightModeEnable = (sender as SwitchCompat).Checked;
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


        private void TextDailyMessage_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle("Thông điệp hàng ngày");
            alert.SetView(LayoutInflater.Inflate(Resource.Layout.mesage_input_layout, null));
            EditText input = null;
            void onOKClick(object s, DialogClickEventArgs ev)
            {
                clockConfiguration.DailyMessage = input.Text;
            }
            alert.SetPositiveButton("Ok", onOKClick);
            var digalog = alert.Show();
            input = digalog.FindViewById<EditText>(Resource.Id.input_daily_message);
        }
        #endregion
    }
}