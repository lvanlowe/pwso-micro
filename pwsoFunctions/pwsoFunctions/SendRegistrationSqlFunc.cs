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
                var organizationConnectionString = System.Environment.GetEnvironmentVariable("SQLCONNSTR_OrganizationModel");
                var organizationOptions = new DbContextOptionsBuilder<PwsoContext>().UseSqlServer(organizationConnectionString ?? throw new InvalidOperationException()).Options;
                var organizationContext = new PwsoContext(organizationOptions);
                IOrganizationRepository organizationRepository = new OrganizationRepository(organizationContext);
                var athlete = await organizationRepository.FindAthleteByName(registrantDb.FirstName, registrantDb.LastName);
                var worker = new RegistrantMessageWorker(registrantDb);
                var registrantSQL = worker.BuildRegistrant();
                var trainingConnectionString = System.Environment.GetEnvironmentVariable("SQLAZURECONNSTR_TrainingModel");
                var trainingOptions = new DbContextOptionsBuilder<PwsodbContext>().UseSqlServer(trainingConnectionString ?? throw new InvalidOperationException()).Options;
                var trainingContext = new PwsodbContext(trainingOptions);
                ITrainingRepository trainingRepository = new TrainingRepository(trainingContext);
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
