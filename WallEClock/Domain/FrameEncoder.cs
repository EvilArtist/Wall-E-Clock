using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Security;

namespace WallEClock.Domain
{
    public class FrameEncoder
    {
        public static readonly byte StartFrame = (byte)0xFE;
        public static readonly byte EndFrame = (byte)0xFD;
        public static readonly byte GetInfoCommand = (byte)'G';
        public static readonly byte SetTimeCommand = (byte)'T';
        public static readonly byte SetColorCommand = (byte)'C';
        public static readonly byte SetNightModeCommand = (byte)'N';
        public static readonly byte SetEffectCommand = (byte)'E';
        public static readonly byte SetPasswordCommand = (byte)'P';
        public static readonly byte SetNewYearCommand = (byte)'y';
        public static readonly byte SetXmasCommand = (byte)'x';
        public static readonly byte SetTetCommand = (byte)'l';
        public static readonly byte SetDailyMessageCommand = (byte)'D';
        public static readonly byte SetBirthdayCommand = (byte)'B';
        public static readonly byte PasswordIncorrect = (byte)'*';
        public static readonly byte Successed = (byte)'!';
        public static readonly byte Failed = (byte)'?';
        private const int passwordLength = 4;
        public static byte[] Encode(byte command, byte[] password, byte[] data)
        {
            if (password.Length != passwordLength)
            {
                throw new InvalidParameterException();
            }
            List<byte> encoded = new List<byte>
            {
                StartFrame,
                (byte)(data.Length + passwordLength + 1),
                command
            };
            encoded.AddRange(password);
            encoded.AddRange(data);
            encoded.Add(EndFrame);            
            return encoded.ToArray();
        }

        public static byte[] Encode(byte command, byte[] password)
        {
            return Encode(command, password, new byte[] { });
        }

        public static byte CalcCheckSum(byte[] data)
        {
            byte sum = 0;
            foreach (var @byte in data)
            {
                sum += @byte;
            }
            byte zero = 0;
            return (byte)(zero - sum);
        }
    }
}