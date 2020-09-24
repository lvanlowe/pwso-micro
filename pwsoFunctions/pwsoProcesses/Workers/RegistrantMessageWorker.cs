using System;
using System.Collections.Generic;
using System.Text;
using InformationService.Models;
using pwsoProcesses.Models;
using SendGrid.Helpers.Mail;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace pwsoProcesses.Workers
{
    public class RegistrantMessageWorker
    {
        private SendGridMessage _message;
        private RegistrantDb _registrant;
        private List<CreateMessageOptions> _textMessageList;
        private string _fromNumber;
        public RegistrantMessageWorker(SendGridMessage message, RegistrantDb registrant)
        {
            _message = message;
            _registrant = registrant;
        }

        public RegistrantMessageWorker(RegistrantDb registrant, string fromNumber)
        {
            _registrant = registrant;
            _fromNumber = fromNumber;
        }

        public RegistrantMessageWorker(RegistrantDb registrant)
        {
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
            var body = "<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            var message = BuildMessage(body);
            _message.HtmlContent = message;
        }

        private string BuildMessage(string body)
        {
            var name = FormatName();
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
            return body;
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

        public List<CreateMessageOptions> PrepareRegistrationText()
        {
            var phoneList = BuildPhoneList();
            return BuildPhoneMessageList(phoneList);
        }

        public List<string> BuildPhoneList()
        {
            var phoneList = new List<string>();
            foreach (var phone in _registrant.Phones)
            {
                if (phone.CanText)
                {
                    phoneList.Add("+1" + phone.Phone);
                }
            }

            return phoneList;
        }

        public List<CreateMessageOptions> BuildPhoneMessageList(List<string> phoneList)
        {
            _textMessageList = new List<CreateMessageOptions>();
            foreach (var phone in phoneList)
            {
                var message = new CreateMessageOptions(new PhoneNumber(phone))
                {
                    From = new PhoneNumber(_fromNumber),
                    Body = BuildMessage(string.Empty),
                };
                _textMessageList.Add(message);
            }
            return _textMessageList;
        }

        public Registrant BuildRegistrant()
        {
            Registrant registrant = new Registrant
            {
                FirstName = _registrant.FirstName
            };


            return registrant;
        }
    }
}
