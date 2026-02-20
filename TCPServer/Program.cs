using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

namespace TCPServer
{
    public class RawSocketServer
    {
        private Socket _serverSocket;
        private readonly ConcurrentDictionary<int, Socket> _clients = new();
        private bool _isRunning;
        private int _clientIdCounter = 0;

        public static async Task Main()
        {
            var server = new RawSocketServer();
            await server.StartAsync(IPAddress.Any, 8080);
        }

        public async Task StartAsync(IPAddress address, int port)
        {
            // Create raw socket
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Configure socket options
                _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Bind to address and port
                _serverSocket.Bind(new IPEndPoint(address, port));

                // Start listening (backlog of 10)
                _serverSocket.Listen(10);
                _isRunning = true;

                Console.WriteLine($"Raw socket server listening on {address}:{port}");
                Console.WriteLine("Press 'q' to quit");

                // Start accepting connections
                _ = Task.Run(AcceptConnectionsAsync);

                // Wait for quit command
                while (Console.ReadKey().KeyChar != 'q') { }

                await StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }

        private async Task AcceptConnectionsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    // Accept incoming connection (blocking call)
                    var clientSocket = await _serverSocket.AcceptAsync();
                    var clientId = Interlocked.Increment(ref _clientIdCounter);

                    _clients[clientId] = clientSocket;

                    var clientEndPoint = clientSocket.RemoteEndPoint;
                    Console.WriteLine($"Client {clientId} connected from {clientEndPoint}");

                    // Handle client in background
                    _ = Task.Run(() => HandleClientAsync(clientId, clientSocket));
                }
                catch (ObjectDisposedException)
                {
                    // Server socket was closed
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Accept error: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(int clientId, Socket clientSocket)
        {
            var buffer = new byte[1024];

            try
            {
                while (clientSocket.Connected && _isRunning)
                {
                    // Receive data from client
                    var bytesReceived = await clientSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    SocketFlags.None);

                    if (bytesReceived == 0)
                    {
                        Console.WriteLine($"Client {clientId} disconnected gracefully");
                        break;
                    }

                    // Process received data
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine($"Client {clientId}: {message.Trim()}");

                    // Echo back with timestamp
                    var response = $"[{DateTime.Now:HH:mm:ss}] Echo: {message}";
                    var responseBytes = Encoding.UTF8.GetBytes(response);

                    await clientSocket.SendAsync(
                        new ArraySegment<byte>(responseBytes),
                        SocketFlags.None);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Client {clientId} socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client {clientId} error: {ex.Message}");
            }
            finally
            {
                // Cleanup
                _clients.TryRemove(clientId, out _);
                clientSocket.Close();
                Console.WriteLine($"Client {clientId} disconnected");
            }
        }

        public async Task StopAsync()
        {
            _isRunning = false;

            // Close all client connections
            foreach (var client in _clients.Values)
            {
                try
                {
                    client
    .Close();
                }
                catch { }
            }
            _clients.Clear();

            // Close server socket
            _serverSocket?.Close();
            Console.WriteLine("Server stopped");
        }
    }
}