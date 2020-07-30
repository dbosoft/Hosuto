using System;
using System.Collections.Concurrent;

namespace Dbosoft.Hosuto.Samples
{
    public class MessageDispatcher : IMessageDispatcher
    {
        readonly ConcurrentBag<IMessageRecipient> _recipients = new ConcurrentBag<IMessageRecipient>();

        public void SendMessage(object sender, string message)
        {
            foreach (var recipient in _recipients)
            {
                recipient.ProcessMessage(sender, message);
            }
        }

        public void RegisterRecipient(IMessageRecipient recipient)
        {
            _recipients.Add(recipient);
        }
    }



}
