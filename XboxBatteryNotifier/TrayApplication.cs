namespace XboxBatteryNotifier
{
    public class TrayApplication : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly BatteryMonitor _batteryMonitor;
        private readonly ControllerDetector _controllerDetector;
        private readonly BatteryAlertService _batteryAlertService;

        public TrayApplication()
        {
            _batteryMonitor = new BatteryMonitor();
            _controllerDetector = new ControllerDetector();

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/icon.ico"),
                Visible = true,
                Text = "Xbox Battery Notifier"
            };

            _batteryAlertService = new BatteryAlertService(_batteryMonitor, _notifyIcon);

            SetMenu();
            SetControllerDetector();
        }

        private void SetMenu()
        {
            var menu = new ContextMenuStrip();

            var showBatDebug = new ToolStripMenuItem("Bat debug");
            showBatDebug.Click += (_, _) => CheckBattery();

            menu.Items.Add(showBatDebug);

            var quitItem = new ToolStripMenuItem("Quitter");
            quitItem.Click += (_, _) => Dispose();

            menu.Items.Add(quitItem);

            _notifyIcon.ContextMenuStrip = menu;
        }

        private void SetControllerDetector()
        {
            _controllerDetector.ControllerConnected += async (name) =>
            {
                var battery = await _batteryMonitor.GetBatteryAsync();

                if (battery.HasValue)
                {
                    _notifyIcon.ShowBalloonTip(
                        3000,
                        "Xbox Controller",
                        $"{name} is connected.\nBattery: {battery}%",
                        ToolTipIcon.Info
                    );
                }
                _batteryAlertService.Start();
            };

            _controllerDetector.ControllerDisconnected += (name) =>
            {
                _notifyIcon.ShowBalloonTip(
                    3000,
                    "Xbox Controller",
                    $"{name} disconnected",
                    ToolTipIcon.Info
                );
                _batteryAlertService.Stop();
            };

            _controllerDetector.Start();
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
            _batteryAlertService.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            Application.Exit();
        }
    }
}
