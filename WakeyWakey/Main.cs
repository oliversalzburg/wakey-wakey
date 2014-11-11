using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WakeyWakey {
	public partial class Main : Form {
		private const int WM_POWERBROADCAST = 0x218;
		private const int PBT_APMPOWERSTATUSCHANGE = 0xA;

		private class Icons {
			public Icon Battery { get; set; }
			public Icon Power { get; set; }
		}

		private Icons StatusIcons { get; set; }

		public Main() {
			InitializeComponent();
			LoadIcons();

			notifyIcon.Icon = StatusIcons.Battery;
			notifyIcon.ContextMenuStrip = notifyIconStrip;

			try {
				WakeyWakeyLib.WakeyWakey.NoLockScreen();
			} catch( SecurityException ) {
				notifyIcon.ShowBalloonTip( (int)TimeSpan.FromSeconds( 5 ).TotalMilliseconds, "Elevated privileges required", "Unable to disable lock screen. Restart as Administrator.", ToolTipIcon.Error );
			}

			WakeyWakeyLib.WakeyWakey.PollBatteryStatus();
			WakeyWakeyLib.WakeyWakey.PowerStatusChanged += WakeyWakey_PowerStatusChanged;
		}

		void WakeyWakey_PowerStatusChanged( object sender, EventArgs e ) {
			HandlePowerStatusChanged();
		}

		private void LoadIcons() {
			StatusIcons = new Icons();

			Assembly myAssembly = Assembly.GetExecutingAssembly();

			Stream streamBattery = myAssembly.GetManifestResourceStream( "WakeyWakey.Resources.battery.png" );
			Bitmap battery = new Bitmap( streamBattery );
			StatusIcons.Battery = Icon.FromHandle( battery.GetHicon() );

			Stream powerBattery = myAssembly.GetManifestResourceStream( "WakeyWakey.Resources.battery_plug.png" );
			Bitmap power = new Bitmap( powerBattery );
			StatusIcons.Power = Icon.FromHandle( power.GetHicon() );
		}

		protected override void WndProc( ref Message m ) {
			if( m.Msg == WM_POWERBROADCAST ) {
				if( m.WParam == (IntPtr)PBT_APMPOWERSTATUSCHANGE ) {
					HandlePowerStatusChanged();
				}
				return;
			}
			base.WndProc( ref m );
		}

		private void HandlePowerStatusChanged() {
			PowerLineStatus currentStatus = SystemInformation.PowerStatus.PowerLineStatus;

			switch( currentStatus ) {
				case PowerLineStatus.Offline:
					if( WakeyWakeyLib.WakeyWakey.State != WakeyWakeyLib.WakeyWakey.States.Indifferent ) {
						WakeyWakeyLib.WakeyWakey.DoWhateverTheFuckYouWant();

						notifyIcon.Icon = StatusIcons.Battery;
						notifyIcon.Text = "AC Offline";
						notifyIcon.ShowBalloonTip( (int)TimeSpan.FromSeconds( 5 ).TotalMilliseconds, "AC Offline", "System is now allowed to save power.", ToolTipIcon.Warning );
					}
					break;

				case PowerLineStatus.Online:
					if( WakeyWakeyLib.WakeyWakey.State != WakeyWakeyLib.WakeyWakey.States.ForcedAwake ) {
						WakeyWakeyLib.WakeyWakey.WakeTheFuckUp();

						notifyIcon.Icon = StatusIcons.Power;
						notifyIcon.Text = "AC Online";
						notifyIcon.ShowBalloonTip( (int)TimeSpan.FromSeconds( 5 ).TotalMilliseconds, "AC Online", "System was forced to wake up.", ToolTipIcon.Info );
					}
					break;
			}
		}

		private void exitToolStripMenuItem_Click( object sender, EventArgs e ) {
			Application.Exit();
		}
	}
}
