namespace TranslatorAPI.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
