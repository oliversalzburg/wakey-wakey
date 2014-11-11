using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WakeyWakeyLib.Windows {
	internal static class NativeMethods {
		public const int HWND_BROADCAST = 0xffff;
		public const int SC_MONITORPOWER = 0xF170;
		public const int WM_SYSCOMMAND = 0x0112;
		public const int MONITOR_ON = -1;
		public const int MONITOR_OFF = 2;
		public const int MONITOR_STANBY = 1;

		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		public static extern IntPtr SendMessage( IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam );
		
		public const int MOUSEEVENTF_MOVE = 0x0001;

		[DllImport( "user32.dll" )]
		public static extern void mouse_event( Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo );
		
		[FlagsAttribute]
		public enum EXECUTION_STATE : uint {
			ES_SYSTEM_REQUIRED = 0x00000001,
			ES_DISPLAY_REQUIRED = 0x00000002,
			// Legacy flag, should not be used.
			// ES_USER_PRESENT   = 0x00000004,
			ES_AWAYMODE_REQUIRED = 0x00000040,
			ES_CONTINUOUS = 0x80000000,
		}
		[DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern EXECUTION_STATE SetThreadExecutionState( EXECUTION_STATE esFlags );

		public static void Wake() {
			
		}
	}
}
