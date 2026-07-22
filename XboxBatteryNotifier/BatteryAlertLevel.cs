using Windows.Devices.Haptics;
using Windows.Gaming.Input;

namespace XboxBatteryNotifier
{
    public enum BatteryAlertLevel
    {
        Normal,
        WarningLevel,
        CriticalLevel
    }

    public class BatteryAlertService : IDisposable
    {
        #region Properties
        private readonly int _thresholdWarning;
        private readonly int _thresholdCritical;
        private readonly TimeSpan _pollingInterval;

        private readonly BatteryMonitor _batteryMonitor;
        private readonly Timer _timer;

        private BatteryAlertLevel _currentLevel = BatteryAlertLevel.Normal;
        #endregion

        private static Gamepad? _currentGamepad = Gamepad.Gamepads.FirstOrDefault();

        static BatteryAlertService()
        {
            Gamepad.GamepadAdded += (_, gamepad) => _currentGamepad = gamepad;
            Gamepad.GamepadRemoved += (_, gamepad) =>
            {
                if (_currentGamepad == gamepad) _currentGamepad = null;
            };
        }

        public BatteryAlertService(
            BatteryMonitor batteryMonitor, 
            int thresholdWarning, 
            int thresholdCritical, 
            TimeSpan pollingInterval)
        {
            _batteryMonitor = batteryMonitor;
            _thresholdWarning = thresholdWarning;
            _thresholdCritical = thresholdCritical;
            _pollingInterval = pollingInterval;

            _timer = new Timer(
                _ => _ = CheckAsync(),
                null,
                Timeout.Infinite,
                Timeout.Infinite
            );
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, _pollingInterval);
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

            if (level <= _thresholdCritical)
            {
                if (_currentLevel != BatteryAlertLevel.CriticalLevel)
                {
                    _currentLevel = BatteryAlertLevel.CriticalLevel;
                    NotifyCritical(level);
                }
            }
            else if (level <= _thresholdWarning)
            {
                if (_currentLevel != BatteryAlertLevel.WarningLevel && _currentLevel != BatteryAlertLevel.CriticalLevel)
                {
                    _currentLevel = BatteryAlertLevel.WarningLevel;
                    NotifyWarning(level);
                }
            }
            else
            {
                _currentLevel = BatteryAlertLevel.Normal;
            }
        }

        private static async Task RumbleAsync(int pulseCount, int pulseDuration, double intensity)
        {
            var controller = RawGameController.RawGameControllers.FirstOrDefault();
            var haptics = controller?.SimpleHapticsControllers.FirstOrDefault();

            if (haptics is null)
            {
                Console.WriteLine("[Rumble] Pas de retour haptique disponible sur ce contrôleur.");
                return;
            }

            var feedback = haptics.SupportedFeedback
                .FirstOrDefault(f => f.Waveform == KnownSimpleHapticsControllerWaveforms.Click)
                ?? haptics.SupportedFeedback.FirstOrDefault();

            if (feedback is null)
            {
                Console.WriteLine("[Rumble] Aucun waveform supporté trouvé.");
                return;
            }

            for (int i = 0; i < pulseCount; i++)
            {
                haptics.SendHapticFeedback(feedback, intensity);
                await Task.Delay(pulseDuration);

                haptics.StopFeedback();

                if (i < pulseCount - 1)
                    await Task.Delay(150);
            }
        }

    private static void NotifyWarning(int level)
        {
            PlayWarningTone();
            _ = RumbleAsync(pulseCount: 2, pulseDuration: 150, intensity: 1);
        }

        private static void NotifyCritical(int level)
        {
            PlayCriticalTone();
            _ = RumbleAsync(pulseCount: 3, pulseDuration: 300, intensity: 1);
        }

        private static void PlayWarningTone()
        {
            Console.Beep(880, 150);
            Console.Beep(880, 150);
        }

        private static void PlayCriticalTone()
        {
            Console.Beep(1046, 100);
            Console.Beep(880, 100);
            Console.Beep(1046, 100);
            Console.Beep(880, 100);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}