using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using InternetShopTGBot.NewFolder;
using Newtonsoft.Json;
using InternetShopTGBot.Modals;
using System.Threading;
using Microsoft.VisualBasic;
using System.Net.Http.Headers;

namespace InternetShopTGBot.Services
{
    public class TGHandlers
    {
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Message message = update.Message!;
            string messageText = String.IsNullOrEmpty(message.Text) ? "" : message.Text;

            var chatId = message.Chat.Id;

            PersonInfo? person = UserDataStatic.People.FirstOrDefault(x => x.UserId == chatId);

            ReplyKeyboardMarkup replyKeyboardMarkupCancel = new(new[]
           {
                new KeyboardButton[] { "Перервати" },
            })
            {
                ResizeKeyboard = true
            };


            if (person != null && messageText != "Перервати")
            {
                if (person.IsLogin)
                {
                    if (string.IsNullOrEmpty(person.Login))
                    {
                        person.Login = messageText;
                        Message msg = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "🔐 Ведіть Ваш пароль 🔐",
                                cancellationToken: cancellationToken);
                    }
                    else
                    {
                        person.Password = messageText;
                        person.IsLogin = false;

                        SendLogin(chatId, person);


                        if (string.IsNullOrEmpty(person.Token))
                        {
                            person.Login = String.Empty;
                            person.Password = String.Empty;

                            await botClient.SendTextMessageAsync(chatId: chatId,
                                text: "Не авторизовано. Авторизуватися заново /login",
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            //Авторизовано
                            await botClient.SendTextMessageAsync(chatId: chatId,
                                text: "Авторизовано /menu",
                                cancellationToken: cancellationToken);
                        }
                    }

                    return;
                }

                if (person.IsAddProduct)
                {
                    person = await AddProduct(chatId, botClient,
                        cancellationToken, person, message, replyKeyboardMarkupCancel, update);

                }

                if (person.Product != null && person.IsAddFilter && messageText != "Підтвердити фільтри")
                {
                    person.Product.Filters += " " + messageText;
                }

                if (person.IsGetAllProduct)
                {
                    if (!int.TryParse(messageText, out int a))
                    {
                        SendMessage(botClient, chatId, "Помилка введення сторінки (може бути лише число)",
                            KeyBoards.KeyBoards.productKeyboard, cancellationToken);
                    }
                    else
                    {
                        GetAllProducts(int.Parse(messageText), botClient, chatId, cancellationToken);
                        person.IsGetAllProduct = false;
                    }
                }

                if (person.IsDeleteProduct)
                {
                    if (!int.TryParse(messageText, out int a))
                    {
                        SendMessage(botClient, chatId, "Помилка введення сторінки (може бути лише число)",
                            KeyBoards.KeyBoards.productKeyboard, cancellationToken);
                    }
                    else
                    {
                        DeleteProduct(botClient, chatId, cancellationToken,
                            person, int.Parse(messageText));
                        person.IsDeleteProduct = false;
                    }
                }

                if (person.IsEditProduct)
                {
                    if (person.Product != null)
                    {
                        if (person.Product.Id != 0)
                        {
                            person = await EditProduct(chatId, botClient, cancellationToken, person,
                                message, replyKeyboardMarkupCancel, update);
                        }
                        else
                        {
                            if (int.TryParse(messageText, out int b))
                            {
                                int id = int.Parse(messageText);
                                person.Product.Id = id;
                                SendMessage(botClient, chatId, "Ведіть нову назву продукту",
                                    KeyBoards.KeyBoards.nextItem, cancellationToken);
                            }
                            else
                            {
                                SendMessage(botClient, chatId, "Ведіть номер продукту",
                                    replyKeyboardMarkupCancel, cancellationToken);
                            }
                        }
                    }
                }

                if (person.IsAcceptOrder)
                {
                    if (int.TryParse(messageText, out int b))
                    {
                        person.Accept = new AcceptModal
                        {
                            Id = int.Parse(messageText),
                            Accept = false
                        };

                        SendMessage(botClient, chatId, "Оберіть дію для цього замовлення",
                            KeyBoards.KeyBoards.acceptAction, cancellationToken);

                        person.IsAcceptOrder = false;
                    }
                    else
                    {
                        SendMessage(botClient, chatId, "Ведіть номер замовлення",
                            replyKeyboardMarkupCancel, cancellationToken);
                    }
                }

                if (person.IsAddFilterManually)
                {
                    if (int.TryParse(messageText, out int b))
                    {
                        SendMessage(botClient, chatId, "Ведіть назву фільтра",
                            replyKeyboardMarkupCancel, cancellationToken);

                        person.AddFilter = new AddFilterModal { ParentId = int.Parse(messageText) };
                    }
                    else
                    {
                        if(person.AddFilter != null && person.AddFilter.ParentId != 0)
                        {
                            person.AddFilter.Title = messageText;
                            person.IsAddFilterManually = false;
                            AddFilterManually(chatId, botClient, 
                                cancellationToken, person);
                        }
                        else
                        {
                            SendMessage(botClient, chatId, "Ведіть номер",
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                    }
                }

                if (person.IsDeleteFilter)
                {
                    if (int.TryParse(messageText, out int b))
                    {
                        int delId = int.Parse(messageText);

                        person.IsDeleteFilter = false;
                        DeleteFilterManually(chatId, botClient, 
                            cancellationToken, person, delId);
                    }
                    else
                    {
                        SendMessage(botClient, chatId, "Ведіть номер",
                            replyKeyboardMarkupCancel, cancellationToken);
                    }
                }

                if (person.IsAddStory)
                {
                    person = await AddStory(chatId, botClient,
                        cancellationToken, person, update, messageText);
                }

                if (person.IsAddPost)
                {
                    person = await AddPost(chatId, botClient,
                        cancellationToken, person, update);
                }

                if (person.IsDelPost)
                {
                    if (int.TryParse(messageText, out int b))
                    {
                        int delId = int.Parse(messageText);
                        DeletePost(botClient, chatId, cancellationToken,person, delId);
                        person.IsDelPost = false;
                    }
                    else
                    {
                        SendMessage(botClient, chatId, "Ведіть номер",
                            replyKeyboardMarkupCancel, cancellationToken);
                    }
                }

                if (person.IsDelStory)
                {
                    if (int.TryParse(messageText, out int b))
                    {
                        int delId = int.Parse(messageText);
                        DeleteStory(botClient, chatId, cancellationToken, person, delId);
                        person.IsDelStory = false;
                    }
                    else
                    {
                        SendMessage(botClient, chatId, "Ведіть номер",
                            replyKeyboardMarkupCancel, cancellationToken);
                    }
                }
            }

            if (person != null && messageText == "Перервати")
            {
                if (person != null)
                {
                    person.IsAddProduct = false;
                    person.IsEditProduct = false;
                    person.IsAddProductImages = false;
                    person.IsAddFilter = false;
                    person.IsAcceptOrder = false;
                    person.IsAddFilterManually = false;
                    person.IsAddStory = false;
                    person.IsAddPost = false;
                    person.IsDelPost = false;
                    person.IsDelStory = false;
                    person.Product = null!;

                }
            }
            bool isFlag = false;
            switch (messageText)
            {
                case "/start":
                case "/login":
                case "Війти знову":
                    {
                        isFlag = true;
                        if (person != null && UserDataStatic.People.Contains(person))
                        {
                            UserDataStatic.People.Remove(person);
                        }
                        person = new PersonInfo
                        {
                            IsLogin = false,
                            UserId = chatId,
                            Login = "",
                            Password = "",
                            Token = ""
                        };
                        UserDataStatic.People
                            .Add(person);

                        Message msg = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "🔐 Ведіть Ваш логін 🔐",
                            cancellationToken: cancellationToken);

                        person.IsLogin = true;
                        break;
                    }
                case "Авторизовано":
                case "/menu":
                case "Назад":
                    {
                        if (person != null)
                        {
                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                            new KeyboardButton[] { "Товари", "Фільтр" },
                            new KeyboardButton[] { "Замовлення", "Блог" },
                            new KeyboardButton[] { "Вийти" },
                        })
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Перед Вами головне меню",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    }
                case "Товари":
                    {
                        if (person != null)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Оберіть в пункті меню, що ви хочете робити з продуктами",
                                replyMarkup: KeyBoards.KeyBoards.productKeyboard,
                                cancellationToken: cancellationToken);
                        }
                        break;

                    }
                case "Отримати всі товари":
                    {
                        if (person != null)
                        {
                            person.IsGetAllProduct = true;
                            SendMessage(botClient, chatId, "Введіть номер сторінки (виводитися буде по 15 товарів;" +
                                "1 - перші 15 товарів; 2 - від 16 до 30 товара і тд)",
                                KeyBoards.KeyBoards.productKeyboard, cancellationToken);
                        }
                        break;
                    }
                case "Перервати":
                    {
                        if (person != null)
                        {
                            person.Product = new ProductInfo();
                            person.IsAddProduct = false;
                            person.IsAddProductImages = false;
                            person.IsAddFilter = false;
                            person.IsAcceptOrder = false;

                            var keyBoard = KeyBoards.KeyBoards.mainKeyboard;

                            SendMessage(botClient, chatId, "Оберіть пункт меню",
                                keyBoard, cancellationToken);
                        }

                        break;
                    }
                case "Додати товар":
                    {
                        if (person != null)
                        {
                            person.IsAddProduct = true;
                            person.Product = new ProductInfo();
                            SendMessage(botClient, chatId, "Введіть повну назву товару",
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                        break;
                    }
                case "Додати фільтри":
                    {
                        if (person != null)
                        {

                            //person.Product = new ProductInfo();
                            person.IsAddProduct = false;
                            person.IsAddProductImages = false;
                            person.IsAddFilter = true;

                            GetFilters(chatId, botClient,
                            cancellationToken, person, message, update);
                        }
                        break;
                    }
                case "Підтвердити фільтри":
                    {
                        if (person != null)
                        {
                            person.IsAddProduct = false;
                            person.IsAddProductImages = false;
                            person.IsAddFilter = false;

                            if (person.IsEditProduct)
                            {
                                SendMessage(botClient, chatId, "Товар сформовано! Натисність 'Редагувати'",
                                    KeyBoards.KeyBoards.editProduct, cancellationToken);
                            }
                            else
                            {
                                SendMessage(botClient, chatId, "Товар сформовано! Натисність 'Відправити'",
                                    KeyBoards.KeyBoards.addProduct, cancellationToken);
                            }
                            person.IsEditProduct = false;
                        }
                        break;
                    }
                case "Редагувати товар":
                    {
                        if (person != null)
                        {
                            person.IsEditProduct = true;
                            person.Product = new ProductInfo();
                            SendMessage(botClient, chatId, "Ведіть номер продукту",
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                        break;
                    }
                case "Відправити":
                    {
                        if (person != null)
                        {
                            SendProductOnTheServer(chatId, botClient, cancellationToken, person);
                            person.Product = new ProductInfo();
                        }
                        break;
                    }
                case "Змінити":
                    {
                        if (person != null)
                        {
                            EditProductOnTheServer(chatId, botClient, cancellationToken, person);
                            SendMessage(botClient, chatId, "Товар змінено!",
                                KeyBoards.KeyBoards.productKeyboard, cancellationToken);
                        }
                        break;
                    }
                case "Видалити товар":
                    {
                        if (person != null)
                        {
                            person.IsDeleteProduct = true;
                            SendMessage(botClient, chatId, "Введіть номер товару",
                                KeyBoards.KeyBoards.productKeyboard, cancellationToken);
                        }
                        break;
                    }
                case "Фільтр":
                    {
                        if (person != null)
                        {

                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { "Додати фільтр", "Видалити фільтр" },
                            new KeyboardButton[] { "Показати ієрархію" },
                            new KeyboardButton[] { "Назад" },
                        })
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Тут можна робити все з фільтрами",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    }
                case "Показати ієрархію":
                    {
                        GetAllFilters(chatId, botClient, cancellationToken);
                        break;
                    }
                case "Додати фільтр":
                    {
                        if(person != null)
                        {
                            GetFiltersMenu(chatId, botClient, cancellationToken);
                            person.IsAddFilterManually = true;
                        }
                        break;
                    }
                case "Видалити фільтр":
                    {
                        if(person != null)
                        {
                            person.IsDeleteFilter = true;
                            SendMessage(botClient, chatId, "Ведіть номер фільтра", 
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                        break;
                    }
                case "Передивитися замовлення":
                    {
                        if(person != null)
                            GetAllOrders(person, botClient, chatId, 
                                KeyBoards.KeyBoards.orderKeyboard, cancellationToken);
                        break;
                    }
                case "Скасувати/Підтвердити":
                    {
                        if(person != null)
                        {
                            person.IsAcceptOrder = true;
                            SendMessage(botClient, chatId, "Виберіть номер замовлення", 
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                        break;
                    }
                case "Скасувати замовлення":
                case "Підтвердити замовлення":
                    {
                        if(person != null && person.Accept != null)
                        {
                            if(messageText == "Скасувати замовлення")
                                person.Accept.Accept = false;
                            else if (messageText == "Підтвердити замовлення")
                                person.Accept.Accept = true;


                            person.IsAcceptOrder = false;

                            AcceptOrder(chatId, botClient, cancellationToken, person);
                        }
                        break;
                    }
                case "Замовлення":
                    {
                        if (person != null)
                        {

                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                                new KeyboardButton[] { "Передивитися замовлення", "Скасувати/Підтвердити" },
                                new KeyboardButton[] { "Назад" },
                            })
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Тут можна робити все із замовленнями",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    }
                case "Блог":
                    {
                        if (person != null)
                        {

                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { "Додати сторіс", "Видалити сторіс" },
                            new KeyboardButton[] { "Додати пост", "Видалити пост" },
                            new KeyboardButton[] { "Назад" },
                        })
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Тут можна робити все з блогом",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    }
                case "Додати сторіс":
                    {
                        if(person != null)
                        {
                            person.IsAddStory = true;
                            person.StoryModal = new AddStoryModal();

                            SendMessage(botClient, chatId, "Введіть тему сторісу",
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                        break;
                    }
                case "Відправити сторіс":
                    {
                        if(person != null)
                        {
                            SendStory(person, botClient, chatId, cancellationToken);
                            person.IsAddStory = false;
                            person.StoryModal = null;
                        }
                        break;
                    }
                case "Додати пост":
                    {
                        if (person != null)
                        {
                            person.IsAddPost = true;
                            person.AddPostModal = new AddPostModal();

                            SendMessage(botClient, chatId, "Додайте фотографію",
                                replyKeyboardMarkupCancel, cancellationToken);
                        }
                        break;
                    }
                case "Відправити пост":
                    {
                        if(person != null)
                        {
                            SendPost(chatId, botClient, 
                                cancellationToken, person);
                        }
                        break;
                    }
                case "Видалити сторіс":
                    {
                        if(person != null)
                        {
                            await GetAllStories( botClient, chatId, cancellationToken);
                            person.IsDelStory = true;
                        }
                        break;
                    }
                case "Видалити пост":
                    {
                        if (person != null)
                        {
                            await GetAllPosts(botClient, chatId, cancellationToken);
                            person.IsDelPost = true;
                        }
                        break;
                    }
                case "Вийти":
                    {
                        if (person != null)
                        {
                            isFlag = true;

                            person.Login = "";
                            person.Password = "";
                            person.Token = "";
                            person.IsLogin = false;

                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                            new KeyboardButton[] { "Війти знову" }
                        });



                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Вихід успішно здійснено! Щоб почати знову напишіть /start",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);


                            UserDataStatic.People
                            .Remove(person);
                            person = null;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


            if (person != null || isFlag)
            {
                if(person != null)
                {
                    int id = UserDataStatic.People.FindIndex(x => x.UserId == person.UserId);
                    UserDataStatic.People[id] = person;
                    isFlag = false;
                }
            }
            else
            {
                SendMessage(botClient, chatId, "Увійдіть в аккаунт. /login", 
                    KeyBoards.KeyBoards.mainKeyboard, cancellationToken);
            }
        }

        public static void DeletePost(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken, PersonInfo person, int delId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(new
                {
                    Id = delId
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/post/delete", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Успішно видалено!",
                        KeyBoards.KeyBoards.blogMain, cancellationToken);
                }
            }
        }


        public static void DeleteStory(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken, PersonInfo person, int delId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(new
                {
                    Id = delId
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/story/delete", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Успішно видалено!",
                        KeyBoards.KeyBoards.blogMain, cancellationToken);
                }
            }
        }


        public static async Task GetAllPosts(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                string path = UrlInfo.URL + "/images/";
                client.BaseAddress = new Uri(UrlInfo.URL);
                
                HttpResponseMessage response = client.GetAsync("/api/post/getallposts").Result;

                if (response.IsSuccessStatusCode)
                {
                    List<PostItem>? postItems = JsonConvert.DeserializeObject<List<PostItem>>
                        (response.Content.ReadAsStringAsync().Result);
                    if(postItems != null)
                    {
                        foreach (var post in postItems)
                        {
                            string newUrl = path + post.Image;
                            IAlbumInputMedia[] albumInputMedias = new IAlbumInputMedia[1];

                            albumInputMedias[0] = new InputMediaPhoto(
                                InputFile.FromUri(newUrl));
                            try
                            {
                                Message[] messages = await botClient.SendMediaGroupAsync(
                                chatId: chatId,
                                media: albumInputMedias,
                                cancellationToken: cancellationToken);


                                string messageText = $"Продукт номер: {post.Id}\n" +
                                $"*******";

                                int mediaGroupId = messages[0].MessageId;

                                // Відправка текстового повідомлення з вказанням ідентифікатора групи фотографій
                                await botClient.SendTextMessageAsync(chatId, messageText,
                                    replyToMessageId: mediaGroupId);
                            }
                            catch(Exception ex){}
                            finally
                            {
                                string messageText = $"Продукт номер: {post.Id}\n" +
                                $"Посилання на фотографію: {UrlInfo.URL + "/images/" + post.Image}\n" +
                                $"*******";

                                await botClient.SendTextMessageAsync(chatId, messageText);
                            }
                        }
                    }


                    SendMessage(botClient, chatId, "Оберіть номер", KeyBoards.KeyBoards.cancelKeyboard, 
                        cancellationToken);
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                    //SendMessage(botClient, chatId, "Успішно додано!",
                    //    KeyBoards.KeyBoards.blogMain, cancellationToken);
                }
            }
        }


        public static async Task GetAllStories(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                string path = UrlInfo.URL + "/images/";
                client.BaseAddress = new Uri(UrlInfo.URL);

                HttpResponseMessage response = client.GetAsync("/api/story/getallstories").Result;

                if (response.IsSuccessStatusCode)
                {
                    List<StoryModal>? postItems = JsonConvert.DeserializeObject<List<StoryModal>>
                        (response.Content.ReadAsStringAsync().Result);
                    if (postItems != null)
                    {
                        foreach (var story in postItems)
                        {
                            string newUrl = path + story.Image;
                            IAlbumInputMedia[] albumInputMedias = new IAlbumInputMedia[1];

                            albumInputMedias[0] = new InputMediaPhoto(
                                InputFile.FromUri(newUrl));
                            try
                            {
                                Message[] messages = await botClient.SendMediaGroupAsync(
                                chatId: chatId,
                                media: albumInputMedias,
                                cancellationToken: cancellationToken);


                                string messageText = $"Продукт номер: {story.Id}\n" +
                                $"*******";

                                int mediaGroupId = messages[0].MessageId;

                                // Відправка текстового повідомлення з вказанням ідентифікатора групи фотографій
                                await botClient.SendTextMessageAsync(chatId, messageText,
                                    replyToMessageId: mediaGroupId);
                            }
                            catch (Exception ex) { }
                            finally
                            {
                                string messageText = $"Продукт номер: {story.Id}\n" +
                                $"Пункт сторісів: {story.Title}\n" +
                                $"Посилання на фотографію: {UrlInfo.URL + "/images/" + story.Image}\n" +
                                $"*******";

                                await botClient.SendTextMessageAsync(chatId, messageText);
                            }
                        }
                    }


                    SendMessage(botClient, chatId, "Оберіть номер", KeyBoards.KeyBoards.cancelKeyboard,
                        cancellationToken);
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                    //SendMessage(botClient, chatId, "Успішно додано!",
                    //    KeyBoards.KeyBoards.blogMain, cancellationToken);
                }
            }
        }

        public static void SendStory(PersonInfo person, ITelegramBotClient botClient, long chatId, 
            CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(person.StoryModal);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/story/add", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Успішно додано!",
                        KeyBoards.KeyBoards.blogMain, cancellationToken);
                }
            }
        }

        public static async Task<PersonInfo> AddStory(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person, Update update, string messageText)
        {

            if(string.IsNullOrEmpty(person.StoryModal.Title))
            {
                person.StoryModal.Title = messageText;
                SendMessage(botClient, chatId, "Додайте фотографію",
                   KeyBoards.KeyBoards.cancelKeyboard, cancellationToken);
            }


            if (update != null && update.Message != null && update.Message.Document != null
               && person != null)
            {
                var fieldId = update.Message.Document.FileId;

                var fileInfo = await botClient.GetFileAsync(fieldId);

                string path = "https://api.telegram.org/file/bot6034830309:AAEijfrNiuJ-A2s2lYUP7uls7zsfvqU7kcU/" + fileInfo.FilePath;

                using HttpClient httpClient = new HttpClient();
                using HttpResponseMessage response = await httpClient.GetAsync(path);

                await using Stream fileStream = response.Content.ReadAsStream();

                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                string base64 = Convert.ToBase64String(bytes);

                person.StoryModal.Image = base64;

                SendMessage(botClient, chatId, "Фотографія додана!",
                    KeyBoards.KeyBoards.addStory, cancellationToken);

                person.IsAddStory = false;  

            }

            return person;
        }

        public static void SendPost(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(person.AddPostModal);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/post/add", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Успішно додано!",
                        KeyBoards.KeyBoards.blogMain, cancellationToken);
                }
            }
        }

        public static async Task<PersonInfo> AddPost(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person, Update update)
        {
            if (update != null && update.Message != null && update.Message.Document != null
               && person != null)
            {
                var fieldId = update.Message.Document.FileId;

                var fileInfo = await botClient.GetFileAsync(fieldId);

                string path = "https://api.telegram.org/file/bot6034830309:AAEijfrNiuJ-A2s2lYUP7uls7zsfvqU7kcU/" + fileInfo.FilePath;

                using HttpClient httpClient = new HttpClient();
                using HttpResponseMessage response = await httpClient.GetAsync(path);

                await using Stream fileStream = response.Content.ReadAsStream();

                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                string base64 = Convert.ToBase64String(bytes);

                person.AddPostModal.Image = base64;

                SendMessage(botClient, chatId, "Фотографія додана!",
                    KeyBoards.KeyBoards.addPost, cancellationToken);

                person.IsAddPost = false;

            }

            return person;
        }

        public static void AcceptOrder(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(person.Accept);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/order/accept", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Операцію здійснено!", 
                        KeyBoards.KeyBoards.orderKeyboard, cancellationToken);
                }
            }
        }

        public static void AddFilterManually(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(person.AddFilter);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/filter/add", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Операцію здійснено!",
                        KeyBoards.KeyBoards.filterMenu, cancellationToken);
                }
            }
        }

        public static void DeleteFilterManually(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person, int delId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(new
                    {
                        Id = delId
                    }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/filter/delete", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error: " + response.Content);
                }
                else
                {
                    SendMessage(botClient, chatId, "Операцію здійснено!",
                        KeyBoards.KeyBoards.filterMenu, cancellationToken);
                }
            }
        }

        public static void GetFiltersMenu (long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer");

                HttpResponseMessage response = client.GetAsync("/api/filter/getfiltermenu").Result;

                if (response.IsSuccessStatusCode)
                {
                    string line = "";
                    List<FilterItem>? filters = JsonConvert
                        .DeserializeObject<List<FilterItem>>(response.Content
                        .ReadAsStringAsync().Result);
                    if (filters != null)
                    {
                        for (int i = 0; i < filters.Count; i++)
                        {
                            line += "Номер: " +filters[i].Id + " & Назва: " +  filters[i].Title + "\n";
                        }
                    }

                    line += "Оберіть підходящий пункт меню (Запишіть номер)";
                    SendMessage(botClient, chatId, line, KeyBoards.KeyBoards.cancelKeyboard,
                        cancellationToken);
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }
        }

        public static void GetAllFilters(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer");

                HttpResponseMessage response = client.GetAsync("/api/filter/getgroupedfilters").Result;

                if (response.IsSuccessStatusCode)
                {
                    string line = "";
                    List<FilterTree>? filters = JsonConvert
                        .DeserializeObject<List<FilterTree>>(response.Content
                        .ReadAsStringAsync().Result);
                    if(filters != null)
                    {
                        for (int i = 0; i < filters.Count; i++)
                        {
                            line += filters[i].Key + ") " + filters[i].Name + "\n";

                            foreach (var filter in filters[i].Items)
                            {
                                line += $" * {filter.Id}) {filter.Title}\n";
                            }

                        }
                    }


                    SendMessage(botClient, chatId, line, KeyBoards.KeyBoards.filterMenu, 
                        cancellationToken);
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }
        }

        public static void SendLogin(long chatId, PersonInfo person)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);

                var requestBody = new
                {
                    email = person.Login,
                    password = person.Password
                };

                string json = JsonConvert.SerializeObject(requestBody);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/auth/login", content).Result;
                Console.WriteLine(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    TokenModal token = JsonConvert.DeserializeObject<TokenModal>(responseContent)!;
                    if (token != null)
                    {
                        int id = UserDataStatic.People.FindIndex(x => x.UserId == person.UserId);
                        UserDataStatic.People[id].Token = token.token;
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }
        }

        public static void GetFilters(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person, Message message, Update update)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);

                HttpResponseMessage response = client.GetAsync("/api/filter/getallfilters").Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    List<FilterItem> filters = JsonConvert.DeserializeObject
                        <List<FilterItem>>(responseContent)!;
                    if (filters != null)
                    {
                        string filterString = "";
                        foreach (var filter in filters)
                        {
                            filterString += $"Номер: {filter.Id}; Фільтр: {filter.Title}\n";
                        }

                        SendMessage(botClient, chatId, "Прив'яжіть товар до фільтра\n" + filterString,
                            KeyBoards.KeyBoards.successFilter, cancellationToken);
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }
        }
        public static void SendProductOnTheServer(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person)
        {
            string personMessage = $"*Товар*\n{person.Product.Title}\n{person.Product.Description}\n" +
                $"{person.Product.Brand}\n{person.Product.Price}\n{person.Product.Filters}";
            SendMessage(botClient, chatId, personMessage, KeyBoards.KeyBoards.productKeyboard, cancellationToken);




            AddProductModal addProduct = new AddProductModal
            {
                Brand = person.Product.Brand,
                Title = person.Product.Title,
                Price = person.Product.Price,
                Description = person.Product.Description,
                Count = person.Product.Count,
            };


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(addProduct);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/product/add", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    ProductId result = JsonConvert.DeserializeObject<ProductId>(responseContent)!;
                    if (result != null)
                    {
                        foreach (var image in person.Product.Images)
                        {
                            ProductImage productImage = new ProductImage
                            {
                                ProductId = result.Id,
                                ImageBase64 = image
                            };

                            json = JsonConvert.SerializeObject(productImage);

                            content = new StringContent(json, Encoding.UTF8, "application/json");

                            response = client.PostAsync("/api/product/addimage", content).Result;
                        }


                        List<int> filtersId = person.Product.Filters.Split(' ').Where(x => !string.IsNullOrEmpty(x))
                            .Select(x => int.Parse(x)).ToList();


                        foreach (var filterId in filtersId)
                        {
                            ProductFilter productFilter = new ProductFilter
                            {
                                FilterId = filterId,
                                ProductId = result.Id
                            };

                            json = JsonConvert.SerializeObject(productFilter);

                            content = new StringContent(json, Encoding.UTF8, "application/json");

                            response = client.PostAsync("/api/product/tofilter", content).Result;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }

        }

        public static void EditProductOnTheServer(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person)
        {
            string personMessage = $"*Товар*\n{person.Product.Title}\n{person.Product.Description}\n" +
                $"{person.Product.Brand}\n{person.Product.Price}\n{person.Product.Filters}";
            SendMessage(botClient, chatId, personMessage, KeyBoards.KeyBoards.productKeyboard, cancellationToken);




            EditProductModal addProduct = new EditProductModal
            {
                Id = person.Product.Id,
                Brand = person.Product.Brand,
                Title = person.Product.Title,
                Price = person.Product.Price,
                Description = person.Product.Description,
                Count = person.Product.Count,
            };


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                string json = JsonConvert.SerializeObject(addProduct);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync("/api/product/edit", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    ProductId result = JsonConvert.DeserializeObject<ProductId>(responseContent)!;
                    if (result != null)
                    {
                        if (person.Product != null && person.Product.Images.Count > 0)
                        {
                            json = JsonConvert.SerializeObject(new
                            {
                                Id = person.Product.Id
                            });

                            content = new StringContent(json, Encoding.UTF8, "application/json");

                            var res = client.PostAsync("/api/product/alldelimages", content).Result;
                            if (res.IsSuccessStatusCode)
                            {
                                foreach (var image in person.Product.Images)
                                {
                                    ProductImage productImage = new ProductImage
                                    {
                                        ProductId = person.Product.Id,
                                        ImageBase64 = image
                                    };

                                    json = JsonConvert.SerializeObject(productImage);

                                    content = new StringContent(json, Encoding.UTF8, "application/json");

                                    response = client.PostAsync("/api/product/addimage", content).Result;
                                }
                            }
                        }

                        if (person.Product != null && person.Product.Filters.Length > 0)
                        {
                            List<int> filtersId = person.Product.Filters.Split(' ').Where(x => !string.IsNullOrEmpty(x))
                                .Select(x => int.Parse(x)).ToList();

                            json = JsonConvert.SerializeObject(new
                            {
                                Id = person.Product.Id
                            });

                            content = new StringContent(json, Encoding.UTF8, "application/json");

                            var res = client.PostAsync("/api/product/alldelfilters", content).Result;
                            if (res.IsSuccessStatusCode)
                            {
                                foreach (var filterId in filtersId)
                                {
                                    ProductFilter productFilter = new ProductFilter
                                    {
                                        FilterId = filterId,
                                        ProductId = person.Product.Id
                                    };

                                    json = JsonConvert.SerializeObject(productFilter);

                                    content = new StringContent(json, Encoding.UTF8, "application/json");

                                    response = client.PostAsync("/api/product/tofilter", content).Result;
                                }

                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }

        }

        public static void GetAllOrders(PersonInfo person, ITelegramBotClient botClient, long chatId,
            ReplyKeyboardMarkup replyKeyboardMarkup, CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                HttpResponseMessage response = client.GetAsync("/api/order/get").Result;


                if(response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    List<OrderModal> orderModals = JsonConvert.DeserializeObject<List<OrderModal>>(result)!;

                    foreach (var orderModal in orderModals)
                    {
                        string form = $"Номер телефону: {orderModal.Id}\n" +
                            $"Ім'я: {orderModal.Name}\n" +
                            $"Прізвище: {orderModal.Surname}\n" +
                            $"По батькові: {orderModal.ParentName}\n" +
                            $"Назва товару: {orderModal.ProductName}\n" +
                            $"Назва бренду: {orderModal.ProductBrand}\n" + 
                            $"Розмір: {orderModal.ProductSize}\n" +
                            $"Номер телефону: {orderModal.Phone}\n" + 
                            $"E-mail: {orderModal.Email}\n" 
                            ;

                        SendMessage(botClient, chatId, form, 
                            replyKeyboardMarkup, cancellationToken);
                    }
                }

            }
        }

        public async static Task<PersonInfo> AddProduct(long chatId, ITelegramBotClient botClient,
        CancellationToken cancellationToken, PersonInfo person, Message message,
        ReplyKeyboardMarkup replyKeyboardMarkup, Update update)
        {

            if (string.IsNullOrEmpty(person.Product.Title))
            {
                person.Product.Title = message.Text;
                SendMessage(botClient, chatId, "Введіть опис товару",
                    replyKeyboardMarkup, cancellationToken);
            }
            else
            if (string.IsNullOrEmpty(person.Product.Description))
            {
                person.Product.Description = message.Text;
                SendMessage(botClient, chatId, "Введіть бренд товару",
                    replyKeyboardMarkup, cancellationToken);
            }
            else
            if (string.IsNullOrEmpty(person.Product.Brand))
            {
                person.Product.Brand = message.Text;
                SendMessage(botClient, chatId, "Введіть ціну товару",
                    replyKeyboardMarkup, cancellationToken);
            }
            else
            if (person.Product.Price == 0)
            {
                if (int.TryParse(message.Text, out int a))
                {
                    person.Product.Price = int.Parse(message.Text);
                    person.Product.Count = 0;
                    SendMessage(botClient, chatId, "Інформація про товар додана!\n" +
                        "Будь-ласка закиньте головну фотографію",
                        replyKeyboardMarkup, cancellationToken);
                }
                else
                {
                    SendMessage(botClient, chatId, "Неправильно введено ціну!",
                   replyKeyboardMarkup, cancellationToken);
                }
            }


            if (update != null && update.Message != null && update.Message.Document != null
                && person != null)
            {
                var fieldId = update.Message.Document.FileId;

                var fileInfo = await botClient.GetFileAsync(fieldId);

                string path = "https://api.telegram.org/file/bot6034830309:AAEijfrNiuJ-A2s2lYUP7uls7zsfvqU7kcU/" + fileInfo.FilePath;

                using HttpClient httpClient = new HttpClient();
                using HttpResponseMessage response = await httpClient.GetAsync(path);

                await using Stream fileStream = response.Content.ReadAsStream();

                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                string base64 = Convert.ToBase64String(bytes);

                person.Product.Images.Add(base64);

                SendMessage(botClient, chatId, "Фотографія додана! Додайте, будь-ласка, решту та натисніть " +
                    "'Додати фільтри' ",
                    KeyBoards.KeyBoards.toFilters, cancellationToken);
            }


            return person;

        }

        public async static Task<PersonInfo> EditProduct(long chatId, ITelegramBotClient botClient,
            CancellationToken cancellationToken, PersonInfo person, Message message,
            ReplyKeyboardMarkup replyKeyboardMarkup, Update update)
        {
            if (string.IsNullOrEmpty(person.Product.Title))
            {
                if (message.Text == "Далі")
                {
                    person.Product.Title = "__empty__";
                }
                else
                {
                    person.Product.Title = message.Text;
                }
                SendMessage(botClient, chatId, "Введіть опис товару",
                    KeyBoards.KeyBoards.nextItem, cancellationToken);
            }
            else
            if (string.IsNullOrEmpty(person.Product.Description))
            {
                if (message.Text == "Далі")
                {
                    person.Product.Description = "__empty__";
                }
                else
                {
                    person.Product.Description = message.Text;
                }
                SendMessage(botClient, chatId, "Введіть бренд товару",
                    KeyBoards.KeyBoards.nextItem, cancellationToken);
            }
            else
            if (string.IsNullOrEmpty(person.Product.Brand))
            {
                if (message.Text == "Далі")
                {
                    person.Product.Brand = "__empty__";
                }
                else
                {
                    person.Product.Brand = message.Text;
                }
                SendMessage(botClient, chatId, "Введіть ціну товару",
                    KeyBoards.KeyBoards.nextItem, cancellationToken);
            }
            else
            if (person.Product.Price == 0)
            {
                if (message.Text == "Далі")
                {
                    person.Product.Price = -1;
                    SendMessage(botClient, chatId, "Інформація про товар додана!\n" +
                            "Будь-ласка закиньте головну фотографію.\n" +
                            "Якщо фотографію не буде додано, то вони не зміняться",
                            KeyBoards.KeyBoards.toFilters, cancellationToken);
                }
                else
                {
                    if (int.TryParse(message.Text, out int a))
                    {
                        person.Product.Price = int.Parse(message.Text);

                        person.Product.Count = 0;
                        SendMessage(botClient, chatId, "Інформація про товар додана!\n" +
                            "Будь-ласка закиньте головну фотографію.\n" +
                            "Якщо фотографію не буде додано, то вони не зміняться",
                            KeyBoards.KeyBoards.toFilters, cancellationToken);
                    }
                    else
                    {
                        SendMessage(botClient, chatId, "Неправильно введено ціну!",
                       replyKeyboardMarkup, cancellationToken);
                    }
                }
            }


            if (update != null && update.Message != null && update.Message.Document != null
                && person != null)
            {
                var fieldId = update.Message.Document.FileId;

                var fileInfo = await botClient.GetFileAsync(fieldId);

                string path = "https://api.telegram.org/file/bot6034830309:AAEijfrNiuJ-A2s2lYUP7uls7zsfvqU7kcU/" + fileInfo.FilePath;

                using HttpClient httpClient = new HttpClient();
                using HttpResponseMessage response = await httpClient.GetAsync(path);

                await using Stream fileStream = response.Content.ReadAsStream();

                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                string base64 = Convert.ToBase64String(bytes);

                person.Product.Images.Add(base64);

                SendMessage(botClient, chatId, "Фотографія додана! Додайте, будь-ласка, решту та натисніть " +
                    "'Додати фільтри' ",
                    KeyBoards.KeyBoards.toFilters, cancellationToken);
            }


            return person;

        }

        public async static void GetAllProducts(int skipped, ITelegramBotClient botClient, long chatId
            , CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);

                HttpResponseMessage response = client.GetAsync($"/api/product/get?skipped={skipped - 1}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    List<ProductItem> items = JsonConvert
                        .DeserializeObject<List<ProductItem>>(responseContent)!;


                    foreach (var product in items)
                    {
                        List<InputMediaPhoto> media = new List<InputMediaPhoto>();
                        IAlbumInputMedia[] albumInputMedias = new IAlbumInputMedia[product.Images.Count];
                        for (int i = 0; i < product.Images.Count; i++)
                        {
                            string url = UrlInfo.URL + "/images/" + product.Images[i].Image;
                            albumInputMedias[i] = new InputMediaPhoto(
                                InputFile.FromUri(url));
                        }
                        try
                        {
                            Message[] messages = await botClient.SendMediaGroupAsync(
                            chatId: chatId,
                            media: albumInputMedias,
                            cancellationToken: cancellationToken);


                            string messageText = $"Продукт номер: {product.Id}\n" +
                            $"Назва продукту: {product.Title}\n" +
                            $"Опис продукту: {product.Description}\n" +
                            $"Ціна продукту: {product.Price}\n" +
                            $"Бренд продукту: {product.Brand}\n" +
                            $"*******";

                            int mediaGroupId = messages[0].MessageId;

                            // Відправка текстового повідомлення з вказанням ідентифікатора групи фотографій
                            await botClient.SendTextMessageAsync(chatId, messageText,
                                replyToMessageId: mediaGroupId);
                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            string messageText = $"Продукт номер: {product.Id}\n" +
                            $"Назва продукту: {product.Title}\n" +
                            $"Опис продукту: {product.Description}\n" +
                            $"Ціна продукту: {product.Price}\n" +
                            $"Бренд продукту: {product.Brand}\n" +
                            $"";

                            for (int i = 0; i < product.Images.Count; i++)
                            {
                                messageText += $"Посилання на фото {i+1}:" + 
                                    UrlInfo.URL + "/images/" + product.Images[i].Image + "\n";
                            }


                            messageText += $"*******";

                            await botClient.SendTextMessageAsync(chatId, messageText
                                );
                        }


                    }


                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }
        }

        public static void DeleteProduct(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken, PersonInfo person, int id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlInfo.URL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", person.Token);

                var requestBody = new
                {
                    id = id
                };

                string json = JsonConvert.SerializeObject(requestBody);

                var content = new StringContent(json, Encoding.UTF8, "application/json");


                HttpResponseMessage response = client.PostAsync($"/api/product/delete", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    DeleteProductMessage? message = JsonConvert
                        .DeserializeObject<DeleteProductMessage>(responseContent);
                    if (message != null)
                    {
                        SendMessage(botClient, chatId, message.Message,
                            KeyBoards.KeyBoards.productKeyboard, cancellationToken);
                    }


                }
                else
                {
                    Console.WriteLine("Error: " + response.Content);
                }
            }
        }

        public async static void SendMessage(ITelegramBotClient botClient,
            long chatId, string text, ReplyKeyboardMarkup replyKeyboardMarkup,
            CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
