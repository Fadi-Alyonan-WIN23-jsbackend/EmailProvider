using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Azure;
using Azure.Messaging.ServiceBus;
using EmailProvider.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace EmailProvider.Functions;

public class EmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly EmailClient _emailClient;
    public EmailSender(ILogger<EmailSender> logger, EmailClient emailClient)
    {
        _logger = logger;
        _emailClient = emailClient;
    }

    [Function(nameof(EmailSender))]
    public async Task Run(
        [ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var req = UnpackEmailRequest(message);
            if (req != null && !string.IsNullOrEmpty(req.to))
            {
                if (SendEmail(req))
                {
                    await messageActions.CompleteMessageAsync(message);
                }

            }

        }catch (Exception ex)
        {
            _logger.LogError($"Error : EmailSender :: {ex.Message}");
        }
    }

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage massrge)
    {
        try
        {
            var req = JsonConvert.DeserializeObject<EmailRequest>(massrge.Body.ToString());
            if (req != null)
            {
                return req;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : EmailSender.UnpackEmailRequest :: {ex.Message}");
        }
        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.to,
                subject: emailRequest.subject,
                htmlContent: emailRequest.HTMLbody,
                plainTextContent: emailRequest.Text);
            if (result.HasCompleted)
            {
                return true;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : EmailSender.SendEmailAsync :: {ex.Message}");
        }
        return false;
    }
}
