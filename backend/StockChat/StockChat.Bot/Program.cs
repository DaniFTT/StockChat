using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using StockChat.Bot.Extensions;
using StockChat.Bot.Options;
using StockChat.Bot.MessageQueue;

namespace StockChat.Bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddRabbitMq(builder.Configuration);

            builder.Services.AddScoped<StockChatQueueConsumer>();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
