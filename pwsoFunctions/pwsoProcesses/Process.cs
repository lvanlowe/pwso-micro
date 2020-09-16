using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using pwsoProcesses.Models;

namespace pwsoProcesses
{
    public class Process
    {
        public void SendRegistrationEmail(RegistrantDb registrant, string url)
        {
            var registrantDb = JsonSerializer.Serialize<RegistrantDb>(registrant);
            var client = new HttpClient();
            _ = client.PostAsync(url, new StringContent(registrantDb, Encoding.UTF8, "application/json"));
            var name = registrant.FirstName;
        }
    }
}
