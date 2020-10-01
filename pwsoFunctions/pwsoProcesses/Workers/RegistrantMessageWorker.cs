using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SendGridMessage _message;
        private readonly RegistrantDb _registrant;
        private List<CreateMessageOptions> _textMessageList;
        private readonly string _fromNumber;
        public string MedicalEmail { get; set; }
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
            BuildEmailCopy();
            BuildEmailTo();
            BuildEmailSubject();
            BuildEmailBody();
            return _message;
        }

        public SendGridMessage PrepareMedicalEmail()
        {
            BuildEmailFrom();
            BuildEmailCopy();
            BuildEmailMedicalTo();
            BuildMedicalSubject();
            BuildMedicalEmailBody();
            return _message;
        }


        public void BuildEmailTo()
        {
            var emailAddresses = _registrant.Emails.Select(email => new EmailAddress(email)).ToList();
            _message.AddTos(emailAddresses);
        }

        public void BuildEmailMedicalTo()
        {
            _message.AddTo(string.IsNullOrEmpty(MedicalEmail) ? "webmaster@pwsova.org" : MedicalEmail);
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

        public void BuildEmailCopy()
        {
            _message.AddCc(_message.From.Email);
        }


        public void BuildEmailSubject()
        {
            var name = FormatName();
            var subject = name + " is registered for ";
            subject += _registrant.Sport;
            _message.Subject = subject;
        }

        public void BuildMedicalSubject()
        {
            var name = FormatName();
            var subject = name + " release form missing for ";
            subject += _registrant.Sport;
            _message.Subject = subject;
        }

        public void BuildEmailBody()
        {
            const string body = "<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            var message = BuildMessage(body);
            _message.HtmlContent = message;
        }

        public void BuildMedicalEmailBody()
        {
            const string body = "<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            var message = BuildMedicalMessage(body);
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

        private string BuildMedicalMessage(string body)
        {
            var name = FormatName();
            body += name;
            body += " release form could not be verified automatically when registering for ";
            body += _registrant.Sport;
            if (!string.IsNullOrEmpty(_registrant.ProgramName))
            {
                body += " at " + _registrant.ProgramName;
            }

            body += ".<br><br>&nbsp;&nbsp;&nbsp;&nbsp;Please verify manually.";
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
            return (from phone in _registrant.Phones where phone.CanText select "+1" + phone.Phone).ToList();
        }

        public List<CreateMessageOptions> BuildPhoneMessageList(List<string> phoneList)
        {
            _textMessageList = new List<CreateMessageOptions>();
            foreach (var message in phoneList.Select(phone => new CreateMessageOptions(new PhoneNumber(phone))
            {
                From = new PhoneNumber(_fromNumber),
                Body = BuildMessage(string.Empty),
            }))
            {
                _textMessageList.Add(message);
            }
            return _textMessageList;
        }

        public Registrant BuildRegistrant()
        {
            var registrant = new Registrant
            {
                FirstName = _registrant.FirstName,
                LastName = _registrant.LastName,
                IsVolunteer = _registrant.IsVolunteer,
                ProgramId = _registrant.ProgramId,
                Size = _registrant.Size,
                SportId = _registrant.SportId,
                NickName = _registrant.NickName,
                RegistrantEmail = new List<RegistrantEmail>(),
                RegistrantPhone = new List<InformationService.Models.RegistrantPhone>()
            };

            foreach (var email in _registrant.Emails)
            {
                registrant.RegistrantEmail.Add(new RegistrantEmail{Email = email});
            }

            foreach (var phone in _registrant.Phones)
            {
                registrant.RegistrantPhone.Add(new InformationService.Models.RegistrantPhone
                {
                    CanText = phone.CanText,
                    Phone = phone.Phone,
                    PhoneType = phone.PhoneType
                });
            }

            if (_registrant.AthleteId > 0)
            {
                registrant.RegisteredAthlete.Add(new RegisteredAthlete{AthletesId = _registrant.AthleteId});
            }

            return registrant;
        }


    }
}
