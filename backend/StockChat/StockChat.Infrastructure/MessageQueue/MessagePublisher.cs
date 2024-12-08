using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace StockChat.Infrastructure.MessageQueue;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(TMessage message) where TMessage : class;
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;

    public MessagePublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<TMessage>(TMessage message)
        where TMessage : class
    {
        await _bus.Publish(message);
    }
}


