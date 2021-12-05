using Broker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore.Extensions;

await CreateHostBuilder(args).Build().RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseKestrel(kestrel =>
            {
                kestrel.ListenAnyIP(1883, l => l.UseMqtt());
            });

            // Instead of the StartUp we can use ConfigureServices like below.
            //webBuilder.UseStartup<Startup>();

            webBuilder.ConfigureServices(services =>
            {
                services
                    .AddHostedMqttServer(mqtt =>
                    {
                        mqtt.WithDefaultEndpoint();
                    })
                    .AddMqttConnectionHandler()
                    .AddConnections();
            });

            webBuilder.Configure(app =>
            {
                app.UseMqttServer(mqtt =>
                {
                    ClientConnectionHandler clientConnectionHandler = new();
                    mqtt.ClientConnectedHandler = clientConnectionHandler;
                    mqtt.ClientDisconnectedHandler = clientConnectionHandler;

                    // To handle last will
                    mqtt.ApplicationMessageReceivedHandler = clientConnectionHandler;
                });
            });
        });
