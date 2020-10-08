using System;
using System.Collections.Generic;
using System.Text;

namespace pwsoProcesses.Models
{
    public class RegistrantDb
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Size { get; set; }
        public int SportId { get; set; }
        public int ProgramId { get; set; }
        public List<string> Emails { get; set; }
        public List<RegistrantPhone> Phones { get; set; }
        public string Sport { get; set; }
        public string ProgramName { get; set; }
        public bool IsVolunteer { get; set; }
        public bool IsWaitListed { get; set; }
        public string Sender { get; set; }
        public int AthleteId { get; set; }
        public DateTime? MedicalExpirationDate { get; set; }
        public DateTime DateCreated { get; set; }
    }

}
