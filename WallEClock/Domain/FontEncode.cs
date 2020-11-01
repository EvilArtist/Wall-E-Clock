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

namespace WallEClock.Domain
{
    public class FontEncode
    {
        private readonly List<char> charEncode = new List<char>
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'Ấ', 'Â', 'Ậ', 'Á', 'À', 'Ả', 'Ã', 'Ạ', 
            'Ă', 'B', 'C', 'D', 'E', 'Ê', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'Ô', 'Ơ', 'P', 
            'Q', 'S', 'T', 'U', 'Ư', 'V', 'X', 'Y', 'Z', 'a', 'à', 'á', 'ả', 'ã', 'ạ', 'ă', 'ằ', 'ắ', 'ẳ', 
            'ẵ', 'ặ', 'â', 'ầ', 'ấ', 'ẩ', 'ẫ', 'ậ', 'b', 'c', 'd', 'đ', 'e', 'è', 'é', 'ẻ', 'ẽ', 'ẹ', 'ê', 
            'ề', 'ế', 'ể', 'ễ', 'ệ', 'f', 'g', 'h', 'i', 'ì', 'í', 'ỉ', 'ĩ', 'ị', 'j', 'k', 'l', 'm', 'n', 
            'o', 'ò', 'ó', 'ỏ', 'õ', 'ọ', 'ô', 'ồ', 'ố', 'ổ', 'ỗ', 'ộ', 'ơ', 'ờ', 'ớ', 'ở', 'ỡ', 'ợ', 'p', 
            'q', 'r', 's', 't', 'u', 'ù', 'ú', 'ủ', 'ũ', 'ụ', 'ư', 'ừ', 'ứ', 'ử', 'ữ', 'ự', 'v', 'w', 'x', 
            'y', 'ỳ', 'ý', 'ỷ', 'ỹ', 'ỵ', 'z', ' ', '/', ';', 'º', '♥', '!', '.', 'Ý'
        };
        public char GetChar(int charCode) => charEncode[charCode];
        public int GetIndex(char @char) => charEncode.FindIndex(x => x == @char) == -1 ? 255 : charEncode.FindIndex(x => x == @char);
    }
}