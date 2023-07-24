using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace InternetShopTGBot.KeyBoards
{
    public static class KeyBoards
    {
        public static ReplyKeyboardMarkup productKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Додати товар", "Редагувати товар" },
                            new KeyboardButton[] { "Видалити товар", "Отримати всі товари" },
                            new KeyboardButton[] { "Назад" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup acceptAction = new(new[]
                        {
                            new KeyboardButton[] { "Скасувати замовлення", "Підтвердити замовлення" },
                        })
        {
            ResizeKeyboard = true
        };


        public static ReplyKeyboardMarkup orderKeyboard = new(new[]
                            {
                                new KeyboardButton[] { "Передивитися замовлення", "Скасувати/Підтвердити" },
                                new KeyboardButton[] { "Назад" },
                            })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup addProduct = new(new[]
                        {
                            new KeyboardButton[] { "Відправити" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup editProduct = new(new[]
                        {
                            new KeyboardButton[] { "Змінити" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup successFilter = new(new[]
                        {
                            new KeyboardButton[] { "Підтвердити фільтри" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup toFilters = new(new[]
                        {
                            new KeyboardButton[] { "Додати фільтри" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup nextItem = new(new[]
                        {
                            new KeyboardButton[] { "Далі" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup mainKeyboard = new(new[]
                            {
                            new KeyboardButton[] { "Товари", "Фільтр" },
                            new KeyboardButton[] { "Замовлення", "Блог" },
                            new KeyboardButton[] { "Вийти" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup cancelKeyboard = new(new[]
                            {
                            new KeyboardButton[] { "Перервати" },
                        })
        {
            ResizeKeyboard = true
        };


        public static ReplyKeyboardMarkup filterMenu = new(new[]
                        {
                            new KeyboardButton[] { "Додати фільтр", "Видалити фільтр" },
                            new KeyboardButton[] { "Показати ієрархію" },
                            new KeyboardButton[] { "Назад" },
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup addStory = new(new[]
                        {
                            new KeyboardButton[] { "Відправити сторіс" }
                        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup addPost = new(new[]
                        {
                            new KeyboardButton[] { "Відправити пост" }
                        })
        {
            ResizeKeyboard = true
        };


        public static ReplyKeyboardMarkup blogMain = new(new[]
                      {
                            new KeyboardButton[] { "Додати сторіс", "Видалити сторіс" },
                            new KeyboardButton[] { "Додати пост", "Видалити пост" },
                            new KeyboardButton[] { "Назад" },
                        })
        {
            ResizeKeyboard = true
        };

    }
}
