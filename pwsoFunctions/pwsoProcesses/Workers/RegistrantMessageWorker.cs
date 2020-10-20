using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure.Storage.Blobs.Models;
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

        public SendGridMessage PrepareRegistrationEmail(BlobDownloadInfo medicalDownload, BlobDownloadInfo downloadInstructions)
        {
            BuildEmailFrom();
            BuildEmailTo();
            BuildEmailCopy();
            BuildEmailSubject();
            BuildEmailBody();
            BuildAthleteMedicalEmailBody();
            BuildSignature();
            AddAttachments(medicalDownload, downloadInstructions);
            return _message;
        }

        public SendGridMessage PrepareMedicalEmail()
        {
            BuildEmailFrom();
            BuildEmailMedicalTo();
            BuildEmailCopy();
            BuildMedicalSubject();
            BuildMedicalEmailBody();
            BuildSignature();
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
            var emailFound = false;
            foreach (var emailToAddress in _message.Personalizations[0].Tos.Where(emailToAddress => emailToAddress.Email == _message.From.Email))
            {
                emailFound = true;
            }

            if (!emailFound)
            {
                _message.AddCc(_message.From.Email);
            }
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
            const string body = "<br>Hi Registrant<br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            var message = BuildMessage(body);
            _message.HtmlContent = message;
        }

        public void BuildAthleteMedicalEmailBody()
        {
            _message.HtmlContent += "<br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            _message.HtmlContent += FormatName();
            _message.HtmlContent += " release form ";
            if (_registrant.AthleteId == 0 || !_registrant.MedicalExpirationDate.HasValue)
            {
                _message.HtmlContent += "could not be verified automatically and therefore needs to be checked manually we will connect you to let you if there is anything you need to do before this athlete can participate.";
            }
            else
            {
                var medicalExpirationDate = _registrant.MedicalExpirationDate.Value;
                if (medicalExpirationDate > DateTime.Now)
                {
                    _message.HtmlContent += "has been verified and is up to date, therefore can participate.";
                    if (medicalExpirationDate >= DateTime.Now.AddMonths(3)) return;
                    _message.HtmlContent += " However, this athletes form will be expiring on ";
                    _message.HtmlContent += medicalExpirationDate.ToShortDateString();
                    _message.HtmlContent += ", please update before it expires.";
                }
                else
                {
                    _message.HtmlContent += "had expired on ";
                    _message.HtmlContent += medicalExpirationDate.ToShortDateString();
                    _message.HtmlContent += ". The athletes release needs to be updated before they can participate.";
                }
            }

        }

        public void BuildSignature()
        {
            _message.HtmlContent += "<br><br><br>PWSO Registration Application";
            _message.HtmlContent += "<br>Prince William Special Olympics";
            _message.HtmlContent += "<br>PO Box 1073";
            _message.HtmlContent += "<br>Woodbridge, VA 22195-1073";
            _message.HtmlContent += "<br>www.pwsova.org";
        }

        public void AddAttachments(BlobDownloadInfo medicalDownload, BlobDownloadInfo downloadInstructions)
        {
            BinaryReader medicalReader = new BinaryReader(medicalDownload.Content);
            Byte[] medicalBytes = medicalReader.ReadBytes(Convert.ToInt32(medicalDownload.ContentLength));
            _message.AddAttachment("Athlete_Registration,_Release_and_Medical_Form4.pdf", Convert.ToBase64String(medicalBytes, 0, medicalBytes.Length));
            BinaryReader instructionReader = new BinaryReader(downloadInstructions.Content);
            Byte[] instructionBytes = instructionReader.ReadBytes(Convert.ToInt32(downloadInstructions.ContentLength));
            _message.AddAttachment("Instructions for Area 23 Medical Forms.docx", Convert.ToBase64String(instructionBytes, 0, instructionBytes.Length));

        }


        public void BuildMedicalEmailBody()
        {
            const string body = "<br>Hi Medical Coordinator<br><br>&nbsp;&nbsp;&nbsp;&nbsp;";
            var message = BuildMedicalMessage(body);
            _message.HtmlContent = message;
        }


        public string BuildMessage(string body)
        {
            var name = FormatName();
            body += name;
            body += " has been";
            if (_registrant.IsVolunteer)
            {
                body += " successfully registered as an";
                body += " volunteer for ";
            }
            else
            {
                if (_registrant.IsWaitListed)
                {
                    body += " placed on the waitlist for ";
                }
                else
                {
                    body += " successfully registered as an";
                    body += " athlete for ";
                }
            }

            body += _registrant.Sport;
            if (!string.IsNullOrEmpty(_registrant.ProgramName))
            {
                body += " at " + _registrant.ProgramName;
            }

            if (_registrant.IsWaitListed)
            {
                body += " and will be notified when the status changes.";
            }
            else
            {
                body += ".";
            }
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
