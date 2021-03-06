﻿using System;
using System.Collections.Generic;
using System.Text;
using pwsoProcesses;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;
using SendGrid.Helpers.Mail;
//using Twilio.Rest.Api.V2010.Account;
using Xunit;

namespace pwsoTest.WorkerTest
{
     public class RegistrantMessageWorkerTest
     {
         private RegistrantMessageWorker _worker;
         private SendGridMessage _message;


         public RegistrantMessageWorkerTest()
         {
            _message = new SendGridMessage();
        }


        [Fact]
        public void BuildEmailToTest_1_Email_email_matches()
        {
            var registrant = new RegistrantDb {Emails = new List<string> {"superman@dc.com"}};


            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailTo();
            Assert.Equal(_message.Personalizations[0].Tos[0].Email, registrant.Emails[0]);
        }

        [Fact]
        public void BuildEmailToTest_2_Email_email_matches()
        {
            var registrant = new RegistrantDb { Emails = new List<string> { "superman@dc.com", "batman@dc.com" } };

            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailTo();
            Assert.Equal(_message.Personalizations[0].Tos[0].Email, registrant.Emails[0]);
            Assert.Equal(_message.Personalizations[0].Tos[1].Email, registrant.Emails[1]);
        }

        [Fact]
        public void BuildEmailToTest_3_Email_email_matches()
        {
            var registrant = new RegistrantDb { Emails = new List<string> { "superman@dc.com", "batman@dc.com", "wonderwoman@dc.com" } };

            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailTo();
            Assert.Equal(_message.Personalizations[0].Tos[0].Email, registrant.Emails[0]);
            Assert.Equal(_message.Personalizations[0].Tos[1].Email, registrant.Emails[1]);
            Assert.Equal(_message.Personalizations[0].Tos[2].Email, registrant.Emails[2]);
        }

        [Fact]
        public void BuildEmailFromTest_no_email_email_default()
        {
            var registrant = new RegistrantDb { };
            const string defaultEmail = "webmaster@pwsova.org";
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailFrom();
            Assert.Equal(_message.From.Email, defaultEmail);
        }

        [Fact]
        public void BuildEmailFromTest_email_email_matches()
        {
            var registrant = new RegistrantDb {Sender = "superman@dc.com"};
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailFrom();
            Assert.Equal(registrant.Sender, _message.From.Email );
        }


        [Fact]
        public void BuildEmailCopyTest_from_eq_cc()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", NickName = "Robin", Sport = "Track", ProgramName = "Gainesville", IsVolunteer = false, Sender = "superman@dc.com", AthleteId = 0};
            _message.From = new EmailAddress(registrant.Sender);
            _message.AddTo("batman@dc.com");
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailCopy();
            Assert.Equal(registrant.Sender, _message.Personalizations[0].Ccs[0].Email);
        }

        [Fact]
        public void BuildEmailCopyTest_when_from_eq__to_no_cc()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", NickName = "Robin", Sport = "Track", ProgramName = "Gainesville", IsVolunteer = true, Sender = "superman@dc.com" };
            _message.From = new EmailAddress(registrant.Sender);
            _message.AddTo(registrant.Sender);
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailCopy();
            Assert.Null(_message.Personalizations[0].Ccs);
        }


        [Fact]
        public void BuildPhoneListTest_1_phone_0_text_0_list()
        {
            var registrant = new RegistrantDb();
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone{CanText = false, Phone = "7035551212"});
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> actual = _worker.BuildPhoneList();
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void BuildPhoneListTest_3_phone_0_text_0_list()
        {
            var registrant = new RegistrantDb();
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> actual = _worker.BuildPhoneList();
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void BuildPhoneListTest_1_phone_1_text_1_list()
        {
            var registrant = new RegistrantDb();
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> actual = _worker.BuildPhoneList();
            Assert.Equal(1, actual.Count);
            Assert.Equal("+17035551212", actual[0]);
        }

        [Fact]
        public void BuildPhoneListTest_3_phone_3_text_3_list()
        {
            var registrant = new RegistrantDb();
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> actual = _worker.BuildPhoneList();
            Assert.Equal(3, actual.Count);
            Assert.Equal("+17035551212", actual[0]);
            Assert.Equal("+12125551212", actual[1]);
            Assert.Equal("+13015551212", actual[2]);
        }

        [Fact]
        public void BuildPhoneListTest_3_phone_1_text_1_list()
        {
            var registrant = new RegistrantDb();
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> actual = _worker.BuildPhoneList();
            Assert.Equal(1, actual.Count);
            Assert.Equal("+12125551212", actual[0]);
        }

        //[Fact]
        //public void BuildPhoneMessageListTest_0_phone_0_list()
        //{
        //    var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
        //    registrant.Phones = new List<RegistrantPhone>();
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "7035551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
        //    _worker = new RegistrantMessageWorker(registrant, "20255512121");
        //    List<string> phonelist = new List<string>();
        //    List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
        //    Assert.Equal(0, actual.Count);

        //}

        //[Fact]
        //public void BuildPhoneMessageListTest_1_phone_1_list()
        //{
        //    var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
        //    registrant.Phones = new List<RegistrantPhone>();
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
        //    _worker = new RegistrantMessageWorker(registrant, "20255512121");
        //    List<string> phonelist = new List<string>();
        //    phonelist.Add("+17035551212");
        //    List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
        //    Assert.Equal(1, actual.Count);

        //}

        //[Fact]
        //public void BuildPhoneMessageListTest_2_phone_2_to_number()
        //{
        //    var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
        //    registrant.Phones = new List<RegistrantPhone>();
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "7035551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "2125551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "3015551212" });
        //    _worker = new RegistrantMessageWorker(registrant, "20255512121");
        //    List<string> phonelist = new List<string>();
        //    phonelist.Add("+12125551212");
        //    phonelist.Add("+13015551212");
        //    List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
        //    Assert.Equal(phonelist[0], actual[0].To.ToString());
        //    Assert.Equal(phonelist[1], actual[1].To.ToString());

        //}

        //[Fact]
        //public void BuildPhoneMessageListTest_3_phone_3_to_number()
        //{
        //    var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
        //    registrant.Phones = new List<RegistrantPhone>();
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "2125551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "3015551212" });
        //    _worker = new RegistrantMessageWorker(registrant, "20255512121");
        //    List<string> phonelist = new List<string>();
        //    phonelist.Add("+17035551212"); 
        //    phonelist.Add("+12125551212");
        //    phonelist.Add("+13015551212");
        //    List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
        //    Assert.Equal(phonelist[0], actual[0].To.ToString());
        //    Assert.Equal(phonelist[1], actual[1].To.ToString());
        //    Assert.Equal(phonelist[2], actual[2].To.ToString());

        //}
        //[Fact]
        //public void BuildPhoneMessageListTest_1_phone_1_to_number()
        //{
        //    var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
        //    registrant.Phones = new List<RegistrantPhone>();
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
        //    _worker = new RegistrantMessageWorker(registrant, "20255512121");
        //    List<string> phonelist = new List<string>();
        //    phonelist.Add("+17035551212");
        //    List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
        //    Assert.Equal(phonelist[0], actual[0].To.ToString());

        //}

        //[Fact]
        //public void BuildPhoneMessageListTest_1_phone_1_body()
        //{
        //    var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
        //    registrant.Phones = new List<RegistrantPhone>();
        //    registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
        //    registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
        //    _worker = new RegistrantMessageWorker(registrant, "20255512121");
        //    List<string> phonelist = new List<string>();
        //    phonelist.Add("+17035551212");
        //    List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
        //    Assert.Equal("Dick Grayson has been successfully registered as an athlete for Track.", actual[0].Body);

        //}

        [Fact]
        public void BuildRegistantModelTest_1_Email_1_Phone_check_regrant_fields()
        {
            var registrant = new RegistrantDb
            {
                Emails = new List<string> { "superman@dc.com" },
                FirstName = "Dick",
                LastName = "Grayson",
                Sport = "Track",
                IsVolunteer = false,
                ProgramName = "Woodbridge",
                SportId = 8,
                ProgramId = 11,
                Size = "small",
            };

            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });


            _worker = new RegistrantMessageWorker(registrant);
            var actual = _worker.BuildRegistrant();
            Assert.Equal(registrant.FirstName, actual.FirstName);
            Assert.Equal(registrant.LastName, actual.LastName);
            Assert.Equal(0, actual.Id);
            Assert.Equal(registrant.IsVolunteer, actual.IsVolunteer);
            Assert.Equal(registrant.ProgramId, actual.ProgramId);
            Assert.Equal(registrant.Size, actual.Size);
            Assert.Equal(registrant.SportId, actual.SportId);
            Assert.Equal(registrant.NickName, actual.NickName);
        }

        [Fact]
        public void BuildRegistantModelTest_1_Email_1_Phone_check_email()
        {
            var registrant = new RegistrantDb
            {
                Emails = new List<string> { "superman@dc.com" },
                FirstName = "Dick",
                LastName = "Grayson",
                Sport = "Track",
                IsVolunteer = false,
                ProgramName = "Woodbridge",
                SportId = 8,
                ProgramId = 11,
                Size = "small",
            };

            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });


            _worker = new RegistrantMessageWorker(registrant);
            var actual = _worker.BuildRegistrant();
            Assert.Equal(1, actual.RegistrantEmail.Count);
        }

        [Fact]
        public void BuildRegistantModelTest_1_Email_1_Phone_check_phone()
        {
            var registrant = new RegistrantDb
            {
                Emails = new List<string> { "superman@dc.com" },
                FirstName = "Dick",
                LastName = "Grayson",
                Sport = "Track",
                IsVolunteer = false,
                ProgramName = "Woodbridge",
                SportId = 8,
                ProgramId = 11,
                Size = "small",
            };

            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });


            _worker = new RegistrantMessageWorker(registrant);
            var actual = _worker.BuildRegistrant();
            Assert.Equal(1, actual.RegistrantPhone.Count);
        }

        [Fact]
        public void BuildRegistantModelTest_2_Email_3_Phone_check_regrant_fields()
        {
            var registrant = new RegistrantDb
            {
                Emails = new List<string> { "superman@dc.com" },
                FirstName = "Dick",
                LastName = "Grayson",
                Sport = "Track",
                IsVolunteer = true,
                ProgramName = "Woodbridge",
                SportId = 8,
                ProgramId = 11,
                NickName = "Robin"
            };

            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            registrant.Emails.Add("batman@dc.com");

            _worker = new RegistrantMessageWorker(registrant);
            var actual = _worker.BuildRegistrant();
            Assert.Equal(registrant.FirstName, actual.FirstName);
            Assert.Equal(registrant.LastName, actual.LastName);
            Assert.Equal(0, actual.Id);
            Assert.Equal(registrant.IsVolunteer, actual.IsVolunteer);
            Assert.Equal(registrant.ProgramId, actual.ProgramId);
            Assert.Equal(registrant.Size, actual.Size);
            Assert.Equal(registrant.SportId, actual.SportId);
            Assert.Equal(registrant.NickName, actual.NickName);
            Assert.Equal(3, actual.RegistrantPhone.Count);
            Assert.Equal(2, actual.RegistrantEmail.Count);

        }

        [Fact]
        public void BuildRegistantModelTest_no_medical_check_no_athlete_fields()
        {
            var registrant = new RegistrantDb
            {
                Emails = new List<string> { "superman@dc.com" },
                FirstName = "Dick",
                LastName = "Grayson",
                Sport = "Track",
                IsVolunteer = true,
                ProgramName = "Woodbridge",
                SportId = 8,
                ProgramId = 11,
                NickName = "Robin"
            };

            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            registrant.Emails.Add("batman@dc.com");

            _worker = new RegistrantMessageWorker(registrant);
            var actual = _worker.BuildRegistrant();
            Assert.Equal(0, actual.RegisteredAthlete.Count);

        }


        [Fact]
        public void BuildRegistantModelTest_medical_check_1_athlete_fields()
        {
            var registrant = new RegistrantDb
            {
                Emails = new List<string> { "superman@dc.com" },
                FirstName = "Dick",
                LastName = "Grayson",
                Sport = "Track",
                IsVolunteer = true,
                ProgramName = "Woodbridge",
                SportId = 8,
                ProgramId = 11,
                NickName = "Robin",
                AthleteId = 123
            };

            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            registrant.Emails.Add("batman@dc.com");

            _worker = new RegistrantMessageWorker(registrant);
            var actual = _worker.BuildRegistrant();
            Assert.Equal(1, actual.RegisteredAthlete.Count);

        }

        [Fact]
        public void BuildEmailMedicalToTest_Found_Email_email_matches()
        {
            var registrant = new RegistrantDb { Emails = new List<string> { "superman@dc.com" } };

            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.MedicalEmail = "batman@dc.com";
            _worker.BuildEmailMedicalTo();
            Assert.Equal(_worker.MedicalEmail, _message.Personalizations[0].Tos[0].Email);
        }

        [Fact]
        public void BuildEmailMedicalToTest_No_Found_Email_email_matches()
        {
            var registrant = new RegistrantDb { Emails = new List<string> { "superman@dc.com" } };

            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailMedicalTo();
            Assert.Equal("webmaster@pwsova.org", _message.Personalizations[0].Tos[0].Email);
        }

    }
}
