using System;
using System.Collections.Generic;
using System.Text;
using pwsoProcesses.Models;
using SendGrid.Helpers.Mail;

namespace pwsoProcesses.Workers
{
    public class RegistrantEmailWorker
    {
        private SendGridMessage _message;
        private RegistrantDb _registrant;
        public RegistrantEmailWorker(SendGridMessage message, RegistrantDb registrant)
        {
            _message = message;
            _registrant = registrant;
        }

        public void BuildEmailTo()
        {
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            foreach (var email in _registrant.Emails)
            {
                emailAddresses.Add(new EmailAddress(email));
            }
            _message.AddTos(emailAddresses);
        }

        public void BuildEmailFrom()
        {

            if (string.IsNullOrEmpty(_registrant.Sender))
            {
                _message.From = new EmailAddress("webmaster@pwsova.org");
            }
            else
            {
                _message.From = new EmailAddress(_registrant.Sender); ;
            }

        }

        public void BuildEmailSubject()
        {
            string subject = _registrant.FirstName + " ";
            if (!string.IsNullOrEmpty(_registrant.NickName))
            {
                subject += "(" + _registrant.NickName + ") ";
            }
            subject += _registrant.LastName + " is registered for ";
            subject += _registrant.Sport;

            _message.Subject = subject;
        }



    }
}
