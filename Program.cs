using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using System.Diagnostics;

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5818669056:AAEuDnLIWqPapce_yYJb59jnCka91zZ_zqE");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text != null)
                {
                    if (message.Text.ToLower() == "/start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Приветствую, {message.Chat.FirstName}. Я помогу тебе обработать фото, кидай");
                        return;
                    }
                }
                if (message.Photo != null)
                {
                    SendMessage("Отправь лучше документом");
                }
                if (message.Document != null)
                {
                    SendMessage("Ща, пару сек...");

                    var fileId = update.Message.Document.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{message.Document.FileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);
                    fileStream.Close();

                    Process.Start(@"C:\Users\Марс\Desktop\test.exe", $@"""{destinationFilePath}""");
                    await Task.Delay(30000);

                    await using Stream stream = System.IO.File.OpenRead(destinationFilePath);
                    await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, message.Document.FileName.Replace(".jpg", " (edited).jpg")));

                }

                async void SendMessage(string mess)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"{mess}");
                }
            }
        }



        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}