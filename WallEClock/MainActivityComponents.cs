using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private View birthdaySettingPage;

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

        private TextView textDailyMessage;
        private string DataFile => System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "applicationstate.json");

        private InputPickerPair[] birthdayResources;
        private ListView bleDeviceList;
        private bool requestScan = false;

        private void GetComponents()
        {
            applicationState = new ApplicationState();
            InitializeTextClockCard();

            homePage = FindViewById(Resource.Id.homepage);
            nightModeSettingPage = FindViewById(Resource.Id.layout_night_mode_setting);
            messagePage = FindViewById(Resource.Id.layout_message_setting);
            birthdaySettingPage = FindViewById(Resource.Id.layout_birthday_setting);

            CardView cardViewNightMode = FindViewById<CardView>(Resource.Id.materialCardViewNightMode);
            cardViewNightMode.Click += CardViewNightMode_Click;
            CardView cardViewMessage = FindViewById<CardView>(Resource.Id.materialCardViewMessage);
            cardViewMessage.Click += CardViewMessage_Click;

            CardView cardViewBirthday = FindViewById<CardView>(Resource.Id.materialCardViewBirthday);
            cardViewBirthday.Click += CardViewBirthday_Click;

            View materialCardViewLock = FindViewById(Resource.Id.materialCardViewLock);
            materialCardViewLock.Click += MaterialCardViewLock_Click;

            clockConfiguration = new ClockConfiguration()
            {
                ClockColor = new Android.Graphics.Color(255, 68, 0),

                NightModeEndHour = 6,
                NightModeEndMinute = 0,
                NightModeStartHour = 22,
                NightModeStartMinute = 0,
                Hollidays = Holliday.Chrismas | Holliday.DailyMessage | Holliday.LunarNewYear | Holliday.SolarNewYear
            };

            InitializeEffectSetting();
            InitializeMessageSetting();
            InitializeDevice();
            //textMessageInfo = FindViewById<TextView>(Resource.Id.messageInfo);

            clockConfiguration.PropertyChanged += ClockConfiguration_PropertyChanged;
            clockConfiguration.NightModeEnable = true;
            clockConfiguration.EffectEnable = true;

        }

        private void InitializeBirthdaysSetting()
        {
            Button backToHome = FindViewById<Button>(Resource.Id.buttonCancelBirthday);
            backToHome.Click += BackToHome_Click;
            birthdayResources = new InputPickerPair[]{
                new InputPickerPair { 
                    Input = Resource.Id.input_birthday1,
                    Button = Resource.Id.button_birthday_picker1, 
                    Index = 0,
                    Name = Resource.Id.input_birthday_name1,
                    Switch = Resource.Id.switch_birthday1
                },
                new InputPickerPair {
                    Input = Resource.Id.input_birthday2,
                    Button = Resource.Id.button_birthday_picker2,
                    Index = 1,
                    Name = Resource.Id.input_birthday_name2,
                    Switch = Resource.Id.switch_birthday2
                },
                new InputPickerPair {
                    Input = Resource.Id.input_birthday3,
                    Button = Resource.Id.button_birthday_picker3,
                    Index = 2,
                    Name = Resource.Id.input_birthday_name3,
                    Switch = Resource.Id.switch_birthday3
                },
                new InputPickerPair {
                    Input = Resource.Id.input_birthday4,
                    Button = Resource.Id.button_birthday_picker4,
                    Index = 3,
                    Name = Resource.Id.input_birthday_name4,
                    Switch = Resource.Id.switch_birthday4
                },
                new InputPickerPair {
                    Input = Resource.Id.input_birthday5,
                    Button = Resource.Id.button_birthday_picker5,
                    Index = 4,
                    Name = Resource.Id.input_birthday_name5,
                    Switch = Resource.Id.switch_birthday5
                },
            };
            foreach (var resource in birthdayResources)
            {
                SwitchCompat switchCompat = FindViewById<SwitchCompat>(resource.Switch);
                switchCompat.Checked = clockConfiguration.Birthdays[resource.Index].Enable;
                switchCompat.CheckedChange += (s, e) => clockConfiguration.Birthdays[resource.Index].Enable = switchCompat.Checked;
                EditText inputName = FindViewById<EditText>(resource.Name);
                inputName.Text = clockConfiguration.Birthdays[resource.Index].Name;
                inputName.TextChanged += (s, e) => clockConfiguration.Birthdays[resource.Index].Name = inputName.Text;
                EditText inputDay = FindViewById<EditText>(resource.Input);
                inputDay.Text = $"{clockConfiguration.Birthdays[resource.Index].Day:D2}/{clockConfiguration.Birthdays[resource.Index].Month:D2}";
                MaterialButton buttonClick1 = FindViewById<MaterialButton>(resource.Button);
                buttonClick1.Click += (s, e) =>
                {
                    DatePickerDialog diaglog = new DatePickerDialog(this);
                    diaglog.DatePicker.DateTime = new DateTime(DateTime.Now.Year, clockConfiguration.Birthdays[resource.Index].Month, clockConfiguration.Birthdays[resource.Index].Day);
                    diaglog.DateSet += (s1, e1) =>
                    {
                        clockConfiguration.Birthdays[resource.Index].Day = e1.Date.Day;
                        clockConfiguration.Birthdays[resource.Index].Month = e1.Date.Month;
                        inputDay.Text = $"{e1.Date:dd/MM}";
                    };
                    diaglog.Show();
                };
            }
           
        }

        private void InitializeMessageSetting()
        {
            var buttonCancelMessage = FindViewById<MaterialButton>(Resource.Id.buttonCancelMessage);
            buttonCancelMessage.Click += ButtonCancelMessage_Click;

            switchSolarNewyear = FindViewById<SwitchCompat>(Resource.Id.switch_solar_newyear);
            switchSolarNewyear.Click += SwitchSolarNewyear_Click; ;
            switchLunarNewyear = FindViewById<SwitchCompat>(Resource.Id.switch_lunar_newyear);
            switchLunarNewyear.Click += SwitchLunarNewyear_Click; ;
            switchXmas = FindViewById<SwitchCompat>(Resource.Id.switch_xmas);
            switchXmas.Click += SwitchXmas_Click; ;

            switchDaily = FindViewById<SwitchCompat>(Resource.Id.switch_daily);
            switchDaily.Click += SwitchDaily_Click; ;

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
            switchNightMode.CheckedChange += (s,e) => clockConfiguration.NightModeEnable = switchNightMode.Checked; ;

            nightModeStart = FindViewById<TextView>(Resource.Id.nightModeStart);
            nightModeStart.Text = $"{clockConfiguration.NightModeStartHour:D2}:{clockConfiguration.NightModeStartMinute:D2}";
            nightModeStart.Click += NightModeTime_Click;

            nightModeEnd = FindViewById<TextView>(Resource.Id.nightModeEnd);
            nightModeEnd.Text = $"{clockConfiguration.NightModeEndHour:D2}:{clockConfiguration.NightModeEndMinute:D2}";
            nightModeEnd.Click += NightModeTime_Click;

            var buttonCancelModeNightMode = FindViewById<MaterialButton>(Resource.Id.buttonCancelNightMode);
            buttonCancelModeNightMode.Click += ButtonCancelNightMode_Click; ;
        }

        private void InitializeEffectSetting()
        {
            chipSimpleEffect = FindViewById<Chip>(Resource.Id.chipSimpleEffect);
            chipRandomEffect = FindViewById<Chip>(Resource.Id.chipFullEffect);
            chipSimpleEffect.Click += (s, e) =>
            {
                clockConfiguration.EffectEnable = false;
                SetClockEffect();
            };
            chipRandomEffect.Click += (s, e) =>
            {
                clockConfiguration.EffectEnable = true;
                SetClockEffect();
            };
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
            switch (e.PropertyName)
            {
                case nameof(ClockConfiguration.ClockColor):
                    var color = clockConfiguration.ClockColor;
                    SetClockColorView(color);
                    break;
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
        #region UIUpdate
        private void SetClockColorView(Color color)
        {
            textClock.SetTextColor(color);
            int index = RedefinedColor.GetColorIndex(color);
            var scrollView = FindViewById<HorizontalScrollView>(Resource.Id.scrollview1);
            if (index > 1)
            {
                var scroll = UinitConverter.Unit2Px(this, (index - 1) * (RedefinedColor.PickerWidth + RedefinedColor.PickeMargin));
                scrollView.ScrollTo(scroll, 0);
            }
        }

        private void SetNighmode(ClockConfiguration config)
        {
            TextView view = FindViewById<TextView>(Resource.Id.nightmode_infor);
            if (config.NightModeEnable)
            {
                view.Text = $"{config.NightModeStartHour:D2}:{config.NightModeStartMinute:D2} - {config.NightModeEndHour:D2}:{config.NightModeEndMinute:D2}";
            }
            else
            {
                view.Text = Resources.GetString(Resource.String.off);
            }
        }
        #endregion
    }

    class InputPickerPair
    {
        public int Switch { get; set; }
        public int Input { get; set; }
        public int Button { get; set; }
        public int Index { get; set; }
        public int Name { get; set; }
    }
}