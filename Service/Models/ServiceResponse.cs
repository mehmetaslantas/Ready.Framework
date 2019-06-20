using System.Diagnostics;
using Ready.Framework.Service.Messaging;

namespace Ready.Framework.Service.Models
{
    public class ServiceResponse
    {
        public bool Result { get; protected set; }

        public Message Message { get; set; }

        public bool HasAuth { get; set; }

        public static ServiceResponse Success()
        {
            return new ServiceResponse { Result = true };
        }
    }

    public class ServiceResponse<T> : ServiceResponse where T : IServiceModel
    {
        public T Data { get; protected set; }

        public static ServiceResponse<T> SuccessResponse(T result)
        {
            return new ServiceResponse<T> { Data = result, Result = true };
        }

        public void Success(T result)
        {
            Data = result;
            Result = true;
        }
    }
}