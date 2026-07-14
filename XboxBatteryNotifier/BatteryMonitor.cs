using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace XboxBatteryNotifier
{
    public class BatteryMonitor
    {
        // See https://www.bluetooth.com/specifications/assigned-numbers/ for the Bluetooth SIG assigned numbers for services and characteristics.
        private static readonly Guid BatteryServiceUuid =
            new("0000180F-0000-1000-8000-00805F9B34FB");

        private static readonly Guid BatteryLevelCharacteristicUuid =
            new("00002A19-0000-1000-8000-00805F9B34FB");


        public async Task<int?> GetBatteryAsync()
        {
            try
            {
                // Get the controller by filtering in devices
                var controller = (await DeviceInformation.FindAllAsync(
                        BluetoothLEDevice.GetDeviceSelectorFromPairingState(true)))
                    .FirstOrDefault(d =>
                        d.Name.Contains("Xbox", StringComparison.OrdinalIgnoreCase));
                if (controller is null) return null;

                // Connect to the controller
                using var bluetoothDevice =
                    await BluetoothLEDevice.FromIdAsync(controller.Id);
                if (bluetoothDevice is null ||
                    bluetoothDevice.ConnectionStatus != BluetoothConnectionStatus.Connected) return null;

                // Get the battery service
                var batteryService = (await bluetoothDevice.GetGattServicesAsync())
                    .Services
                    .FirstOrDefault(s => s.Uuid == BatteryServiceUuid);
                if (batteryService is null) return null;

                // Get the battery level characteristic
                var batteryCharacteristic = (await batteryService.GetCharacteristicsAsync())
                    .Characteristics
                    .FirstOrDefault(c => c.Uuid == BatteryLevelCharacteristicUuid);
                if (batteryCharacteristic is null) return null;

                // Read the battery level from buffer and convert
                var buffer = (await batteryCharacteristic.ReadValueAsync()).Value;

                using var reader = Windows.Storage.Streams.DataReader.FromBuffer(buffer);

                return reader.ReadByte();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with battery info : {ex.Message}");
                return null;
            }
        }
    }
}