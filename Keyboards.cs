using Telegram.Bot.Types.ReplyMarkups;

namespace WebApplication1
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup GetStartKeyboard()
        {
            return new(new[]
                         {
                          new KeyboardButton[] { "Узнать больше🧿" },
                          new KeyboardButton[] { "Задать вопрос📝" },
                          new KeyboardButton[] { "Хочу на игру🎯" }
                        })
            {
                ResizeKeyboard = true
            };
        }

        public static InlineKeyboardMarkup GetQuestionKeyboard()
        {
            return new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Как задавать вопрос⁉️","Как?")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Задать вопрос💬","Задать")
                }
            });
        }

        public static InlineKeyboardMarkup GetInlineKeyboard()
        {
            return new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("История ","0")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Автор ","1")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Обозначения","2")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Ход игры","3")
                }
            });
        }

        public static ReplyKeyboardMarkup GetBackKeyboard()
        {
            return new(new[]
                       {
                          new KeyboardButton[] { "Назад" },
                        })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetStartButton()
        {
            return new(new[]
                      {
                          new KeyboardButton[] { "✨🎲✨" },
                        })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetDiceRollButton()
        {
            return new(new[]
                      {
                          new KeyboardButton[] { "🌜Бросить додэкаэдр🌛" },
                        })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetEndKeyboard()
        {
            return new(new[]
                         {
                          new KeyboardButton[] { "Хочу на игру🎯" },
                          new KeyboardButton[] { "Получить личную карту Пилигрима📜" }
                        })
            {
                ResizeKeyboard = true
            };
        }
    }
}
