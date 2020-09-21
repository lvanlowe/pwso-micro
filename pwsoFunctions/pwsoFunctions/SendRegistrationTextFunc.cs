using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Twilio;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace pwsoFunctions
{
    public static class SendRegistrationTextFunc
    {
        [FunctionName("SendRegistrationTextFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [TwilioSms(AccountSidSetting = "AccountSid", AuthTokenSetting = "AuthToken")] IAsyncCollector<CreateMessageOptions> messages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var toNumber = "+17033146512";
                var message = new CreateMessageOptions(new PhoneNumber(toNumber))
                {
                    From = new PhoneNumber(System.Environment.GetEnvironmentVariable("FromPhone")),
                    Body = "This is a test",
                    
                };
                await messages.AddAsync(message);
            }
            catch (Exception e)
            {
                log.LogInformation(e.ToString());
                return new BadRequestResult();
            }


            return new OkResult();


        }
    }
}
