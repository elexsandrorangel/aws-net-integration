using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AWSIntegration
{
    public class SESIntegration
    {
        private static AmazonSimpleEmailServiceClient GetSimpleEmailClientInstance()
        {
            // Choose the AWS region of the Amazon SES endpoint you want to connect to. Note that your sandbox 
            // status, sending limits, and Amazon SES identity-related settings are specific to a given 
            // AWS region, so be sure to select an AWS region in which you set up Amazon SES. Here, we are using 
            // the US West (Oregon) region. Examples of other regions that Amazon SES supports are USEast1 
            // and EUWest1. For a complete list, see http://docs.aws.amazon.com/ses/latest/DeveloperGuide/regions.html 
            RegionEndpoint defaultRegion = RegionEndpoint.USEast1;
            // Instantiate an Amazon SES client, which will make the service call.
            string accessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];

            return new AmazonSimpleEmailServiceClient(accessKey, secretKey, defaultRegion);
        }

        /// <summary>
        /// Sent a simple email using AWS SES
        /// https://aws.amazon.com/ses/
        /// </summary>
        /// <param name="from">Email address of sender</param>
        /// <param name="to">Receiver email address</param>
        /// <param name="subject">Subject of email</param>
        /// <param name="content">Content of email message</param>
        /// <remarks>The sender of the email must be validated by AWS</remarks>
        /// <exception cref="InvalidOperationException">Invalid email addresses</exception>
        /// <exception cref="MessageRejectedException">Email is rejected</exception>
        /// <returns></returns>
        public static bool SendMail(string from, string to, string subject, string content)
        {
            List<string> dest = new List<string>
            {
                to
            };
            return SendMail(from, dest, false, subject, content);
        }

        /// <summary>
        /// Sent a simple email using AWS SES
        /// https://aws.amazon.com/ses/
        /// </summary>
        /// <param name="from">Email address of sender</param>
        /// <param name="to">List of addresses</param>
        /// <param name="hideCopy">Send mail as hide copy (BCC)</param>
        /// <param name="subject">Subject of email</param>
        /// <param name="content">Content of email message</param>
        /// <remarks>The sender of the email must be validated by AWS</remarks>
        /// <exception cref="InvalidOperationException">Invalid email addresses</exception>
        /// <exception cref="MessageRejectedException">Email is rejected</exception>
        /// <returns></returns>
        public static bool SendMail(string from, List<string> to, bool hideCopy, string subject, string content)
        {
            if (!Util.IsValidEmail(from) || !Util.ValidateEmailList(to))
            {
                throw new InvalidOperationException("Invalid email address");
            }

            try
            {
                // Create the subject and body of the message.
                Content subjectContentt = new Content(subject);
                Content textBody = new Content(content);

                Body body = new Body();
                body.Html = textBody;

                // Create a message with the specified subject and body.
                Message message = new Message(subjectContentt, body);

                Destination destination = new Destination();
                if (hideCopy)
                {
                    destination.BccAddresses = to;
                }
                else
                {
                    destination.ToAddresses = to;
                }

                // Assemble the email.
                SendEmailRequest request = new SendEmailRequest(from, destination, message);

                // Instantiate an Amazon SES client, which will make the service call.
                using (AmazonSimpleEmailServiceClient client = GetSimpleEmailClientInstance())
                {
                    // Send Email
                    SendEmailResponse response = client.SendEmail(request);
                    return response.HttpStatusCode == HttpStatusCode.OK;
                }
            }
            catch (MessageRejectedException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks if the email is validated on the mailing list
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <exception cref="InvalidOperationException">Invalid email addresses</exception>
        /// <returns>Email is checked in the mailing list</returns>
        public static bool IsVerifiedEmail(string email)
        {
            if (!Util.IsValidEmail(email))
            {
                throw new InvalidOperationException("Invalid email address");
            }

            using (AmazonSimpleEmailServiceClient client = GetSimpleEmailClientInstance())
            {
                ListVerifiedEmailAddressesRequest request = new ListVerifiedEmailAddressesRequest();
                ListVerifiedEmailAddressesResponse response = client.ListVerifiedEmailAddresses(request);
                List<string> verifiedAddresses = response.VerifiedEmailAddresses;

                return verifiedAddresses.Contains(email);
            }
        }

        /// <summary>
        /// Add a email to Amazon SES Verified Email.
        /// You will receive a confirmation email from Amazon to confirm the email.
        /// </summary>
        /// <param name="email">Email to add</param>
        /// <exception cref="InvalidOperationException">Invalid email addresses</exception>
        /// <returns></returns>
        public static bool SendVerificationEmailAmazonSES(string email)
        {
            if (!Util.IsValidEmail(email))
            {
                throw new InvalidOperationException("Invalid email address");
            }
            using (AmazonSimpleEmailServiceClient client = GetSimpleEmailClientInstance())
            {
                VerifyEmailAddressRequest request = new VerifyEmailAddressRequest();
                request.EmailAddress = email;
                VerifyEmailAddressResponse response = client.VerifyEmailAddress(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }
    }
}
