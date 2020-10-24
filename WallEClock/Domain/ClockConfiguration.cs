using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WallEClock.Domain
{
    public class ClockConfiguration : INotifyPropertyChanged
    {
        private bool nightModeEnable;

        public bool NightModeEnable {
            get { return nightModeEnable; }
            set { 
                nightModeEnable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NightModeEnable)));
            }
        }

        public int NightModeStartHour { get; set; }
        public int NightModeStartMinute { get; set; }
        public int NightModeEndHour { get; set; }
        public int NightModeEndMinute { get; set; }
        public Color ClockColor { get; set; }
        private bool effectEnable;

        public bool EffectEnable {
            get { return effectEnable; }
            set { 
                effectEnable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EffectEnable)));
            }
        }

        private Holliday hollidays;

        public Holliday Hollidays {
            get { return hollidays; }
            set {
                hollidays = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hollidays)));
            }
        }

        private string dailyMessage;

        public string DailyMessage {
            get { return dailyMessage; }
            set { 
                dailyMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DailyMessage)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ClockConfiguration()
        {

        }
    }

    [Flags]
    public enum Holliday
    {
        SolarNewYear = 1,
        LunarNewYear = 2, 
        Chrismas = 4,
        DailyMessage = 8,
    }
}