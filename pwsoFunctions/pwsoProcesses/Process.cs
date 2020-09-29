using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InformationService.Models;
using pwsoProcesses.Models;

namespace pwsoProcesses
{
    public class Process
    {
        public void SendRegistrationNotification(RegistrantDb registrant, string url)
        {
            var registrantDb = JsonSerializer.Serialize<RegistrantDb>(registrant);
            var client = new HttpClient();
            _ = client.PostAsync(url, new StringContent(registrantDb, Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> GetRegistrationAthlete(RegistrantDb registrant, string url)
        {
            var registrantDb = JsonSerializer.Serialize<RegistrantDb>(registrant);
            var client = new HttpClient();
            HttpResponseMessage athlete = await client.PostAsync(url, new StringContent(registrantDb, Encoding.UTF8, "application/json"));

            return athlete;
        }

    }
}
