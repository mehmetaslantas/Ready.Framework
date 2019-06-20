namespace Ready.Framework.Service.Models
{
    public class ServiceRequest<T> where T : IServiceModel
    {
        public T Parameters { get; set; }
    }
}