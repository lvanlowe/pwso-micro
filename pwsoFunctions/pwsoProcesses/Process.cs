using System;
using pwsoProcesses.Models;

namespace pwsoProcesses
{
    public class Process
    {
        public void SendRegistrationEmail(RegistrantDb registrant)
        {
            var name = registrant.FirstName;
        }
    }
}
