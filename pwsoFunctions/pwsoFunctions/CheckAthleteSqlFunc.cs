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
using Newtonsoft.Json;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace pwsoFunctions
{
    public static class CheckAthleteSqlFunc
    {
        [FunctionName("CheckAthleteSqlFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            Athletes athlete;
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var registrantDb = JsonSerializer.Deserialize<RegistrantDb>(requestBody);
                var connectionString = System.Environment.GetEnvironmentVariable("SQLCONNSTR_OrganizationModel");
                var options = new DbContextOptionsBuilder<PwsoContext>().UseSqlServer(connectionString ?? throw new InvalidOperationException()).Options;
                var context = new PwsoContext(options);
                IOrganizationRepository organizationRepository = new OrganizationRepository(context);
                athlete = await organizationRepository.FindAthleteByName(registrantDb.FirstName, registrantDb.LastName);

            }
            catch (Exception e)
            {
                log.LogInformation(e.ToString());
                return new BadRequestResult();
            }


            return new OkObjectResult(athlete);
        }
    }
}
