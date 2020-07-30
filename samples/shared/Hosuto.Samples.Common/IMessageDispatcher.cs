namespace Dbosoft.Hosuto.Samples
{
    public interface IMessageDispatcher
    {
        void SendMessage(object sender, string message);

        void RegisterRecipient(IMessageRecipient recipient);
    }
}