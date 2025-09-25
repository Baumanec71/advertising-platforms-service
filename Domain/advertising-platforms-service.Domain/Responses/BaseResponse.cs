using System.Net;

namespace advertising_platforms_service.Domain.Responses
{
    public class BaseResponse<T> : IBaseResponse<T>
    {
        public T? Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }
    }

    public interface IBaseResponse<T>
    {
        T? Value { get; }
        string Description { get; }
        HttpStatusCode StatusCode { get; }
    }
}
