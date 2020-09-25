using System;
using System.IO;
using System.Threading.Tasks;
using InformationService.Interfaces;
using InformationService.Models;
using InformationService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;

namespace pwsoFunctions
{
    public static class SendRegistrationSqlFunc
    {
        [FunctionName("SendRegistrationSqlFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var registrantDb = JsonSerializer.Deserialize<RegistrantDb>(requestBody);
                var worker = new RegistrantMessageWorker(registrantDb);
                var registrantSQL = worker.BuildRegistrant();
                var connectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_TrainingModel");
                var options = new DbContextOptionsBuilder<PwsodbContext>().UseSqlServer(connectionString ?? throw new InvalidOperationException()).Options;
                var context = new PwsodbContext(options);
                ITrainingRepository trainingRepository = new TrainingRepository(context);
                await trainingRepository.AddRegistrant(registrantSQL);

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
