using System;
using System.Collections.Generic;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon;

namespace AmazonSimpleEmailService
{
    class Program
    {
        static AmazonSimpleEmailServiceClient _client;
        static void Main(string[] args)
        {
            Console.WriteLine("Sample of AmazonSES API with AWSSDK.SimpleEmail");
            string AccessKeyID = "AKIAIXGKP3LTQBBC5FMQ"; // this is not SMTP username
            string SecretAccessKey = "a long secret key here"; // this is not SMTP password
            _client = new AmazonSimpleEmailServiceClient(AccessKeyID, SecretAccessKey, RegionEndpoint.USEast1);
            
            // Replace these email with valid emails.
            var sourceAddress = "email@domain.com";
            var destinationAddress = "_juniorm@outlook.com";

            // send verification request for email accounts, both source and destination
            // VerifyEmails(sourceAddress, destinationAddress);

            // send email
            var sendEmailRequest = new SendEmailRequest
            {
                Source = sourceAddress,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { destinationAddress }
                },
                Message = new Message
                {
                    Subject = new Content("Subject"),
                    Body = new Body
                    {
                        Html = new Content(@"A message body.")
                    }
                }
            };
            var sendEmailResponse = _client.SendEmailAsync(sendEmailRequest);
            sendEmailResponse.Wait();

            Console.WriteLine($"MessageId {sendEmailResponse.Result.MessageId}, HttpStatusCode {sendEmailResponse.Result.HttpStatusCode}, Status {sendEmailResponse.Status}");
            Console.ReadKey();
        }

        public static void VerifyEmails(params string[] emails)
        {
            // start verification process for all email addresses
            foreach (var email in emails)
            {
                _client.VerifyEmailIdentityAsync(new VerifyEmailIdentityRequest
                {
                    EmailAddress = email
                });
            }

            // wait until all are verified, maximum wait time of two minutes
            bool allVerified = true;
            DateTime latest = DateTime.Now + TimeSpan.FromMinutes(2);
            while (DateTime.Now < latest)
            {
                // get verification status for all emails
                var attr = _client.GetIdentityVerificationAttributesAsync(new GetIdentityVerificationAttributesRequest
                {
                    Identities = new List<string>(emails)
                });
                attr.Wait();
                var verificationAttributes = attr.Result.VerificationAttributes;

                // test verification status
                allVerified = true;
                foreach (var email in emails)
                {
                    var attribute = verificationAttributes[email];
                    if (attribute.VerificationStatus != VerificationStatus.Success)
                        allVerified = false;
                }

                if (allVerified)
                    break;

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));
            }

            if (!allVerified)
                throw new InvalidOperationException("Not all email addresses have been verified");
        }
    }
}
