using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using System.Text;

namespace WebApplication1
{
    public class UpdateHandler 
    {
        readonly int sleepTime = 000;
        readonly ITelegramBotClient botClient;
        readonly ILogger<UpdateHandler> logger;
        DataContext dataContext;       

        public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, DataContext dataContext)
        {
            this.botClient = botClient;
            this.logger = logger;
            this.dataContext = dataContext;
        }

        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
           if(update.Type == UpdateType.MyChatMember)
            {
                AppUser user = dataContext.Users.Where(p => p.ChatId == update.MyChatMember.Chat.Id).FirstOrDefault();
                if (user != null)
                {
                    dataContext.Remove(user);
                    await dataContext.SaveChangesAsync();
                }
                return;
            }
            
            if (update.Type == UpdateType.Message)
            {               
                Message? message = update.Message;

                if (message == null) return;
                
                string? question = await CheckQuestion(message);
                if (question != null) message.Text = question;

                switch (message.Text)
                {
                    case "/start":
                        await StartCommunication(message);
                        break;
                    case "Узнать больше\U0001f9ff":
                        IReplyMarkup keyboard = Keyboards.GetInlineKeyboard();
                        await SendInlineKeyboard(message, keyboard, "выберите следующее:");
                        break;
                    case "Назад":
                        keyboard = Keyboards.GetStartKeyboard();
                        await SendInlineKeyboard(message, keyboard, "выберите следующее:");
                        break;
                    case "Задать вопрос📝":
                        var replyKeyboard = Keyboards.GetQuestionKeyboard();
                        await SendInlineKeyboard(message, replyKeyboard, "выберите следующее:");
                        break;
                    case "✨🎲✨":
                        await AnswerOrDice(message,true);
                        break;
                    case "🌜Бросить додэкаэдр🌛":
                        await AnswerOrDice(message, false);
                        Thread.Sleep(sleepTime);
                        await AnswerOrDice(message, true);
                        break;
                    case "Хочу на игру🎯":
                        await botClient.SendTextMessageAsync(message.Chat.Id, Messages.link, parseMode: ParseMode.Html);
                        break;
                    case "Получить личную карту Пилигрима📜":
                        string finalMessage = FinalMapMessage(message);
                        await botClient.SendTextMessageAsync(message.Chat.Id, finalMessage, parseMode: ParseMode.Html);
                        break;
                    case "?":
                        keyboard = Keyboards.GetStartButton();
                        await botClient.SendTextMessageAsync
                        (message.Chat.Id, Messages.letsStart, replyMarkup:keyboard, parseMode: ParseMode.Html);
                        break;
                }

                return;
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
               await BotOnCallbackQueryReceived(update.CallbackQuery, cancellationToken);
            }
            
            return;
        }

        async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string? callbackData = callbackQuery.Data;

            if (callbackData == null) return;

            IReplyMarkup keyboard;
            Chat chat = callbackQuery.Message.Chat;

            if (callbackData.Count() == 1)
            {
                int number;
                try
                {
                    number = int.Parse(callbackData);
                }
                catch
                {
                    throw new Exception("No number in callbackData");
                }

                keyboard = Keyboards.GetBackKeyboard();

                await botClient.SendTextMessageAsync
                    (chat.Id, Messages.aboutTambolia[number], replyMarkup: keyboard, parseMode: ParseMode.Html);
                return;
            }

            switch (callbackData)
            {
                case "Как?":
                    keyboard = Keyboards.GetBackKeyboard();
                    await botClient.SendTextMessageAsync
                    (chat.Id, Messages.aboutQuestion,replyMarkup: keyboard, parseMode: ParseMode.Html);
                    break;
                case "Задать": 
                    await botClient.SendTextMessageAsync
                    (chat.Id, Messages.question, parseMode: ParseMode.Html);
                    break;
            }
            return;
        }

        async Task StartCommunication(Message message)
        {
            Chat chat = message.Chat;
            var userId = message.From.Id;
            AppUser appUser = new AppUser(chat.Id, userId);

            await ISUsrInDB(message);

            if (!CheckUser(message))
            {
                await AddUserToDatabase(message, appUser);
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, Messages.messagesList[0], parseMode: ParseMode.Html);

            Thread.Sleep(sleepTime);
            await botClient.SendTextMessageAsync(message.Chat.Id, Messages.messagesList[1], parseMode: ParseMode.Html);

            Thread.Sleep(sleepTime);
            ReplyKeyboardMarkup replyKeyboard = Keyboards.GetStartKeyboard();

            await botClient.SendTextMessageAsync(message.Chat.Id, Messages.messagesList[2],
                  replyMarkup: replyKeyboard, parseMode: ParseMode.Html);

            return;
        }

        async Task SendInlineKeyboard(Message message, IReplyMarkup keyboard, string text)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, text,
                    replyMarkup: keyboard, parseMode: ParseMode.Html);
            return;
        }

        async Task AddUserToDatabase(Message message, AppUser appUser)
        {
            logger.LogInformation("Receive message type: {MessageType}", message.Type);           
            await dataContext.Users.AddAsync(appUser);
            await dataContext.SaveChangesAsync();            

            return ;
        }

        async Task AnswerOrDice(Message message, bool choise)
        {
            Chat chat = message.Chat;
            IReplyMarkup keyboard;
            int requestNumber = GetRequestNumber(message, out AppUser user);

            if (choise)
            {
                if (requestNumber == RollInfo.fourResultValue)
                {
                    user.ResetRequestNumber();
                    await dataContext.SaveChangesAsync();
                    keyboard = Keyboards.GetEndKeyboard();
                    Thread.Sleep(sleepTime);
                    await botClient.SendTextMessageAsync(chat.Id, Messages.lastMessages[0], parseMode: ParseMode.Html);
                    Thread.Sleep(sleepTime);
                    await botClient.SendTextMessageAsync
                        (chat.Id, Messages.lastMessages[1], replyMarkup: keyboard, parseMode: ParseMode.Html);
                }

                else
                {
                    if(requestNumber == 0)
                    {
                       user.ClearUnswers();
                        await dataContext.SaveChangesAsync();
                    }

                    keyboard = Keyboards.GetDiceRollButton();
                    await botClient.SendTextMessageAsync
                    (chat.Id, Messages.aboutDiceRolls[requestNumber], replyMarkup: keyboard, parseMode: ParseMode.Html);
                }

                return;
            }

            else
            {
                int result = RandomRoll(requestNumber);

                await botClient.SendTextMessageAsync
                    (chat.Id, Messages.unswers[requestNumber][result], parseMode: ParseMode.Html);
              
                user.SetUnswer(requestNumber, result);

                user.IncreaseRequestNumber();

                await dataContext.SaveChangesAsync();

                return;
            }
        }

        bool CheckUser(Message message)
        {   
            AppUser? user = dataContext.Users.Where(p => p.ChatId == message.Chat.Id).FirstOrDefault();

            if (user == null) return false;

            return true;
        }

        async Task ISUsrInDB(Message message)
        {
            AppUser? user = dataContext.Users.Where(p => p.Id == message.From.Id).FirstOrDefault();

            if(user != null)
            {
                user.ResetAppUser(message.Chat.Id);
                await dataContext.SaveChangesAsync();
            }

            return;
        }

        int RandomRoll(int rollnumber)
        {
            var random = new Random();
            int maxValue;

            if (rollnumber == RollInfo.fourResultRolls[0] || 
                rollnumber == RollInfo.fourResultRolls[RollInfo.fourResultRolls.Length - 1]) 
                        maxValue = RollInfo.fourResultValue;
            else maxValue = RollInfo.twelveResultValue;

            return random.Next(maxValue);
        }

        int GetRequestNumber(Message message, out AppUser? user)
        {
            Chat chat = message.Chat;
            user = dataContext.Users.Where(p => p.ChatId == chat.Id).FirstOrDefault();
            if (user == null) return 0;

            return user.RequestNumber;
        }

        string FinalMapMessage(Message message)
        {
            Chat chat = message.Chat;
            StringBuilder sb = new StringBuilder();
            AppUser? user = dataContext.Users.Where(p => p.ChatId == chat.Id).FirstOrDefault();

            if (user == null) throw new Exception("no user in database");

            List<int> unwsers = user.Unswers;

            string question = user.Question + "\r\n";

            sb.Append(question);

            for(int i = 0; i < unwsers.Count; i++)
            {
                string str = $"{i + 1} бросок- " + Messages.unswers[i][unwsers[i]] + "\r\n";
                sb.Append(str);
            }

            return sb.ToString();
        }

         async Task<string?> CheckQuestion(Message message)
        {
            string? str = message.Text;

            if (str == null) return null;
            if (str[str.Length-1] == '?')
            {
                AppUser? user = dataContext.Users.Where(p => p.Id == message.From.Id).FirstOrDefault();
                user.AddQuestion(str);
                await dataContext.SaveChangesAsync();
                return "?";
            }

            return null;
        }
    }
}
