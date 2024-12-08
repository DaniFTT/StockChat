using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockChat.Bot.MessageQueue;
using StockChat.Bot.Options;

namespace StockChat.Bot.Extensions;

public static class ServicesExtensions
{
    public static void AddRabbitMq(this IServiceCollection services, ConfigurationManager config)
    {
        var options = config.GetSection("RabbitMq").Get<RabbitMqOptions>();

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<StockChatQueueConsumer>();
            cfg.UsingRabbitMq((context, config) =>
            {
                config.ReceiveEndpoint(options!.QueueName, e =>
                {
                    e.PrefetchCount = 1;

                    e.ConfigureConsumer<StockChatQueueConsumer>(context, ac =>
                    {
                        ac.ConsumerMessage<StockChatQueueMessage>();
                    });
                });

                config.Host(new Uri(options.Uri), h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
            });
        });
    }
}
