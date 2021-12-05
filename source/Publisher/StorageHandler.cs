using System.Text.Json;
using MQTTnet.Extensions.ManagedClient;

namespace Publisher;

public class StorageHandler : IManagedMqttClientStorage
{
    private static readonly JsonSerializerOptions s_serializerOptions = new() { WriteIndented = true };
    //-------------------------------------------------------------------------
    public async Task SaveQueuedMessagesAsync(IList<ManagedMqttApplicationMessage> messages)
    {
        Console.WriteLine($"\tStoring {messages.Count} messages");

        if (messages.Count == 0)
        {
            if (File.Exists("data.json"))
            {
                File.Delete("data.json");
            }

            return;
        }

        using FileStream stream = File.Create("data.json");
        await JsonSerializer.SerializeAsync(stream, messages, options: s_serializerOptions);
    }
    //-------------------------------------------------------------------------
    public async Task<IList<ManagedMqttApplicationMessage>> LoadQueuedMessagesAsync()
    {
        if (!File.Exists("data.json")) return new List<ManagedMqttApplicationMessage>();

        using FileStream stream = File.OpenRead("data.json");
        return await JsonSerializer.DeserializeAsync<IList<ManagedMqttApplicationMessage>>(stream)
            ?? new List<ManagedMqttApplicationMessage>();
    }
}
