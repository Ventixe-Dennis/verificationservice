using System.Diagnostics;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Caching.Memory;
using Presentation.Models;

namespace Presentation.Services;

public interface IVerificationService
{
    Task<VerificationResult> SendCodeAsync(SendCodeRequest request);
    void SaveVerficationCode(SaveCodeRequest request);
    VerificationResult VerifyCode(VerifyCodeRequest request);
}
public class VerificationService(IConfiguration configuration, EmailClient emailClient, IMemoryCache cache) : IVerificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailClient _emailClient = emailClient;
    private readonly IMemoryCache _cache = cache;
    private static readonly Random _random = new();

    public async Task<VerificationResult> SendCodeAsync(SendCodeRequest request)
    {
        try
        {

            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return new VerificationResult { Success = false, Message = "Email required" };

            var verificationCode = _random.Next(10000, 999990).ToString();
            var subject = $"Din kod är: {verificationCode}";
            var plainTextContent = $@"
               Hej,

               Din verifikationskod är: {verificationCode}!
                
               
               Med Vänliga Hälsningar,
               Ventixe Events



                ";
            var htmlContent = $@"
                <html>
                    <body>
                        <h2> Hej </h2>
                        <p>Din verifikationskod är: <strong>{verificationCode}</strong>.</p>
                        
                        <br/>
                        <p>Med Vänliga Hälsningars,<br/>Ventixe Events</p>
                    </body>
                </html>";

            var emailMessage = new EmailMessage(
                senderAddress: _configuration["ACS:SenderAdress"],
                recipients: new EmailRecipients([new(request.Email)]),
                content: new EmailContent(subject)
                {
                    Html = htmlContent,
                    PlainText = plainTextContent,
                });

            var emailSenOperator = await _emailClient.SendAsync(WaitUntil.Started, emailMessage);
            SaveVerficationCode(new SaveCodeRequest { Email = request.Email, Code = verificationCode, ValidFor = TimeSpan.FromMinutes(10) });

            return new VerificationResult { Success = true, Message = "The verification code was sent successfully." };
        }
        catch (Exception ex) {

            Debug.WriteLine(ex);
            return new VerificationResult { Success = false, Message = "Failed to send verification email." };
        }

        
    }

    public void SaveVerficationCode(SaveCodeRequest request)
    {
        _cache.Set(request.Email.ToLowerInvariant(), request.Code, request.ValidFor);
    }

    public VerificationResult VerifyCode(VerifyCodeRequest request)
    {
        var key = request.Email.ToLowerInvariant();

        if (_cache.TryGetValue(key, out string? storedCode))
        {

            if (storedCode == request.Code)
            {
                _cache.Remove(key);
                return new VerificationResult { Success = true, Message = "Verification success"};
            }

        }
        return new VerificationResult { Success = false, Message = "Invalid code" };
    }
}
