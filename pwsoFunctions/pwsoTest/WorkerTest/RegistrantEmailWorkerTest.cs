using System;
using System.Collections.Generic;
using System.Text;
using pwsoProcesses.Models;
using pwsoProcesses.Workers;
using SendGrid.Helpers.Mail;
using Xunit;

namespace pwsoTest.WorkerTest
{
     public class RegistrantEmailWorkerTest
     {
         private RegistrantEmailWorker _worker;
         private SendGridMessage _message;


         public RegistrantEmailWorkerTest()
         {
            _message = new SendGridMessage();
        }


        [Fact]
        public void BuildEmailToTest_1_Email_email_matches()
        {
            var registrant = new RegistrantDb {Emails = new List<string> {"superman@dc.com"}};


            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailTo();
            Assert.Equal(_message.Personalizations[0].Tos[0].Email, registrant.Emails[0]);
        }

        [Fact]
        public void BuildEmailToTest_2_Email_email_matches()
        {
            var registrant = new RegistrantDb { Emails = new List<string> { "superman@dc.com", "batman@dc.com" } };

            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailTo();
            Assert.Equal(_message.Personalizations[0].Tos[0].Email, registrant.Emails[0]);
            Assert.Equal(_message.Personalizations[0].Tos[1].Email, registrant.Emails[1]);
        }

        [Fact]
        public void BuildEmailToTest_3_Email_email_matches()
        {
            var registrant = new RegistrantDb { Emails = new List<string> { "superman@dc.com", "batman@dc.com", "wonderwoman@dc.com" } };

            _worker = new RegistrantEmailWorker(_message, registrant);
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
            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailFrom();
            Assert.Equal(_message.From.Email, defaultEmail);
        }

        [Fact]
        public void BuildEmailFromTest_email_email_matches()
        {
            var registrant = new RegistrantDb {Sender = "superman@dc.com"};
            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailFrom();
            Assert.Equal(registrant.Sender, _message.From.Email );
        }

        [Fact]
        public void BuildSubjectTest_no_nickName_formated_name_subject()
        {
            var registrant = new RegistrantDb {FirstName = "Dick", LastName = "Grayson", Sport = "Track"};
            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailSubject();
            Assert.Equal("Dick Grayson is registered for Track", _message.Subject);
        }

        [Fact]
        public void BuildSubjectTest_nickName_formated_name_subject()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", NickName = "Robin"};
            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailSubject();
            Assert.Equal("Dick (Robin) Grayson is registered for Track", _message.Subject);
        }

        [Fact]
        public void BuildEmailBodyTest_no_nickName_no_location_formated_name_sport_body()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", Sport = "Track", IsVolunteer = false};
            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailBody();
            Assert.Equal("<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;Dick Grayson has been successfully registered as an athlete for Track.", _message.HtmlContent);
        }

        [Fact]
        public void BuildEmailBodyTest_nickName_location_formated_name_sport_body()
        {
            var registrant = new RegistrantDb { FirstName = "Dick", LastName = "Grayson", NickName = "Robin", Sport = "Track", ProgramName = "Gainesville", IsVolunteer = true};
            _worker = new RegistrantEmailWorker(_message, registrant);
            _worker.BuildEmailBody();
            Assert.Equal("<br>Hi <br><br>&nbsp;&nbsp;&nbsp;&nbsp;Dick (Robin) Grayson has been successfully registered as an volunteer for Track at Gainesville.", _message.HtmlContent);
        }
    }
}
