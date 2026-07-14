namespace XboxBatteryNotifier
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var app = new TrayApplication();
            app.Start();

            Application.Run();
        }
    }
}