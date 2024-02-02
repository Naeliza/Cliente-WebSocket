using System.Net.WebSockets;
using System.Text;

namespace WebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (ClientWebSocket clientWebSocket = new ClientWebSocket())
            {
                await clientWebSocket.ConnectAsync(new Uri("ws://localhost:9090"), CancellationToken.None);

                Console.WriteLine("Conectado al servidor WebSocket.");

                // Iniciar un bucle para recibir actualizaciones del servidor
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer.Array, 0, result.Count);
                        Console.WriteLine($"Actualización del servidor: {receivedMessage}");

                        await Task.Delay(4000); // Esperar 4 segundos antes de la próxima actualización
                    }
                });

                // Bucle principal para enviar solicitudes al servidor
                while (true)
                {
                    Console.Write("Ingrese el número del piso al que desea ir (1-10): ");
                    string input = Console.ReadLine();

                    if (input.ToLower() == "exit")
                        break;

                    if (int.TryParse(input, out int floor))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(input);
                        await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                        // No necesitamos esperar respuesta explícitamente aquí
                    }
                    else
                    {
                        Console.WriteLine("Entrada no válida. Por favor, ingrese un número de piso válido.");
                    }
                }

                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando conexión", CancellationToken.None);
            }
        }

    }
}
