namespace Dbosoft.Hosuto.Samples
{
    public interface IMessageRecipient
    {
        void ProcessMessage(object sender, string message);
    }
}