using advertising_platforms_service.Domain.Responses;
using System.Collections.Concurrent;

namespace advertising_platforms_service.Service.Interfaces
{
    public interface IWorkToTxtFile
    {
        Task<IBaseResponse<ConcurrentDictionary<string, List<string>>>> ReadFile();
    }
}
