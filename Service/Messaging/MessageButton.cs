using System.Collections.Generic;
using Ready.Framework.Service.Messaging.Enum;

namespace Ready.Framework.Service.Messaging
{
    public class MessageButton
    {
        public string Text { get; set; }

        public MessageButtonType Type { get; set; }

        public string Url { get; set; }

        public string Data { get; set; }

        public static List<MessageButton> Alert()
        {
            return new List<MessageButton> { new MessageButton { Type = MessageButtonType.Ok, Text = "OK" } };
        }

        public static List<MessageButton> AlertWithLink(string url, string buttonText = "OK")
        {
            return new List<MessageButton> { new MessageButton { Type = MessageButtonType.OpenUrl, Url = url, Text = buttonText } };
        }
    }
}