using System;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;

namespace KioskMode.Droid
{
    /// <summary>
    /// This class helps detecting when Home button is pressed and new apps are launched
    /// </summary>
    [Service]
    public class KioskService : Service
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Android.Content.Context _context;
        private bool _isFocus = true;
        private const string IsFocus = "is_focus";


        public override void OnDestroy()
        {

            Console.WriteLine("OnDestroy KioskService");

            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();

            base.OnDestroy();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags,
            int startId)
        {

            Console.WriteLine("STARTCOMMANDRESULT");
            _cancellationTokenSource = new CancellationTokenSource();
            _context = this;

            try
            {
                RunAsync(_cancellationTokenSource.Token);
            }
            catch { }

            return base.OnStartCommand(intent, flags, startId);
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                HandleKioskMode();
                await Task.Delay(2000, token);
            }
        }

        private void HandleKioskMode()
        {
            Console.WriteLine("HandleKiosMode");
            if (IsKioskModeActive(this) && IsInBackground())
            {
                RestoreApp();
            }
        }

        //Hack By Paul
        private bool IsInBackground()
        {

            var sp = PreferenceManager.GetDefaultSharedPreferences(_context);
            bool result = !sp.GetBoolean(IsFocus, false);

            Console.WriteLine("IsInBackground: " + result);
            return result;

            /* var am = _context.GetSystemService(ActivityService).JavaCast<ActivityManager>(); 
             var taskInfo = am.GetRunningTasks(2);
             var componentInfo = taskInfo[0].TopActivity;
             var componentInfo1 = taskInfo[1].TopActivity;

             Console.WriteLine("0: " + componentInfo.PackageName);
             Console.WriteLine("1: " + componentInfo1.PackageName);
             Console.WriteLine("isFocus: " + _isFocus); */


            /* if (_context.ApplicationContext.PackageName == componentInfo.PackageName)
             {
                 if (_isFocus)
                     return false;
                 else
                 {
                     _isFocus = true;
                     return true;
                 }
             }


             _isFocus = false;
             return true; */



            //return _context.ApplicationContext.PackageName != componentInfo.PackageName;
        }

        private void RestoreApp()
        {
            var intent = new Intent(_context, typeof(KioskModeActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            _context.StartActivity(intent);
        }

        private const string PrefKioskMode = "pref_kiosk_mode";
        private static bool IsKioskModeActive(Android.Content.Context context){
            var sp = PreferenceManager.GetDefaultSharedPreferences(context);
            return sp.GetBoolean(PrefKioskMode, false);
        }

        public override IBinder OnBind(Intent intent) { return null; }
    }
}
