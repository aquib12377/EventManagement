namespace EventManagement.Helper
{
    public class Response
    {
        public Response(bool status,string message,object? payload = null)
        {

            Status = status;
            Message = message;
            Payload = payload;
        }
        public bool Status { get; set; }
        public string Message { get; set; }
        public object? Payload { get; set; }
    }
}
