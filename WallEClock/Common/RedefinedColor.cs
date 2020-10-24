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
using Android.Views;
using Android.Widget;

namespace WallEClock.Common
{
    public static class RedefinedColor
    {
        public static readonly int PickerWidth = 30;
        public static readonly int PickeMargin = 5;
        private static readonly List<Color> colors = new List<Color>
        {
            Color.Rgb(255, 0, 0), Color.Rgb(255,67,0), Color.Rgb(255,128,0),   Color.Rgb(255, 255,0), Color.Rgb(128, 255, 0),
            Color.Rgb(0, 255, 0), Color.Rgb(0, 255, 128), Color.Rgb(0, 255, 255), Color.Rgb(0, 128, 255),
            Color.Rgb(0, 0, 255), Color.Rgb(128, 0, 255), Color.Rgb(255, 0, 255), Color.Rgb(255, 0, 128),
            Color.Rgb(255, 255, 255),
        };
        public static List<View> GetColorPicker(this Context context)
        {
            List<View> views = new List<View>();
            var pixels = UinitConverter.Unit2Px(context, PickerWidth);
            
            foreach (var color in colors)
            {
                var view = new View(context)
                {
                    LayoutParameters = new ViewGroup.MarginLayoutParams(pixels, pixels)
                    {
                        RightMargin = PickeMargin
                    }
                };
                view.SetBackgroundColor(color);
                view.RequestLayout();
                views.Add(view);
            }
            return views;
        }

        public static int GetColorIndex(Color color)
        {
            return colors.IndexOf(color);
        }
    }

    public static class UinitConverter
    {
        public static int Unit2Px(Context context, int dp)
        {
            float scale = context.Resources.DisplayMetrics.Density;
            int pixels = (int)(dp * scale + 0.5f);
            return pixels;
        }

        public static float Px2Unit(Context context, int px)
        {
            float scale = context.Resources.DisplayMetrics.Density;
            float dps =(float) (px - 0.5) / scale;
            return dps;
        }
    }
}