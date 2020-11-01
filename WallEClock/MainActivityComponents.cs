using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Button;
using Android.Support.Design.Chip;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using WallEClock.Common;
using WallEClock.Domain;

namespace WallEClock
{
    public partial class MainActivity
    {
        private View homePage;
        private View nightModeSettingPage;
        private View messagePage;
        private View deviceListPage;

        private TextClock textClock;
        private TextView dateView;
        private LinearLayout clockColor;
        private SwitchCompat switchNightMode;
        private TextView nightModeStart;
        private TextView nightModeEnd;
        private ClockConfiguration clockConfiguration;
        private Chip chipSimpleEffect;
        private Chip chipRandomEffect;

        private SwitchCompat switchSolarNewyear;
        private SwitchCompat switchLunarNewyear;
        private SwitchCompat switchXmas;
        private SwitchCompat switchDaily;
        private readonly SwitchCompat switchBirthday;

        private TextView textMessageInfo;
        private TextView textDailyMessage;
        private string DataFile => System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "applicationstate.json");


        private ListView bleDeviceList;
        private bool requestScan = false;

        private void GetComponents()
        {
            applicationState = new ApplicationState();
            InitializeTextClockCard();

            homePage = FindViewById(Resource.Id.homepage);
            nightModeSettingPage = FindViewById(Resource.Id.layout_night_mode_setting);
            messagePage = FindViewById(Resource.Id.layout_message_setting);

            CardView cardViewNightMode = FindViewById<CardView>(Resource.Id.materialCardViewNightMode);
            cardViewNightMode.Click += CardViewNightMode_Click;
            CardView cardViewMessage = FindViewById<CardView>(Resource.Id.materialCardViewMessage);
            cardViewMessage.Click += CardViewMessage_Click;

            InitializeNightModeLayout();
            InitializeEffectSetting();
            InitializeMessageSetting();
            InitializeDevice();

            textMessageInfo = FindViewById<TextView>(Resource.Id.messageInfo);

            clockConfiguration = new ClockConfiguration()
            {
                ClockColor = new Android.Graphics.Color(255, 68, 0),

                NightModeEndHour = 6,
                NightModeEndMinute = 0,
                NightModeStartHour = 22,
                NightModeStartMinute = 0,
                Hollidays = Holliday.Chrismas | Holliday.DailyMessage | Holliday.LunarNewYear | Holliday.SolarNewYear
            };
            clockConfiguration.PropertyChanged += ClockConfiguration_PropertyChanged;
            clockConfiguration.NightModeEnable = true;
            clockConfiguration.EffectEnable = true;
        }

        private void InitializeMessageSetting()
        {
            var buttonCancelMessage = FindViewById<MaterialButton>(Resource.Id.buttonCancelMessage);
            buttonCancelMessage.Click += ButtonCancelMessage_Click;

            switchSolarNewyear = FindViewById<SwitchCompat>(Resource.Id.switch_solar_newyear);
            switchSolarNewyear.Click += (s, e) => clockConfiguration.Hollidays |= Holliday.SolarNewYear;
            switchLunarNewyear = FindViewById<SwitchCompat>(Resource.Id.switch_lunar_newyear);
            switchLunarNewyear.Click += (s, e) => clockConfiguration.Hollidays |= Holliday.LunarNewYear;
            switchXmas = FindViewById<SwitchCompat>(Resource.Id.switch_xmas);
            switchXmas.Click += (s, e) => clockConfiguration.Hollidays |= Holliday.Chrismas;

            switchDaily = FindViewById<SwitchCompat>(Resource.Id.switch_daily);
            switchDaily.Click += (s,e) => clockConfiguration.Hollidays |= Holliday.DailyMessage;

            textDailyMessage = FindViewById<TextView>(Resource.Id.daily_message);
            textDailyMessage.Click += TextDailyMessage_Click;
        }

        private void InitializeTextClockCard()
        {
            textClock = FindViewById<TextClock>(Resource.Id.clockView);
            clockColor = FindViewById<LinearLayout>(Resource.Id.clockColor);
            var colors = this.GetColorPicker();
            foreach (var picker in colors)
            {
                picker.Click += Picker_Click;
                clockColor.AddView(picker);
            }
            dateView = FindViewById<TextView>(Resource.Id.calendarView);
            dateView.Text = DateTime.Today.ToString("dd/MM/yyyy");
        }

        private void InitializeNightModeLayout()
        {
            switchNightMode = FindViewById<SwitchCompat>(Resource.Id.switch_night_mode);
            switchNightMode.CheckedChange += SwitchNightMode_CheckedChange;

            nightModeStart = FindViewById<TextView>(Resource.Id.nightModeStart);
            nightModeEnd = FindViewById<TextView>(Resource.Id.nightModeEnd);
            nightModeStart.Click += NightModeTime_Click;
            nightModeEnd.Click += NightModeTime_Click;
            var buttonCancelModeNightMode = FindViewById<MaterialButton>(Resource.Id.buttonCancelNightMode);
            buttonCancelModeNightMode.Click += ButtonCancelNightMode_Click; ;
        }

        private void InitializeEffectSetting()
        {
            chipSimpleEffect = FindViewById<Chip>(Resource.Id.chipSimpleEffect);
            chipRandomEffect = FindViewById<Chip>(Resource.Id.chipFullEffect);
            chipSimpleEffect.Click += (s, e) => clockConfiguration.EffectEnable = false;
            chipRandomEffect.Click += (s, e) => clockConfiguration.EffectEnable = true;

            int[][] states = new int[][] {
               
                new int[] {-Android.Resource.Attribute.StateActivated}, // unchecked
                new int[] { Android.Resource.Attribute.StateActivated }  // pressed
            };

            int colorIndex = GetColor(Resource.Color.primaryColor);
            Color color = Color.Rgb(Color.GetRedComponent(colorIndex), Color.GetGreenComponent(colorIndex), Color.GetBlueComponent(colorIndex));

            int[] colors = new int[] {
               
                Color.Gray,
                color
            };
            int[] textColors =
            {
                 Color.White,
                 Color.White,
            };
            ColorStateList backgroundColorList = new ColorStateList(states, colors);
            ColorStateList textColorList = new ColorStateList(states, textColors);
            chipSimpleEffect.ChipBackgroundColor = backgroundColorList;
            chipRandomEffect.ChipBackgroundColor = backgroundColorList;
            chipSimpleEffect.SetTextColor(textColorList);
            chipRandomEffect.SetTextColor(textColorList);
        }

        private void ClockConfiguration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var config = sender as ClockConfiguration;
            //TODO send data here
            switch (e.PropertyName)
            {
                case nameof(ClockConfiguration.NightModeEnable):
                    SetNighmode(config);
                    break;
                case nameof(ClockConfiguration.EffectEnable):
                    chipSimpleEffect.Activated = !clockConfiguration.EffectEnable;
                    chipRandomEffect.Activated = clockConfiguration.EffectEnable;
                    break;
                case nameof(ClockConfiguration.Hollidays):
                    
                    break;
                case nameof(ClockConfiguration.DailyMessage):
                    textDailyMessage.Text = clockConfiguration.DailyMessage;
                    break;
                default:
                    break;
            }
        }

        private void SetNighmode(ClockConfiguration config)
        {
            TextView view = FindViewById<TextView>(Resource.Id.nightmode_infor);
            nightModeStart.Text = $"{config.NightModeStartHour:D2}:{config.NightModeStartMinute:D2}";
            nightModeEnd.Text = $"{config.NightModeEndHour:D2}:{config.NightModeEndMinute:D2}";
            if (config.NightModeEnable)
            {
                view.Text = $"{config.NightModeStartHour:D2}:{config.NightModeStartMinute:D2} - {config.NightModeEndHour:D2}:{config.NightModeEndMinute:D2}";
            }
            else
            {
                view.Text = Resources.GetString(Resource.String.off);
            }
        }
    }
}