namespace custom_identity.Infrastructure.Messaging
{
    public interface IEmailSender
    {
        Task SendAsync(string recepient, string subject, string message);
    }
}
