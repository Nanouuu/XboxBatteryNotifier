namespace XboxBatteryNotifier
{
    internal class TrayApplication
    {
        public readonly NotifyIcon _trayIcon;

        public TrayApplication()
        {
            _trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Xbox Battery Notifier"
            };
        }
        
        internal void Start()
        {
            throw new NotImplementedException();
        }
    }
}
