namespace Ready.Framework.Authorization
{
    public interface IAuthProvider<TClient, TUser>
        where TClient : class, IClient
        where TUser : class, IUser
    {
        TClient GetClient();

        TUser GetUser();

        void SetClient(TClient client);

        void SetUser(TUser user);
    }
}