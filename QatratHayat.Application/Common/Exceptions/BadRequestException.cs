namespace QatratHayat.Application.Common.Exceptions
{
    public class BadRequestException : Exception
    {
        public string Code { get; }
        public List<string> Errors { get; }

        public BadRequestException(string message, string code) : base(message)
        {
            Code = code;
            Errors = new List<string>();
        }

        public BadRequestException(string message, string code, List<string> errors) : base(message)
        {
            Code = code;
            Errors = errors;
        }
    }
}