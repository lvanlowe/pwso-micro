using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using InformationService.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using pwsoProcesses;
using pwsoProcesses.Models;
using RegistrantPhone = pwsoProcesses.RegistrantPhone;


namespace pwsoFunctions
{
    public static class SaveRegistrantFunc
    {
        [FunctionName("SaveRegistrantFunc")]
        public static async Task Run([QueueTrigger("registrant", Connection = "AzureWebJobsStorage")]string myQueueItem,
                        [CosmosDB(
                databaseName: "pwso",
                collectionName: "registrant",
                ConnectionStringSetting = "CosmosDBConnection")]IAsyncCollector<RegistrantDb> registrantDocuments,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            Process process = new Process();
            var emailUrl = System.Environment.GetEnvironmentVariable("EmailUrl");
            var phoneUrl = System.Environment.GetEnvironmentVariable("PhoneUrl");
            var trainingUrl = System.Environment.GetEnvironmentVariable("TrainingUrl");
            var athleteUrl = System.Environment.GetEnvironmentVariable("AthleteUrl");
            try
            {

                var registrantMessage = JsonSerializer.Deserialize<RegistrantMessage>(myQueueItem, options);
                var registrantDb = JsonSerializer.Deserialize<RegistrantDb>(myQueueItem, options);
                registrantDb.Emails = new List<string>();
                registrantDb.Phones = new List<RegistrantPhone>();
                registrantDb.Sport = registrantMessage.SportName;
                AddEmail(registrantMessage.Email1, registrantDb);
                AddEmail(registrantMessage.Email2, registrantDb);
                AddEmail(registrantMessage.Email3, registrantDb);
                AddPhone(registrantMessage.Phone1, registrantMessage.Phone1Type, registrantMessage.CanText1, registrantDb);
                AddPhone(registrantMessage.Phone2, registrantMessage.Phone2Type, registrantMessage.CanText2, registrantDb);
                AddPhone(registrantMessage.Phone3, registrantMessage.Phone3Type, registrantMessage.CanText3, registrantDb);
                if (!registrantDb.IsVolunteer)
                {
                    var athleteJob = await process.GetRegistrationAthlete(registrantDb, athleteUrl);
                    if (athleteJob.IsSuccessStatusCode)
                    {
                        var result = athleteJob.Content.ReadAsStringAsync();
                        if (result.Result.Length > 0)
                        {
                            var athlete = JsonSerializer.Deserialize<Athletes>(result.Result, options);
                            registrantDb.AthleteId = athlete.Id;
                            registrantDb.MedicalExpirationDate = athlete.MedicalExpirationDate;
                        }
                    }
                }                
                process.SendRegistrationNotification(registrantDb, trainingUrl);
                await registrantDocuments.AddAsync(registrantDb);
                process.SendRegistrationNotification(registrantDb, emailUrl);
                process.SendRegistrationNotification(registrantDb, phoneUrl);

            }
            catch (Exception e)
            {
                log.LogInformation(e.ToString());
                //throw;
            }

            //document = registrantDb;
        }

        private static void AddPhone(string phone, string phoneType, bool canText, RegistrantDb registrantDb)
        {
            if (string.IsNullOrEmpty(phone)) return;
            var registrantPhone = new RegistrantPhone {Phone = phone, PhoneType = phoneType, CanText = canText};
            registrantDb.Phones.Add(registrantPhone);
        }

        private static void AddEmail(string email, RegistrantDb registrantDb)
        {
            if (!string.IsNullOrEmpty(email))
            {
                registrantDb.Emails.Add(email);
            }
        }
    }

    public class RegistrantMessage
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Size { get; set; }
        public int SportId { get; set; }
        public int ProgramId { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string Email3 { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Phone1Type { get; set; }
        public string Phone2Type { get; set; }
        public string Phone3Type { get; set; }
        public bool CanText1 { get; set; }
        public bool CanText2 { get; set; }
        public bool CanText3 { get; set; }
        public string SportName { get; set; }
        public string ProgramName { get; set; }
        public bool IsVolunteer { get; set; }
        public bool IsWaitListed { get; set; }
        public string Sender { get; set; }
    }

}
