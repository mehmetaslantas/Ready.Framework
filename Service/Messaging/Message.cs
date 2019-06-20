using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Ready.Framework.Configuration;
using Ready.Framework.Service.Messaging.Enum;

namespace Ready.Framework.Service.Messaging
{
    public class Message
    {
        public Message()
        {
            Buttons = new List<MessageButton>();
        }

        public string Code { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public bool IsSilent { get; set; }

        public MessageIconType IconType { get; set; }

        public List<MessageButton> Buttons { get; set; }

        [JsonIgnore]
        public HttpStatusCode HttpStatus { get; set; }

        public static Message Alert(string code, string text = null, string title = null)
        {
            code = ConfigurationManager.MessageCodePrefix + code;

            var message = new Message
            {
                Code = code,
                Title = ConfigurationManager.GetParameter($"{code}_TITLE", title),
                Text = ConfigurationManager.GetParameter($"{code}_TEXT", text),
                Buttons = new List<MessageButton>
                {
                    new MessageButton
                    {
                        Text = ConfigurationManager.GetParameter($"{code}_BTN_TEXT", "Ok"),
                        Type = MessageButtonType.Ok
                    }
                },
                IconType = MessageIconType.Error,
                HttpStatus = HttpStatusCode.ServiceUnavailable,
                IsSilent = false
            };

            return message;
        }
    }
}