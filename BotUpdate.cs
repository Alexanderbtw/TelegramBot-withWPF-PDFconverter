using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot
{
    struct BotUpdate
    {
        public string text { get; set; }
        public long id { get; set; }
        public string username { get; set; }
        public string time { get; set; }

        public BotUpdate(string text, string username, long id, string time)
        {
            this.text = text;
            this.username = username;
            this.id = id;
            this.time = time;
        }
    }
}
