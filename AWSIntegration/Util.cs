using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AWSIntegration
{
    public class Util
    {
        public static bool IsValidEmail(string emailAddress)
        {
            const string emailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

            bool isEmail = Regex.IsMatch(emailAddress, emailRegex, RegexOptions.IgnoreCase);

            return isEmail;
        }

        public static bool ValidateEmailList(List<string> emailAddresses)
        {
            if (emailAddresses == null || !emailAddresses.Any())
            {
                return true;
            }
            foreach (var emailAddress in emailAddresses)
            {
                if (!IsValidEmail(emailAddress))
                {
                    return false;
                }
            }
            return true;
        }
    }
}