using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using GameSchema.Schemas.Request;
using GameSchema.Schemas.Response;
using EntryServer.Services.Connection;
using System.Threading.Channels;

namespace EntryServer.Controllers.Connection;

[ApiController]
[Route("[controller]")]
public class ConnectionController(
    ILogger<ConnectionController> logger,
    ConnectionManager connectionManager
) : ControllerBase
{
    [HttpGet]
    public async Task GetTaskAsync()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var connectionId = HttpContext.TraceIdentifier;

            var responseChannel = Channel.CreateUnbounded<BaseResponsePacket>();
            var requestChannel = Channel.CreateUnbounded<BaseRequestPacket>();

            logger.LogInformation("Connection for {} opened", connectionId);

            await connectionManager.StartHandleAsync(connectionId, requestChannel, responseChannel);

            var requestHandlerTask = RequestHandler(webSocket, connectionId, requestChannel);
            var responseHandlerTask = ResponseHandler(webSocket, connectionId, responseChannel);

            await requestHandlerTask;

            await connectionManager.StopHandleAsync(connectionId);

            requestChannel.Writer.TryComplete();
            responseChannel.Writer.TryComplete();

            await responseHandlerTask;
            
            logger.LogInformation("Connection for {} closed", connectionId);
        }
        else
        {
            logger.LogWarning("Received non-WebSocket request to ConnectionController");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    async Task RequestHandler(WebSocket webSocket, string connectionId, Channel<BaseRequestPacket> requestChannel)
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var receivedMessage = new List<byte>();

            WebSocketReceiveResult result;
            do
            {
                var buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                receivedMessage.AddRange(buffer.Take(result.Count));
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                requestChannel.Writer.TryComplete();
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            else
            {
                try
                {
                    var message = JsonSerializer.Deserialize<BaseRequestPacket>(Encoding.UTF8.GetString([.. receivedMessage]));
                    if (message is not null)
                    {
                        logger.LogInformation("New message from {}", connectionId);
                        try
                        {
                            await requestChannel.Writer.WriteAsync(message);
                        }
                        catch (ChannelClosedException ex)
                        {
                            logger.LogError(ex, "Failed to write message to channel: {}", ex.Message);
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                            break;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize message: {}", ex.Message);
                }
                catch (NotSupportedException ex)
                {
                    logger.LogError(ex, "Unsupported message type: {}", ex.Message);
                }
            }
        }
        requestChannel.Writer.TryComplete();
    }
    static async Task ResponseHandler(WebSocket webSocket, string connectionId, Channel<BaseResponsePacket> responseChannel)
    {
        await foreach (var response in responseChannel.Reader.ReadAllAsync())
        {
            if (webSocket.State == WebSocketState.Closed)
            {
                break;
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            var bytes = Encoding.UTF8.GetBytes(jsonResponse);

            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        responseChannel.Writer.TryComplete();
    }
}