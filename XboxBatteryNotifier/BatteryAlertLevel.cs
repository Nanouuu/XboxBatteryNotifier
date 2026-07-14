using System.Media;
using System.Windows.Forms;

namespace XboxBatteryNotifier
{
    public enum BatteryAlertLevel
    {
        Normal,
        AlertedAt30,
        AlertedAt10
    }

    public class BatteryAlertService : IDisposable
    {
        private const int ThresholdWarning = 30;
        private const int ThresholdCritical = 10;
        private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(5);

        private readonly BatteryMonitor _batteryMonitor;
        private readonly NotifyIcon _notifyIcon;
        private readonly System.Threading.Timer _timer;

        private BatteryAlertLevel _currentLevel = BatteryAlertLevel.Normal;

        public BatteryAlertService(BatteryMonitor batteryMonitor, NotifyIcon notifyIcon)
        {
            _batteryMonitor = batteryMonitor;
            _notifyIcon = notifyIcon;

            _timer = new System.Threading.Timer(
                _ => _ = CheckAsync(),
                null,
                Timeout.Infinite,
                Timeout.Infinite
            );
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, PollingInterval);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _currentLevel = BatteryAlertLevel.Normal;
        }

        public async Task CheckAsync()
        {
            var battery = await _batteryMonitor.GetBatteryAsync();

            if (!battery.HasValue)
                return;

            int level = battery.Value;

            if (level <= ThresholdCritical)
            {
                if (_currentLevel != BatteryAlertLevel.AlertedAt10)
                {
                    _currentLevel = BatteryAlertLevel.AlertedAt10;
                    NotifyCritical(level);
                }
            }
            else if (level <= ThresholdWarning)
            {
                if (_currentLevel != BatteryAlertLevel.AlertedAt30 && _currentLevel != BatteryAlertLevel.AlertedAt10)
                {
                    _currentLevel = BatteryAlertLevel.AlertedAt30;
                    NotifyWarning(level);
                }
            }
            else
            {
                _currentLevel = BatteryAlertLevel.Normal;
            }
        }

        private void NotifyWarning(int level)
        {
            _notifyIcon.ShowBalloonTip(
                4000,
                "Manette Xbox",
                $"Batterie faible : {level}%",
                ToolTipIcon.Warning
            );

            SystemSounds.Exclamation.Play();
        }

        private void NotifyCritical(int level)
        {
            _notifyIcon.ShowBalloonTip(
                6000,
                "Manette Xbox",
                $"Batterie critique : {level}% — pense à la recharger !",
                ToolTipIcon.Error
            );

            SystemSounds.Hand.Play();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}