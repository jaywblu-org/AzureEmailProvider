using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Functions;
using EmailProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProvider.Services;

public class EmailService(EmailClient emailClient, ILogger<EmailService> logger)
{
    private readonly ILogger<EmailService> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (request != null)
            {
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: EmailSender.UnpackEmailRequest :: {ex.Message}");
        }

        return null!;
    }

    public bool SendEmail(EmailRequest request)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: request.To,
                subject: request.Subject,
                htmlContent: request.HtmlBody,
                plainTextContent: request.PlainText
            );

            if (result.HasCompleted)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: EmailSender.SendEmail :: {ex.Message}");
        }

        return false;
    }
}
