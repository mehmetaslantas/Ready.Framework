using Ready.Framework.Authorization;
using Ready.Framework.Service.Messaging;
using Ready.Framework.Service.Models;
using Ready.Framework.Versioning;

namespace Ready.Framework.Service.Base
{
    public class AbstractService<TServiceFacade, TClient, TUser> : IService<TServiceFacade, TClient, TUser>
        where TServiceFacade : class, IServiceVersionFacade
        where TClient : class, IClient
        where TUser : class, IUser
    {
        public AbstractService(TServiceFacade facade)
        {
            Facade = facade;
        }

        public TClient Client => AuthProvider.GetClient();

        public TUser User => AuthProvider.GetUser();

        public TServiceFacade Facade { get; }

        public IAuthProvider<TClient, TUser> AuthProvider { get; set; }

        public ServiceResponse MessageResponse(Message errorMessage)
        {
            return new ServiceResponse { Message = errorMessage };
        }

        public ServiceResponse MessageResponse(string code)
        {
            return new ServiceResponse { Message = Message.Alert(code) };
        }

        public ServiceResponse<TEntity> MessageResponse<TEntity>(Message errorMessage) where TEntity : class, IServiceModel
        {
            return new ServiceResponse<TEntity> { Message = errorMessage };
        }
    }
}