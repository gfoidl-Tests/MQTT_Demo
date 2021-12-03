using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;

Console.WriteLine("Waiting for broker / server to be up...any key to continue");
Console.ReadKey();

using CancellationTokenSource cts = new();
MqttFactory factory = new();
using IMqttClient mqttClient = factory.CreateMqttClient();

var lastWillMessage = new MqttApplicationMessageBuilder()
    .WithTopic("gfoidl/test/publisher/status")
    .WithPayload("offline")
    .Build();

IMqttClientOptions options = new MqttClientOptionsBuilder()
    .WithClientId("gfoidl/my-test-publisher")
    //.WithTcpServer("localhost")
    .WithTcpServer("broker.hivemq.com")
    .WithWillMessage(lastWillMessage)
    .Build();

mqttClient.UseConnectedHandler(e =>
{
    Console.WriteLine("Connected");
});

mqttClient.UseDisconnectedHandler(async e =>
{
    if (cts.IsCancellationRequested) return;

    Console.WriteLine("Disconnected from server");

    await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);

    try
    {
        MqttClientConnectResult connectionResult = await mqttClient.ConnectAsync(options, cts.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Reconnecting failed: {0}", ex);
    }
});

MqttClientConnectResult connectionResult = await mqttClient.ConnectAsync(options, cts.Token);

try
{
    Task runTask = Run(cts.Token);

    Console.WriteLine("Running...hit Enter to stop");
    Console.ReadLine();
    cts.Cancel();

    await runTask;
    await mqttClient.DisconnectAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(ex.Message);
    Console.ResetColor();
}
//-----------------------------------------------------------------------------
async Task Run(CancellationToken cancellationToken)
{
    try
    {
        await Task.Yield();

        lastWillMessage.Payload = Encoding.UTF8.GetBytes("online");
        await mqttClient.PublishAsync(lastWillMessage);

        using PeriodicTimer ticker = new(TimeSpan.FromSeconds(2));

        await PublishAsync();

        while (await ticker.WaitForNextTickAsync(cancellationToken))
        {
            await PublishAsync();
        }

        async Task PublishAsync()
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("gfoidl/test/time")
                .WithPayload(DateTimeOffset.UtcNow.ToString("O"))
                //.WithRetainFlag(true)
                .WithAtLeastOnceQoS()
                .Build();

            MqttClientPublishResult publishResult = await mqttClient.PublishAsync(message, cancellationToken);
            Console.WriteLine($"Published message with {publishResult.ReasonCode} and ID {publishResult.PacketIdentifier} at {DateTimeOffset.Now}");
        }
    }
    catch (OperationCanceledException)
    {
    }
}
