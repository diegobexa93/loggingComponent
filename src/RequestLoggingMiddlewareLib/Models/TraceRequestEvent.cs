namespace RequestLoggingMiddlewareLib.Models
{
    public class TraceRequestEvent: IntegrationBaseEvent
    {
        public TraceRequestEvent(Guid requestId, DateTime requestTimestamp)
        {
            RequestId = requestId;
            RequestTimestamp = requestTimestamp;
            RequestHeaders = new Dictionary<string, string?>();
            TraceResponse = new TraceResponseEvent(requestId);
        }

        public Guid RequestId { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string? RequestURL { get; set; }
        public Dictionary<string, string?> RequestHeaders { get; set; }
        public string? RequestBody { get; set; }
        public TraceResponseEvent TraceResponse { get; set; }
    }

    public class TraceResponseEvent
    {
        public TraceResponseEvent(Guid responseId)
        {
            ResponseId = responseId;
            ResponseHeaders = new Dictionary<string, string?>();
        }

        public Guid ResponseId { get; set; }
        public int ResponseStatusCode { get; set; }
        public Dictionary<string, string?> ResponseHeaders { get; set; }
        public string? ResponseBody { get; set; }

    }
}

