using System;

namespace PrintNodeNet
{
    public class PrintNodeDelegatedClientContext
    {
        internal PrintNodeDelegatedClientContextAuthenticationMode AuthenticationMode;

        internal string AuthenticationValue { get; private set; }

        public PrintNodeDelegatedClientContext(int accountId)
        {
            AuthenticationValue = accountId.ToString();
            AuthenticationMode = PrintNodeDelegatedClientContextAuthenticationMode.Id;
        }

        public PrintNodeDelegatedClientContext(string email)
        {
            AuthenticationValue = email;
            AuthenticationMode = PrintNodeDelegatedClientContextAuthenticationMode.Email;
        }
    }
}