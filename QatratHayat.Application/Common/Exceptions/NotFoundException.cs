namespace QatratHayat.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string Code { get; }

        public NotFoundException(string message, string code) : base(message)
        {
            Code = code;
        }
    }
}