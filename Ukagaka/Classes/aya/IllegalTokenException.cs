using System;
 
namespace aya
{
    // Exception indicating that the Parser received an incorrect token during parsing.
    public class IllegalTokenException : Exception
    {
        private readonly Token token;

        public IllegalTokenException(Token token) : base(token.ToString())
        {
            this.token = token;
        }

        public Token GetToken()
        {
            return token;
        }
    }
}
