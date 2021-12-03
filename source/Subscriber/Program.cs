using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
    .WithClientOptions(new MqttClientOptionsBuilder()
        .WithClientId("my-test-subscriber")
        .WithTcpServer("localhost")
    )
    .Build();

using IManagedMqttClient mqttClient = new MqttFactory().CreateManagedMqttClient();

JsonSerializerOptions serializerOptions = new() { WriteIndented = true };
mqttClient.UseApplicationMessageReceivedHandler(e =>
{
    string json = JsonSerializer.Serialize(e, serializerOptions);
    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

    lock (options)
    {
        Console.WriteLine(json);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(e.ApplicationMessage.Topic);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(payload);
        Console.ResetColor();
        Console.WriteLine();
    }
});

await mqttClient.SubscribeAsync("test/#");
await mqttClient.StartAsync(options);

Console.WriteLine("Running...any key to stop.");
Console.ReadKey();

await mqttClient.StopAsync();
