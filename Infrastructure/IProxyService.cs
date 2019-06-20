using Ready.Framework.Authorization;
using Ready.Framework.Logging;

namespace Ready.Framework.Infrastructure
{
    public interface IProxyService<TClient, TUser>
        where TClient : class, IClient
        where TUser : class, IUser
    {
        void SetParameters(ref IAuthProvider<TClient, TUser> authProvider);
    }
}