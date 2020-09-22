using System;
using System.Collections.Generic;
using System.Text;
using pwsoProcesses;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;
using SendGrid.Helpers.Mail;
using Twilio.Rest.Api.V2010.Account;
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
        public void BuildSubjectTest_no_nickName_formated_name_subject()
        {
            var registrant = new RegistrantDb {FirstName = "Dick", LastName = "Grayson", Sport = "Track"};
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailSubject();
            Assert.Equal("Dick Grayson is registered for Track", _message.Subject);
        }

        [Fact]
        public void BuildSubjectTest_nickName_formated_name_subject()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", NickName = "Robin"};
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailSubject();
            Assert.Equal("Dick (Robin) Grayson is registered for Track", _message.Subject);
        }

        [Fact]
        public void BuildEmailBodyTest_no_nickName_no_location_formated_name_sport_body()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false};
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailBody();
            Assert.Equal("<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;Dick Grayson has been successfully registered as an athlete for Track.", _message.HtmlContent);
        }

        [Fact]
        public void BuildEmailBodyTest_nickName_location_formated_name_sport_body()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", NickName = "Robin", Sport = "Track", ProgramName = "Gainesville", IsVolunteer = true};
            _worker = new RegistrantMessageWorker(_message, registrant);
            _worker.BuildEmailBody();
            Assert.Equal("<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;Dick (Robin) Grayson has been successfully registered as an volunteer for Track at Gainesville.", _message.HtmlContent);
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

        [Fact]
        public void BuildPhoneMessageListTest_0_phone_0_list()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> phonelist = new List<string>();
            List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
            Assert.Equal(0, actual.Count);

        }

        [Fact]
        public void BuildPhoneMessageListTest_1_phone_1_list()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> phonelist = new List<string>();
            phonelist.Add("+17035551212");
            List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
            Assert.Equal(1, actual.Count);

        }

        [Fact]
        public void BuildPhoneMessageListTest_2_phone_2_to_number()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> phonelist = new List<string>();
            phonelist.Add("+12125551212");
            phonelist.Add("+13015551212");
            List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
            Assert.Equal(phonelist[0], actual[0].To.ToString());
            Assert.Equal(phonelist[1], actual[1].To.ToString());

        }

        [Fact]
        public void BuildPhoneMessageListTest_3_phone_3_to_number()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> phonelist = new List<string>();
            phonelist.Add("+17035551212"); 
            phonelist.Add("+12125551212");
            phonelist.Add("+13015551212");
            List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
            Assert.Equal(phonelist[0], actual[0].To.ToString());
            Assert.Equal(phonelist[1], actual[1].To.ToString());
            Assert.Equal(phonelist[2], actual[2].To.ToString());

        }
        [Fact]
        public void BuildPhoneMessageListTest_1_phone_1_to_number()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> phonelist = new List<string>();
            phonelist.Add("+17035551212");
            List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
            Assert.Equal(phonelist[0], actual[0].To.ToString());

        }

        [Fact]
        public void BuildPhoneMessageListTest_1_phone_1_body()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false };
            registrant.Phones = new List<RegistrantPhone>();
            registrant.Phones.Add(new RegistrantPhone { CanText = true, Phone = "7035551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "2125551212" });
            registrant.Phones.Add(new RegistrantPhone { CanText = false, Phone = "3015551212" });
            _worker = new RegistrantMessageWorker(registrant, "20255512121");
            List<string> phonelist = new List<string>();
            phonelist.Add("+17035551212");
            List<CreateMessageOptions> actual = _worker.BuildPhoneMessageList(phonelist);
            Assert.Equal("Dick Grayson has been successfully registered as an athlete for Track.", actual[0].Body);

        }
    }
}
