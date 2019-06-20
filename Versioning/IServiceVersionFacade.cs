namespace Ready.Framework.Versioning
{
    public interface IServiceVersionFacade
    {
        string Version { get; set; }

        bool IsEnable(string clientVersion);
    }
}