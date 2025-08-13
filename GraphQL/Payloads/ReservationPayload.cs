namespace TerminoApp_NewBackend.GraphQL.Payloads
{
    public class ReservationPayload
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }

        public ReservationPayload(string id, bool success, string? message = null)
        {
            Id = id;
            Success = success;
            Message = message;
        }
    }
}