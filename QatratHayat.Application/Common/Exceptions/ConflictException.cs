namespace QatratHayat.Application.Common.Exceptions
{
    public class ConflictException : Exception
    {
        public string Code { get; }

        public ConflictException(string message, string code) : base(message)
        {
            Code = code;
        }
    }
}