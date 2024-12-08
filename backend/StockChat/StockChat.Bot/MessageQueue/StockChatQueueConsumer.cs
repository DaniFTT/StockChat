using MassTransit;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockChat.Bot.MessageQueue;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using StockChat.Domain.Enums;

public class StockChatQueueConsumer : IConsumer<StockChatQueueMessage>
{
    private readonly ILogger<StockChatQueueConsumer> _logger;
    private readonly HubConnection _hubConnection;

    public StockChatQueueConsumer(ILogger<StockChatQueueConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;

        var hubUrl = configuration.GetValue<string>("SignalR:ChatHubUrl")!;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = handler =>
                {
                    if (handler is HttpClientHandler clientHandler)
                        clientHandler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    
                    return handler;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.StartAsync().GetAwaiter().GetResult();
    }

    public async Task Consume(ConsumeContext<StockChatQueueMessage> context)
    {
        var stockMessage = context.Message;
        var userId = Guid.Parse(stockMessage.UserId);
        var chatId = Guid.Parse(stockMessage.ChatId);

        try
        {
            var stockPriceMessage = await FetchStockPrice(stockMessage.StockCode);
            if (string.IsNullOrEmpty(stockPriceMessage))
            {
                _logger.LogWarning($"Failed to fetch stock data for {stockMessage.StockCode}");
                await _hubConnection.InvokeAsync(
                    "SendStockBotMessage",
                    chatId,
                    userId,
                    $"Failed to fetch stock data for {stockMessage.StockCode}"
                );

                return;
            }

            await _hubConnection.InvokeAsync(
                "SendStockBotMessage",
                chatId,
                userId,
                stockPriceMessage
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            await _hubConnection.InvokeAsync(
                "SendStockBotMessage",
                chatId,
                userId,
                $"Failed to fetch stock data for {stockMessage.StockCode}"
            );
        }
    }

    private async Task<string?> FetchStockPrice(string stockCode)
    {
        using var httpClient = new HttpClient();
        var url = $"https://stooq.com/q/l/?s={stockCode}&f=sd2t2ohlcv&h&e=csv";

        try
        {
            var response = await httpClient.GetStringAsync(url);
            var lines = response.Split('\n');
            if (lines.Length > 1)
            {
                var data = lines[1].Split(',');
                var price = data[3];
                if (price is "N/D")
                    return $"Invalid stock code: {stockCode.ToUpper()}";

                return $"{stockCode.ToUpper()} quote is ${price} per share.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching stock data for: {stockCode.ToUpper()}");
        }

        return null;
    }
}
