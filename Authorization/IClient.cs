namespace Ready.Framework.Authorization
{
    public interface IClient
    {
        string ClientId { get; set; }

        string ClientVersion { get; set; }
    }
}