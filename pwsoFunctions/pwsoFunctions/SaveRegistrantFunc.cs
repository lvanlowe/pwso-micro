using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace pwsoFunctions
{
    public static class SaveRegistrantFunc
    {
        [FunctionName("SaveRegistrantFunc")]
        public static void Run([QueueTrigger("registrant", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
