namespace XboxBatteryNotifier
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            using var app = new TrayApplication();

            Application.Run();
        }
    }
}