namespace XboxBatteryNotifier
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var (warningLevel, criticalLevel, intervalMinutes) = ParseArgs(args);

            var batteryMonitor = new BatteryMonitor();
            var controllerDetector = new ControllerDetector();
            var batteryAlertService = new BatteryAlertService(batteryMonitor, warningLevel, criticalLevel, TimeSpan.FromMinutes(intervalMinutes));

            controllerDetector.ControllerConnected += (name) =>
            {
                Console.WriteLine($"[{DateTime.Now:T}] {name} connectée");
                batteryAlertService.Start();
            };

            controllerDetector.ControllerDisconnected += (name) =>
            {
                Console.WriteLine($"[{DateTime.Now:T}] {name} déconnectée");
                batteryAlertService.Stop();
            };

            controllerDetector.Start();

            Console.WriteLine("Xbox Battery Notifier démarré. Ctrl+C pour quitter.");

            var exitSignal = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Arrêt en cours...");

                batteryAlertService.Dispose();
                exitSignal.Set();
            };

            exitSignal.Wait();
        }

        // This value are default for an official Xbox Wireless Controller
        // Using Philips Rechargeable AA HR6 RTU of 2600mAh
        // Expect the controller to last about 10 minutes after critical warning level (20%) and about 30 minutes after warning level (40%)
        private static (int warning, int critical, int intervalMinutes) ParseArgs(string[] args)
        {
            int warning = 40;
            int critical = 20;
            int intervalMinutes = 5;

            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--warning":
                        if (int.TryParse(args[i + 1], out var w)) warning = w;
                        break;
                    case "--critical":
                        if (int.TryParse(args[i + 1], out var c)) critical = c;
                        break;
                    case "--interval":
                        if (int.TryParse(args[i + 1], out var iv)) intervalMinutes = iv;
                        break;
                }
            }

            return (warning, critical, intervalMinutes);
        }
    }
}