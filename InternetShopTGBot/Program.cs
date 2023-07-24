using InternetShopTGBot.Services;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("6034830309:AAEijfrNiuJ-A2s2lYUP7uls7zsfvqU7kcU");

var me = await botClient.GetMeAsync();

Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};


botClient.StartReceiving(
    updateHandler: TGHandlers.HandleUpdateAsync,
    pollingErrorHandler: TGHandlers.HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Console.WriteLine("Bot is running. Press Ctrl+C to exit.");

// Keep the main thread alive
await Task.Delay(-1);

cts.Cancel();

