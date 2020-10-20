using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
using Microsoft.WindowsAzure.Storage.Blob;

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
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var registrantDb = JsonSerializer.Deserialize<RegistrantDb>(requestBody);
                var message = new SendGridMessage();
                var worker = new RegistrantMessageWorker(message, registrantDb);
                var connectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("forms");
                BlobClient blobMedical = containerClient.GetBlobClient("Athlete_Registration,_Release_and_Medical_Form4.pdf");
                BlobDownloadInfo downloadMedical = await blobMedical.DownloadAsync();
                BlobClient blobInstructions = containerClient.GetBlobClient("Instructions for Area 23 Medical Forms_0.docx");
                BlobDownloadInfo downloadInstructions = await blobInstructions.DownloadAsync();
                //Stream myBlob = downloadMedical.Content;
                //var length = downloadMedical.ContentLength;
                await messageCollector.AddAsync(worker.PrepareRegistrationEmail(downloadMedical, downloadInstructions));
                if (!registrantDb.IsVolunteer && registrantDb.AthleteId == 0)
                {
                    var medMessage = new SendGridMessage();
                    var medWorker = new RegistrantMessageWorker(medMessage, registrantDb)
                    {
                        MedicalEmail = System.Environment.GetEnvironmentVariable("MedicalEmail")
                    };
                    await messageCollector.AddAsync(medWorker.PrepareMedicalEmail());
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
