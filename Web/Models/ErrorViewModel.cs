namespace SpeiderGames.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class ErrorModel
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class RequestErrorModel
    {
        public string GameName { get; set; }
        public string GameCode { get; set; }
    }
}
