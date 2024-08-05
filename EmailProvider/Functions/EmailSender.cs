using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Models;
using EmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProvider.Functions;

public class EmailSender(ILogger<EmailSender> logger, EmailService emailService)
{
    private readonly ILogger<EmailSender> _logger = logger;
    private readonly EmailService _emailService = emailService;

    [Function(nameof(EmailSender))]
    public async Task Run(
        [ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var request = _emailService.UnpackEmailRequest(message);
            if (request != null && !string.IsNullOrEmpty(request.To))
            {
                if (_emailService.SendEmail(request))
                {
                    await messageActions.CompleteMessageAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: EmailSender.Run :: {ex.Message}");
        }
    }
}
