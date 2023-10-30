using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot;
using System.Diagnostics;

namespace WPFapp
{
    class TelegramClient
    {
        private MainWindow w;
        private static TelegramBotClient bot;

        private string fileName = "updates.json";
        private string idsFileName = "ids_updates.json";
        public ObservableCollection<BotUpdate> botUpdates;
        public HashSet<long> botIds;

        public TelegramClient(MainWindow W)
        {
            this.botIds = new HashSet<long>();
            this.botUpdates = new ObservableCollection<BotUpdate>();
            this.w = W;
            
            try
            {
                var botUpdatesString = System.IO.File.ReadAllText(fileName);
                var botIdsString = System.IO.File.ReadAllText(idsFileName);
                botIds = JsonConvert.DeserializeObject<HashSet<long>>(botIdsString) ?? botIds;
                botUpdates = JsonConvert.DeserializeObject<ObservableCollection<BotUpdate>>(botUpdatesString) ?? botUpdates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }

            bot = new TelegramBotClient(System.IO.File.ReadAllText(@"D:\C#\Homework\Homework 10\WPFapp\key.txt"));
            bot.StartReceiving(UpdateHandler, ErrorHandler);
        }

        private async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cancToken)
        {
            if (update.Type == UpdateType.Message)
            {
                botIds.Add(update.Message.Chat.Id);
                var botIdsString = JsonConvert.SerializeObject(botIds);
                System.IO.File.WriteAllText(idsFileName, botIdsString);

                if (update.Message.Type == MessageType.Text)
                {
                    w.Dispatcher.Invoke(() =>
                    {
                        botUpdates.Add(
                            new BotUpdate(
                            update.Message.Text,
                            update.Message.Chat.Username,
                            update.Message.Chat.Id,
                            DateTime.Now.ToLongTimeString()));
                    });

                    var botUpdatesString = JsonConvert.SerializeObject(botUpdates);
                    System.IO.File.WriteAllText(fileName, botUpdatesString);
                }
                
                else if (update.Message.Type == MessageType.Document)
                {
                    var fileId = update.Message.Document.FileId;
                    var fileInfo = await bot.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    var full_path = System.IO.Path.Combine($@"D:\C#\Homework\Homework 10\WPFapp\Data\{update.Message.Document.FileName}");
                    FileStream fs = System.IO.File.OpenWrite(full_path);
                    await bot.DownloadFileAsync(filePath, fs);
                    fs.Close();
                    fs.Dispose();

                    Task pdfTask = Task.Run(() => ConvertToPDF(full_path));
                    pdfTask.Wait();

                    Stream stream = System.IO.File.OpenRead(@"D:\C#\Homework\Homework 10\WPFapp\Data\Out.pdf");
                    await bot.SendDocumentAsync(
                        chatId: update.Message.Chat.Id, 
                        document: InputFile.FromStream(stream, "Out.pdf"),
                        caption: "Porno");
                    stream.Dispose();
                    return;
                }
            }
        }

        public void SendMessage(string message, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Select receiver");
                return;
            }
            long _id = Convert.ToInt64(id);
            bot.SendTextMessageAsync(_id, message);
        }

        private Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private void ConvertToPDF(object obj_full_path)
        {
            string full_path = obj_full_path as string;
            var ext = Path.GetExtension(full_path);
            var out_path = @"D:\C#\Homework\Homework 10\WPFapp\Data\Out.pdf";

            if (ext == ".docx" || ext == ".doc")
            {
                var appWord = new Microsoft.Office.Interop.Word.Application();
                if (appWord.Documents != null)
                {
                    var wordDocument = appWord.Documents.Open(full_path);
                    if (wordDocument != null)
                    {
                        wordDocument.ExportAsFixedFormat(out_path,
                        Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                        wordDocument.Close();
                    }
                    appWord.Quit();
                }
            }

            else if (ext == ".jpg")
            {
                iTextSharp.text.Rectangle pageSize = null;

                using (var srcImage = new Bitmap(full_path))
                {
                    pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
                }
                using (var ms = new MemoryStream())
                {
                    var document = new iTextSharp.text.Document(pageSize);
                    PdfWriter.GetInstance(document, ms).SetFullCompression();
                    document.Open();
                    var image = iTextSharp.text.Image.GetInstance(full_path);
                    document.Add(image);
                    document.Close();

                    System.IO.File.WriteAllBytes(out_path, ms.ToArray());
                }
            }

            System.IO.File.Delete(full_path);
        }
    }
}