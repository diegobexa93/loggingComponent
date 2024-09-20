namespace RequestLoggingMiddlewareLib.Exceptions
{
    public class Error
    {
        public static readonly Error None = new(ErrorType.None, string.Empty, string.Empty);

        public ErrorType Type { get; }
        public string Code { get; }
        public string Description { get; }

        public Error(ErrorType type, string code, string description)
        {
            Type = type;
            Code = code;
            Description = description;
        }
    }
}
