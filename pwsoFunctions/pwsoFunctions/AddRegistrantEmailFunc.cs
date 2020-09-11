using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using pwsoProcesses.Models;
using SendGrid.Helpers.Mail;

namespace pwsoFunctions
{
    public static class AddRegistrantEmailFunc
    {
        [FunctionName("AddRegistrantEmailFunc")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "TalkingBook",
            collectionName: "registrant",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
                foreach (var item in input)
                {
                    var registrantMessage = JsonSerializer.Deserialize<RegistrantDb>(item.ToString());

                    var message = new SendGridMessage();
                    message.AddTo(registrantMessage.Emails[0]);
                    message.AddContent("text/html", "This is a new test");
                    message.SetFrom(new EmailAddress("webmaster@pwsova.org"));
                    message.SetSubject("Registrant has been added");

                    await messageCollector.AddAsync(message);
                }


            }
        }
    }
}
