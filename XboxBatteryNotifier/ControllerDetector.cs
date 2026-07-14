using System.Collections.Concurrent;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace XboxBatteryNotifier
{
    public class ControllerDetector
    {
        private readonly DeviceWatcher _watcher;
        private readonly ConcurrentDictionary<string, BluetoothLEDevice> _trackedDevices = new();

        public event Action<string>? ControllerConnected;
        public event Action<string>? ControllerDisconnected;

        public ControllerDetector()
        {
            string selector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
            _watcher = DeviceInformation.CreateWatcher(selector);
            _watcher.Added += OnDeviceAdded;
        }

        public void Start() => _watcher.Start();

        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation device)
        {
            if (!device.Name.Contains("Xbox", StringComparison.OrdinalIgnoreCase))
                return;

            if (_trackedDevices.ContainsKey(device.Id))
                return;

            var bleDevice = await BluetoothLEDevice.FromIdAsync(device.Id);
            if (bleDevice == null)
                return;

            _trackedDevices[device.Id] = bleDevice;

            bleDevice.ConnectionStatusChanged += (s, o) =>
            {
                if (s.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    ControllerConnected?.Invoke(s.Name);
                else
                    ControllerDisconnected?.Invoke(s.Name);
            };


            if (bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                ControllerConnected?.Invoke(bleDevice.Name);
        }
    }
}