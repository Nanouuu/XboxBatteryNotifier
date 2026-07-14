namespace XboxBatteryNotifier
{
    public class TrayApplication : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly BatteryMonitor _batteryMonitor;

        public TrayApplication()
        {
            _batteryMonitor = new BatteryMonitor();

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Xbox Battery Notifier"
            };

            var menu = new ContextMenuStrip();

            var showBatDebug = new ToolStripMenuItem("Bat debug");
            showBatDebug.Click += (_, _) => CheckBattery();

            menu.Items.Add(showBatDebug);

            var quitItem = new ToolStripMenuItem("Quitter");
            quitItem.Click += (_, _) => Dispose();

            menu.Items.Add(quitItem);

            _notifyIcon.ContextMenuStrip = menu;
        }

        private async void CheckBattery()
        {
            var battery = await _batteryMonitor.GetBatteryAsync();

            if (battery.HasValue)
            {
                _notifyIcon.ShowBalloonTip(
                    3000,
                    "Manette Xbox",
                    $"Batterie : {battery.Value}%",
                    ToolTipIcon.Info
                );
            }
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            Application.Exit();
        }
    }
}
