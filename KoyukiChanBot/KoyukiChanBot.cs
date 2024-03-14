using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.HostApp;

public class KoyukiChanBot {
    static void Main(string[] args) => ConfigureHostBuider(args).Build().Run();

    public static IHostBuilder ConfigureHostBuider(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureLogging(b => {
            b.AddConsole();
        })
        .ConfigureServices((hostContext, services) => {
            services.AddHostedService<DiscordBotService>();
        });
}

public class DiscordBotService : BackgroundService {
    private DiscordSocketClient _client;
    private readonly IConfiguration _configuration;

    private string Token => _configuration.GetValue<string>("DiscordToken");
    public DiscordBotService(IConfiguration configuration) {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = Discord.LogSeverity.Info });
        _client.Log += x => {
            Console.WriteLine($"{x.Message}, {x.Exception}");
            return Task.CompletedTask;
        };
        _client.MessageReceived += MessageReceived;
        await _client.LoginAsync(Discord.TokenType.Bot, Token);
        await _client.StartAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        await _client.StopAsync();
    }

    private async Task MessageReceived(SocketMessage arg) {
        if (!(arg is SocketUserMessage m) || m.Author.IsBot) {
            return;
        }

        await m.Channel.SendMessageAsync($"{m.Content} と言いましたね。");
    }
}