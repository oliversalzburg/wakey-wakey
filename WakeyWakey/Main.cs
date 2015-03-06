using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security;
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

        /// <summary>
        /// Allow the device to utilize the battery.
        /// </summary>
        private bool UseBattery = false;

        public Main( string[] args ) {
            ParseCommandLine( args );

            InitializeComponent();
            LoadIcons();

            notifyIcon.Icon = StatusIcons.Battery;
            notifyIcon.ContextMenuStrip = notifyIconStrip;

            notifyIcon.ShowBalloonTip( (int)TimeSpan.FromSeconds( 5 ).TotalMilliseconds, "Wakey Wakey ;)", "Monitoring power state…", ToolTipIcon.Info );
            try {
                WakeyWakeyLib.WakeyWakey.NoLockScreen();
            } catch( SecurityException ) {
                notifyIcon.ShowBalloonTip( (int)TimeSpan.FromSeconds( 5 ).TotalMilliseconds, "Elevated privileges required!", "Unable to disable lock screen. Restart as Administrator.", ToolTipIcon.Error );
            }

            WakeyWakeyLib.WakeyWakey.PowerStatusChanged += WakeyWakey_PowerStatusChanged;
            WakeyWakeyLib.WakeyWakey.PollBatteryStatus();
        }

        private void ParseCommandLine( string[] args ) {
            var p = new OptionSet() { { "b|battery", "Allow the device to use the battery.", v => UseBattery = v != null } };

            List<string> extra;
            try {
                extra = p.Parse( args );
            } catch( OptionException e ) {
                Console.Write( "wakey: " );
                Console.WriteLine( e.Message );
                Console.WriteLine( "Try `wakey --help' for more information." );
                return;
            }
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
                        if( UseBattery ) {
                            WakeyWakeyLib.WakeyWakey.DoWhateverTheFuckYouWant();
                            WakeyWakeyLib.WakeyWakey.DisplayOff();
                        } else {
                            WakeyWakeyLib.WakeyWakey.Shutdown();
                        }

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
