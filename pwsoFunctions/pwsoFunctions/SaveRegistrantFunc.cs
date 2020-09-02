using System;
using System.Collections.Generic;
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

    public class RegistrantMessage
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string nickName { get; set; }
        public string size { get; set; }
        public int sportId { get; set; }
        public int programId { get; set; }
        public string email1 { get; set; }
        public string email2 { get; set; }
        public string email3 { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string phone3 { get; set; }
        public string phoneType1 { get; set; }
        public string phoneType2 { get; set; }
        public string phoneType3 { get; set; }
        public bool canText1 { get; set; }
        public bool canText2 { get; set; }
        public bool canText3 { get; set; }
        public string sportName { get; set; }
        public string programName { get; set; }
        public bool isVolunteer { get; set; }
        public bool isWaitlisted { get; set; }
    }

    public class RegistrantDb
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string nickName { get; set; }
        public string size { get; set; }
        public int sportId { get; set; }
        public int programId { get; set; }
        public List<string> Emails { get; set; }
        public List<RegistrantPhone> Phones{ get; set; }
        public string sportName { get; set; }
        public string programName { get; set; }
        public bool isVolunteer { get; set; }
        public bool isWaitlisted { get; set; }
    }

    public class RegistrantPhone
    {
        public string Phone { get; set; }
        public string PhoneType { get; set; }
        public bool CanText { get; set; }
    }
}
