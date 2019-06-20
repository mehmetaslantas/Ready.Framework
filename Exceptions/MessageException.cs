using System;
using Ready.Framework.Service.Messaging;
#pragma warning disable 168

namespace Ready.Framework.Exceptions
{
    public class MessageException : Exception
    {
        public MessageException(string messageCode, string messageText = null, string messageTitle = null) : base(messageCode)
        {
            try
            {
                Message = Message.Alert(messageCode, messageText, messageTitle);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        public new Message Message { get; }
    }
}