using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GuideMeCadastroTags.Interfaces;
using GuideMeCadastroTags.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(MessageAndroid))]
namespace GuideMeCadastroTags.Droid.Services
{
    
    public class MessageAndroid : IMessage
    {
        public void CustomAlert(string message, long time)
        {
            Toast mToastToShow = Toast.MakeText(Application.Context, message, ToastLength.Long);
            // Set the countdown to display the toast
            CountDown1 toastCountDown = new CountDown1(time,100,mToastToShow);
            toastCountDown.Start();

        }

        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }


    }
    public class CountDown1 : CountDownTimer
    {
        Toast _toast;
        public CountDown1(long mTimeLeftInMilli, long countDownInterva, Toast toast) : base(mTimeLeftInMilli, countDownInterva)
        {
            _toast = toast;
        }

        public override void OnFinish()
        {
            _toast.Cancel();
        }

        public override void OnTick(long millisUntilFinished)
        {
            _toast.Show();
        }

    }
}