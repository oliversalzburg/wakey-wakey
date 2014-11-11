using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Management;
using System.Threading;
using WakeyWakeyLib.Windows;

namespace WakeyWakeyLib {
	public static class WakeyWakey {

		public enum States {
			Unknown = 0,
			Indifferent,
			ForcedAwake
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

	}
}
