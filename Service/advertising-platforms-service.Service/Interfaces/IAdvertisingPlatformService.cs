using advertising_platforms_service.Domain.Responses;
using advertising_platforms_service.Domain.ViewModels;
using System.Collections.Concurrent;

namespace advertising_platforms_service.Service.Interfaces
{
    public interface IAdvertisingPlatformService
    {
        IBaseResponse<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>> GetPlatforms(int page, AdvertisingPlatformFilterModel advertisingPlatformFilterModel);
        IBaseResponse<AdvertisingPlatformViewModel> Update(ConcurrentDictionary<string, List<string>> newPlatforms);
    }
}
