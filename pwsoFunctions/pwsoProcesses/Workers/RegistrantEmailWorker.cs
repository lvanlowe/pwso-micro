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

        public SendGridMessage PrepareRegistrationEmail()
        {
            BuildEmailFrom();
            BuildEmailTo();
            BuildEmailSubject();
            BuildEmailBody();
            return _message;
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
            var name = FormatName();
            var subject = name + " is registered for ";
            subject += _registrant.Sport;
            _message.Subject = subject;
        }

        public void BuildEmailBody()
        {
            var name = FormatName();
            var body = "<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            body += name;
            body += " has been successfully registered as an";
            if (_registrant.IsVolunteer)
            {
                body += " volunteer for ";
            }
            else
            {
                body += " athlete for ";
            }
            body += _registrant.Sport;
            if (!string.IsNullOrEmpty(_registrant.ProgramName))
            {
                body += " at " + _registrant.ProgramName;
            }
            body += ".";
            _message.HtmlContent = body;
        }

        private string FormatName()
        {
            var name = _registrant.FirstName + " ";
            if (!string.IsNullOrEmpty(_registrant.NickName))
            {
                name += "(" + _registrant.NickName + ") ";
            }

            name += _registrant.LastName;
            return name;
        }
    }
}
