namespace Aub.Eece503e.ChatService.DataContracts
{
    public class DownloadImageResponse
    {
        public DownloadImageResponse(byte[] bytes)
        {
            Image = bytes;
        }

        public byte[] Image { get; set; }
    }
}