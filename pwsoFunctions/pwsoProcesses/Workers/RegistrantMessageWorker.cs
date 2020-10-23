using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly RegistrantSendGrid _registrantSendGrid;

        private List<CreateMessageOptions> _textMessageList;
        private readonly string _fromNumber;
        private const string VolunteerTemplate = "d-f889b9fc09c54ad58bf83a0cb36720bf";
        private const string MedicalTemplate = "d-8226e92536ce4514a2223781c3a45af0";
        private const string AthleteTemplate = "d-23ad534900d547a6b5b04362ea96d16b";
        private const string WaitListTemplate = "d-b7b1ec6c2ec54e31b49dab23584b1faa";
        private const string NoMedicalTemplate = "d-97360ccc62864937a3e0e79a60b54dda";
        private const string ExpiredTemplate = "d-ddf54cab8a514b02a72d54f4b1faa41a";
        private const string ExpiringTemplate = "d-1dabedfc6e5445c7b914916822c7b1ab";
        private const string WaitListNoMedicalTemplate = "d-16dcb63a082b4ccb8242b55f20f89ba9";
        private const string WaitListExpiredTemplate = "d-a094a54cf8fb427eaeae56f20fccc419";
        private const string WaitListExpiringTemplate = "d-3450ce63e0b4401cbb35529bd799c248";
        public string MedicalEmail { get; set; }
        public RegistrantMessageWorker(SendGridMessage message, RegistrantDb registrant)
        {
            _message = message;
            _registrant = registrant;
            _registrantSendGrid = new RegistrantSendGrid {Name = FormatName(), Sport = registrant.Sport};

            if (!string.IsNullOrEmpty(registrant.ProgramName))
            {
                _registrantSendGrid.Sport += " at " + registrant.ProgramName;
            }

            if (registrant.AthleteId > 0 && registrant.MedicalExpirationDate.HasValue)
            {
                _registrantSendGrid.MedicalExpirationDate = registrant.MedicalExpirationDate.Value.ToLongDateString();
            }

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
            SetUpTemplate(BuildTemplate());
            if (!_registrant.IsVolunteer || _registrant.AthleteId == 0 || !_registrant.MedicalExpirationDate.HasValue || _registrant.MedicalExpirationDate.Value < DateTime.Now.AddMonths(3))
            {
                AddAttachments(medicalDownload, downloadInstructions);
            }

            return _message;
        }

        private void SetUpTemplate(string template)
        {
            _message.SetTemplateId(template);
            _message.SetTemplateData(_registrantSendGrid);
        }

        public SendGridMessage PrepareMedicalEmail()
        {
            BuildEmailFrom();
            BuildEmailMedicalTo();
            BuildEmailCopy();
            SetUpTemplate(MedicalTemplate);
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

        public void AddAttachments(BlobDownloadInfo medicalDownload, BlobDownloadInfo downloadInstructions)
        {
            BinaryReader medicalReader = new BinaryReader(medicalDownload.Content);
            Byte[] medicalBytes = medicalReader.ReadBytes(Convert.ToInt32(medicalDownload.ContentLength));
            _message.AddAttachment("Athlete_Registration,_Release_and_Medical_Form4.pdf", Convert.ToBase64String(medicalBytes, 0, medicalBytes.Length));
            BinaryReader instructionReader = new BinaryReader(downloadInstructions.Content);
            Byte[] instructionBytes = instructionReader.ReadBytes(Convert.ToInt32(downloadInstructions.ContentLength));
            _message.AddAttachment("Instructions for Area 23 Medical Forms.docx", Convert.ToBase64String(instructionBytes, 0, instructionBytes.Length));

        }


        public string BuildTemplate()
        {
            if (_registrant.IsVolunteer)
            {
                return VolunteerTemplate;
            }

            return _registrant.IsWaitListed ? SetAthleteTemplates(WaitListNoMedicalTemplate, WaitListTemplate, WaitListExpiringTemplate, WaitListExpiredTemplate) : SetAthleteTemplates(NoMedicalTemplate, AthleteTemplate, ExpiringTemplate, ExpiredTemplate);
        }

        private string SetAthleteTemplates(string noMed, string normal, string expiring, string expired)
        {
            if (_registrant.AthleteId == 0 || !_registrant.MedicalExpirationDate.HasValue)
            {
                return noMed;
            }

            if (_registrant.MedicalExpirationDate.Value > DateTime.Now)
            {
                return _registrant.MedicalExpirationDate.Value >= DateTime.Now.AddMonths(3) ? normal : expiring;
            }

            return expired;
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

    public class RegistrantSendGrid
    {
        public string Name { get; set; }
        public string Sport { get; set; }
        public string MedicalExpirationDate { get; set; }
    }

}
