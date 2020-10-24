using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace WallEClock.Common
{
    public static class ViewExtensions
    {

        public static void LayoutCome(this View layout, long delay)
        {
            layout.Visibility = ViewStates.Visible;
            layout.TranslationY = layout.Height;
            ObjectAnimator animation = ObjectAnimator.OfFloat(layout, "translationY", 0f);
            animation.SetDuration(200);
            animation.StartDelay = delay;
            animation.SetInterpolator(new DecelerateInterpolator());
            animation.Start();
        }

        public static void LayoutComeFromRight(this View layout, long duration, long delay)
        {
            layout.Visibility = ViewStates.Visible;
            var width = layout.Width == 0 ? ((View)layout.Parent).Width : layout.Width;
            layout.TranslationX = width;
            ObjectAnimator animation = ObjectAnimator.OfFloat(layout, "translationX", 0f);
            animation.SetDuration(duration);
            animation.StartDelay = delay;
            animation.SetInterpolator(new DecelerateInterpolator());
            animation.Start();
        }

        public static void LayoutGoToLeft(this View layout, long duration, long delay)
        {
            layout.TranslationX = 0;
            ObjectAnimator animation = ObjectAnimator.OfFloat(layout, "translationX", -layout.Width);
            animation.SetDuration(duration);
            animation.StartDelay = delay;
            animation.SetInterpolator(new AccelerateInterpolator());
            animation.AnimationEnd += (x, y) => layout.Visibility = ViewStates.Gone;
            animation.Start();
        }

        public static void LayoutGoToRight(this View layout, long duration, long delay)
        {
            layout.TranslationX = 0;
            ObjectAnimator animation = ObjectAnimator.OfFloat(layout, "translationX", layout.Width);
            animation.SetDuration(duration);
            animation.StartDelay = delay;
            animation.SetInterpolator(new AccelerateInterpolator());
            animation.AnimationEnd += (x, y) => layout.Visibility = ViewStates.Gone;
            animation.Start();
        }

        public static void LayoutGone(this View layout, long delay)
        {
            layout.TranslationY = 0;
            ObjectAnimator animation = ObjectAnimator.OfFloat(layout, "translationY", layout.Height);
            animation.SetDuration(200);
            animation.StartDelay = delay;
            animation.SetInterpolator(new DecelerateInterpolator());
            animation.Start();
            animation.AnimationEnd += (x, y) => layout.Visibility = ViewStates.Gone;
        }

        public static void LayoutFadeout(this View layout, long duration, long delay)
        {
            layout.Alpha = 1f;
            layout.Animate()
                .Alpha(0f)
                .SetDuration(duration)
                .SetStartDelay(delay)
                .SetListener(
                    new EClockAnimatorListenerAdapter(layout, ViewStates.Gone)
                );
        }

        public static void LayoutFadein(this View layout, long duration, long delay)
        {
            layout.Alpha = 0f;
            layout.Visibility = ViewStates.Visible;
            layout.Animate()
                .Alpha(1f)
                .SetDuration(duration)
                .SetStartDelay(delay)
                .SetListener(
                    new EClockAnimatorListenerAdapter(layout, ViewStates.Visible)
                );
        }
    }

    public class EClockAnimatorListenerAdapter: AnimatorListenerAdapter
    {
        private readonly View layout;
        private readonly ViewStates viewState;

        public EClockAnimatorListenerAdapter(View layout, ViewStates viewState)
        {
            this.layout = layout;
            this.viewState = viewState;
        }
        public override void OnAnimationEnd(Animator animation)
        {
            layout.Visibility = viewState;
        }
    }
}