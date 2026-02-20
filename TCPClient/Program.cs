using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClient
{
    public class RawSocketClient
    {
        private Socket _clientSocket;
        private bool _isConnected;

        public static async Task Main()
        {
            await Task.Delay(2_000);
            var client = new RawSocketClient();
            await client.ConnectAndRunAsync("127.0.0.1", 8080);
        }

        public async Task ConnectAndRunAsync(string serverAddress, int port)
        {
            try
            {
                // Create raw socket
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to server
                var serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), port);
                await _clientSocket.ConnectAsync(serverEndPoint);
                _isConnected = true;

                Console.WriteLine($"Connected to server at {serverAddress}:{port}");
                Console.WriteLine("Type messages (or 'quit' to exit):");

                // Start receiving messages in background
                _ = Task.Run(ReceiveMessagesAsync);

                // Handle user input
                await HandleUserInputAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private async Task HandleUserInputAsync()
        {
            while (_isConnected)
            {
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                    break;

                try
                {
                    // Send message to server
                    var messageBytes = Encoding.UTF8.GetBytes(input);
                    await _clientSocket.SendAsync(
                        new ArraySegment<byte>(messageBytes),
                        SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send error: {ex.Message}");
                    break;
                }
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];

            try
            {
                while (_isConnected && _clientSocket.Connected)
                {
                    // Receive data from server
                    var bytesReceived = await _clientSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    SocketFlags.None);

                    if (bytesReceived == 0)
                    {
                        Console.WriteLine("Server disconnected");
                        _isConnected = false;
                        break;
                    }

                    // Display received message
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine($"Server: {message}");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Receive error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            _clientSocket?.Close();
            Console.WriteLine("Disconnected from server");
        }
    }
}