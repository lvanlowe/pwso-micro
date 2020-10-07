using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;

namespace pwsoFunctionsText
{
    public static class SendRegistrationTextFunc
    {
        [FunctionName("SendRegistrationTextFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [TwilioSms(AccountSidSetting = "AccountSid", AuthTokenSetting = "AuthToken")] IAsyncCollector<Twilio.Rest.Api.V2010.Account.CreateMessageOptions> messages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var registrantDb = JsonSerializer.Deserialize<RegistrantDb>(requestBody);
                var worker = new RegistrantMessageWorker(registrantDb, System.Environment.GetEnvironmentVariable("FromPhone"));

                var textMessageList = worker.PrepareRegistrationText();
                foreach (var textMessage in textMessageList)
                {
                    await messages.AddAsync(textMessage);
                }
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
