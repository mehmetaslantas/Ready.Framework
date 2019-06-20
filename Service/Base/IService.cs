using Ready.Framework.Authorization;
using Ready.Framework.Versioning;

namespace Ready.Framework.Service.Base
{
    public interface IService<TServiceFacade, TClient, TUser>
        where TServiceFacade : IServiceVersionFacade
        where TClient : class, IClient
        where TUser : class, IUser
    {
        IAuthProvider<TClient, TUser> AuthProvider { get; set; }
        TServiceFacade Facade { get; }
    }
}