using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using pwsoProcesses;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;
using SendGrid.Helpers.Mail;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace pwsoFunctions
{
    public static class SendRegistrationEmailFunc
    {
        [FunctionName("SendRegistrationEmailFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var registrantDb = JsonSerializer.Deserialize<RegistrantDb>(requestBody);
                var message = new SendGridMessage();
                var worker = new RegistrantMessageWorker(message, registrantDb);
                await messageCollector.AddAsync(worker.PrepareRegistrationEmail());

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
