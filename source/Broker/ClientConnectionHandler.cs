using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Server;

namespace Broker;

public class ClientConnectionHandler :
    IMqttApplicationMessageReceivedHandler,
    IMqttServerClientConnectedHandler,
    IMqttServerClientDisconnectedHandler
{
    public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        if (e.ApplicationMessage.Topic == "test/publisher/status")
        {
            byte[] payload = e.ApplicationMessage.Payload;
            string msg = Encoding.UTF8.GetString(payload);
            Console.WriteLine($"Publisher: {msg}");
        }

        return Task.CompletedTask;
    }
    //-------------------------------------------------------------------------
    public Task HandleClientConnectedAsync(MqttServerClientConnectedEventArgs e)
    {
        if (e.ClientId == "my-test-publisher")
        {
            Console.WriteLine($"Publisher: connected from endpoint {e.Endpoint}");
        }

        return Task.CompletedTask;
    }
    //-------------------------------------------------------------------------
    public Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs e)
    {
        if (e.ClientId == "my-test-publisher")
        {
            Console.WriteLine($"Publisher: disconnected from endpoint {e.Endpoint}");
        }

        return Task.CompletedTask;
    }
}
