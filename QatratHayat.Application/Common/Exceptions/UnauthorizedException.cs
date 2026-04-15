namespace QatratHayat.Application.Common.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public string Code { get; }
        public UnauthorizedException(string message, string code) : base(message)
        {
            Code = code;
        }
    }
}