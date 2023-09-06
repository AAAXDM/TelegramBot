using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace WebApplication1
{
    public class ConfigureWebhook : IHostedService
    {
        readonly ILogger<ConfigureWebhook> logger;
        readonly IServiceProvider serviceProvider;
        readonly IConfiguration configuration;

        public ConfigureWebhook(
            ILogger<ConfigureWebhook> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.configuration = configuration; 
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            var webhookAddress = $"{configuration["Url"]}api/message/update";

            logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
            UpdateType[] types = new UpdateType[] 
            { UpdateType.Message, UpdateType.MyChatMember, UpdateType.CallbackQuery, 
              UpdateType.ChosenInlineResult, UpdateType.ChatMember };

            await botClient.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: types,
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            logger.LogInformation("Removing webhook");
            await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}
