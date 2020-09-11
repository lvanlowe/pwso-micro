using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace pwsoFunctions
{
    public static class AddRegistrantSqlFunc
    {

        private static string logicAppUri = @"https://prod-30.eastus2.logic.azure.com:443/workflows/43224f8aaed143faa7f8903f4b5f85f9/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=GeCbfnbXTZdF3puctBa84KRODNg6QoRxjKtGFuFsy_8";
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("AddRegistrantSqlFunc")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "TalkingBook",
            collectionName: "registrant",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "leases"   )]IReadOnlyList<Document> input, ILogger log)
        {



            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);

                foreach (var item in input)
                {
                    //var response = await httpClient.PostAsync(logicAppUri, new StringContent(item.ToString(), Encoding.UTF8, "application/json"));
                }
            }
        }
    }
}

