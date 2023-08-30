using ElasticEmail.Api;
using ElasticEmail.Client;
using ElasticEmail.Model;

namespace custom_identity.Infrastructure.Messaging
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration,
            ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendAsync(string recepient, string subject, string message)
        {
            var basePath = _configuration.GetValue<string>("ElasticEmail:BasePath");
            var keyName = _configuration.GetValue<string>("ElasticEmail:ApiKeyHeader");
            var keyValue = _configuration.GetValue<string>("ElasticEmail:ApiKey");
            var fromEmail = _configuration.GetValue<string>("ElasticEmail:FromEmail");

            var bodyParts = new List<BodyPart>();
            BodyPart bodyPart = new BodyPart();
            bodyPart.ContentType = BodyContentType.HTML;
            bodyPart.Charset = "utf-8";
            bodyPart.Content = $"<h1>Test Message </h1> <br> <br> <p> {message} </p>";
            bodyParts.Add(bodyPart);

            var recepients = new List<EmailRecipient>();
            var recp = new EmailRecipient(recepient);
            recepients.Add(recp);

            var emailContent = new EmailContent();
            emailContent.Subject = subject;
            emailContent.Body = bodyParts;
            emailContent.From = fromEmail;

            var emailMessageData = new EmailMessageData(recepients, emailContent);

            var emailConfiguration = new Configuration() { BasePath = basePath };
            emailConfiguration.ApiKey.Add(keyName, keyValue);
            var apiInstance = new EmailsApi(emailConfiguration);

            try
            {
                EmailSend result = await apiInstance.EmailsPostAsync(emailMessageData);
                _logger.LogInformation(result.MessageID, result);
            }
            catch (ApiException e)
            {
                _logger.LogError(e.Message, e);
            }
        }
    }
}
