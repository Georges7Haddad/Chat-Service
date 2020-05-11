namespace Aub.Eece503e.ChatService.DataContracts
{
    public class AddMessageRequestBody
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string SenderUsername { get; set; }
    }
}