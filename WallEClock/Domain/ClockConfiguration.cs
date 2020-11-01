using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly int maxMessageLength = 65;
        private readonly int maxNameLength = 15;
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

        public ObservableCollection<Birthday> Birthdays { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public ClockConfiguration()
        {
            Birthdays = new ObservableCollection<Birthday>()
            {
                new Birthday{Enable = false},
                new Birthday{Enable = false},
                new Birthday{Enable = false},
                new Birthday{Enable = false},
                new Birthday{Enable = false},
            };
            Birthdays.CollectionChanged += Birthdays_CollectionChanged;
        }

        private void Birthdays_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Birthdays)));
        }

        private readonly FontEncode fontEncode = new FontEncode();
        public void ParseFromData(byte[] data)
        {
            try
            {
                int pointer = 3;
                ClockColor = new Color(data[pointer++], data[pointer++], data[pointer++]);
                var nightMode = data[pointer++];
                NightModeStartHour = data[pointer++];
                NightModeStartMinute = data[pointer++];
                NightModeEndHour = data[pointer++];
                NightModeEndMinute = data[pointer++];
                NightModeEnable = nightMode == 1;
                EffectEnable = data[pointer++] == 1;
                Hollidays = (Holliday)data[pointer++];
                int birthdayFlag = data[pointer++];
                byte[] message = new byte[maxMessageLength];
                for (int i = 0; i < maxMessageLength; i++)
                {
                    message[i] = data[pointer++];
                }
                DailyMessage = new string(message.Where(x => x < 255).Select(x => fontEncode.GetChar(x)).ToArray());
                for (int i = 0; i < Birthdays.Count; i++)
                {
                    if ((birthdayFlag & (1 << i)) > 0)
                    {
                        Birthdays[i].Enable = true;
                    }
                    Birthdays[i].Day = data[pointer++];
                    Birthdays[i].Month = data[pointer++];
                    byte[] name = new byte[maxNameLength];
                    for (int j = 0; j < maxNameLength; j++)
                    {
                        name[j] = data[pointer++];
                    }
                    Birthdays[i].Name = new string(name.Where(x => x < 255).Select(x => fontEncode.GetChar(x)).ToArray());
                }
            }
            catch {}
        }
    }

    public class Birthday: INotifyPropertyChanged
    {
        private bool enable;

        public bool Enable {
            get { return enable; }
            set {
                enable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enable)));
            }
        }

        private int day;

        public int Day {
            get { return day; }
            set {
                day = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Day)));
            }
        }

        private int month;

        public int Month {
            get { return month; }
            set { month = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Month)));
            }
        }

        private string name;

        public string Name {
            get { return name; }
            set {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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