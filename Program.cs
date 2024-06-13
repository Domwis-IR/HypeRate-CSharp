using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using HypeRate;
using HypeRate.EventArgs;

class Program
{
    static SerialPort? _serialPort;
    static async Task Main(string[] args)
    {
        try{
            // Initialize the serial port for Arduino communication
            _serialPort = new SerialPort("/dev/cu.usbmodem11201", 9600); // Replace with your Arduino's port
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            _serialPort.Open();
            Console.WriteLine("Successfully Opened Arduino Port");

            // Step 1: Initialize the HyperRate instance and set the API token
            var hypeRate = HypeRate.HypeRate.GetInstance();
            hypeRate.SetApiToken("mGZQK7sMnVCqShZ5xLfVIiUVZztZFS18xn1lMPykUPayIEVaEFTiW2cGhslqLNW7"); // Replace with your actual API token
            
            // Step 2: Subscribe to necessary events
            hypeRate.Connected += OnConnected;
            hypeRate.Disconnected += OnDisconnected;
            hypeRate.HeartbeatReceived += OnHeartbeatReceived;

            // Step 3: Start the library and connect to the device
            hypeRate.Start();
            await hypeRate.Connect();

            // Ensure WebSocket connection is established
            if (hypeRate.IsConnected)
            {
                Console.WriteLine("Connected to the HypeRate server.");

                // Step 4: Join the heartbeat channel with the device ID
                string deviceId = "7b4b04"; // Replace with your actual device ID
                Console.WriteLine($"Joining heartbeat channel for device ID: {deviceId}");
                await hypeRate.JoinHeartbeatChannel(deviceId, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("Failed to connect to the HypeRate server.");
                return;
            }


            // Step 5: Wait for user input to disconnect
            Console.WriteLine("Press 'Q' to disconnect...");
            while (Console.ReadKey(true).Key != ConsoleKey.Q)
            {
                // Continue running until 'Q' is pressed
            }

            // Step 6: Disconnect from the server
            await hypeRate.Disconnect();
            _serialPort.Close();
        }
        catch(Exception ex){
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static void OnConnected(object? sender, EventArgs e)
    {
        Console.WriteLine("Connected to HypeRate server.");
    }

    private static void OnDisconnected(object? sender, EventArgs e)
    {
        Console.WriteLine("Disconnected from HypeRate server.");
    }

    private static void OnHeartbeatReceived(object? sender, HeartbeatReceivedEventArgs e)
    {
        Console.WriteLine($"Heartbeat received from device {e.Device}: {e.Heartbeat} BPM");
        // Debugging info
        Console.WriteLine($"Timestamp: {DateTime.Now}");
        Console.WriteLine($"EventArgs: DeviceId={e.Device}, Heartbeat={e.Heartbeat}");
        SendPulseToArduino(e.Heartbeat);
    }

    private static void SendPulseToArduino(int heartbeat)
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.WriteLine(heartbeat.ToString());
        }
    }

    private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        string inData = sp.ReadExisting();
        Console.WriteLine("Data Received:");
        Console.WriteLine(inData);
    }
}
