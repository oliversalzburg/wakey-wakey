using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Timers;
using WakeyWakeyLib.Windows;

namespace WakeyWakeyLib {
    public static class WakeyWakey {

        public enum States {
            Unknown = 0,
            Indifferent,
            ForcedAwake,
            ForcedShutdown
        }

        public static States State { get; private set; }

        public static event EventHandler PowerStatusChanged;

        public static UInt16 PreviousBatteryStatus { get; private set; }

        public static void PollBatteryStatus() {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler( CheckPowerState );
            aTimer.Interval = 5000;
            aTimer.Enabled = true;
        }

        private static void CheckPowerState( object source, ElapsedEventArgs e ) {
            System.Management.ObjectQuery query = new ObjectQuery( "Select * FROM Win32_Battery" );
            ManagementObjectSearcher searcher = new ManagementObjectSearcher( query );

            ManagementObjectCollection collection = searcher.Get();
            if( collection.Count == 0 ) {
                return;
            }

            foreach( ManagementObject mo in collection ) {
                UInt16 batteryStatus = (UInt16)mo[ "BatteryStatus" ];
                if( PreviousBatteryStatus != batteryStatus ) {
                    PowerStatusChanged( null, EventArgs.Empty );
                }

                PreviousBatteryStatus = batteryStatus;
            }
        }

        public static void DoWhateverTheFuckYouWant() {
            if( State == States.Indifferent ) return;

            NativeMethods.SetThreadExecutionState( NativeMethods.EXECUTION_STATE.ES_CONTINUOUS );
            State = States.Indifferent;
        }

        public static void WakeTheFuckUp() {
            if( State == States.ForcedAwake ) return;

            if( NativeMethods.SetThreadExecutionState(
                NativeMethods.EXECUTION_STATE.ES_CONTINUOUS |
                NativeMethods.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                NativeMethods.EXECUTION_STATE.ES_AWAYMODE_REQUIRED ) == 0 ) {
                NativeMethods.SetThreadExecutionState(
                NativeMethods.EXECUTION_STATE.ES_CONTINUOUS |
                NativeMethods.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED );
            }

            NativeMethods.mouse_event( NativeMethods.MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero );
            Thread.Sleep( 40 );
            NativeMethods.mouse_event( NativeMethods.MOUSEEVENTF_MOVE, 0, -1, 0, UIntPtr.Zero );

            State = States.ForcedAwake;
        }

        public static void NoLockScreen( bool disableLockScreen = true ) {
            RegistryKey hklm = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 );
            RegistryKey software = hklm.OpenSubKey( "SOFTWARE" );
            RegistryKey policies = software.OpenSubKey( "Policies" );
            RegistryKey microsoft = policies.OpenSubKey( "Microsoft" );
            RegistryKey windows = microsoft.OpenSubKey( "Windows", true );

            RegistryKey personalization = windows.CreateSubKey( "Personalization" );
            int keyValue = ( disableLockScreen ) ? 1 : 0;
            personalization.SetValue( "NoLockScreen", keyValue, RegistryValueKind.DWord );
        }

        public static void DisplayOff() {
            NativeMethods.SendMessage( (IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SYSCOMMAND, (IntPtr)NativeMethods.SC_MONITORPOWER, (IntPtr)NativeMethods.MONITOR_OFF );
        }

        public static bool Hibernate() {
            return NativeMethods.SetSuspendState( true, false, false );
        }

        public static bool Sleep() {
            return NativeMethods.SetSuspendState( false, false, false );
        }

        public static IntPtr SetWakeAt( DateTime dt ) {
            NativeMethods.TimerCompleteDelegate timerComplete = null;

            // read the manual for SetWaitableTimer to understand how this number is interpreted.
            long interval = dt.ToFileTimeUtc();
            IntPtr handle = NativeMethods.CreateWaitableTimer( IntPtr.Zero, true, "WaitableTimer" );
            NativeMethods.SetWaitableTimer( handle, ref interval, 0, timerComplete, IntPtr.Zero, true );
            return handle;
        }

        public static void Shutdown() {
            var processStartInfo = new ProcessStartInfo( "shutdown", "/s /t 0" );
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            Process.Start( processStartInfo );
        }

        public static bool EnableHibernate() {
            Process p = new Process();
            p.StartInfo.FileName = "powercfg.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = "/hibernate on"; // this might be different in other locales
            return p.Start();
        }
    }
}
