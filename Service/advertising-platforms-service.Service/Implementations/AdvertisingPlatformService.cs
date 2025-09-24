using advertising_platforms_service.DAL.Interfaces;
using advertising_platforms_service.Domain.Responses;
using advertising_platforms_service.Domain.ViewModels;
using advertising_platforms_service.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace advertising_platforms_service.Service.Implementations
{
    public class AdvertisingPlatformService : IAdvertisingPlatformService
    {
        private readonly IAdvertisingPlatformRepository _advertisingPlatformRepository;
        private readonly ILogger _logger;
        const int pageSize = 10;
        public AdvertisingPlatformService(IAdvertisingPlatformRepository advertisingPlatformRepository, ILogger logger)
        {
            _advertisingPlatformRepository = advertisingPlatformRepository;
            _logger = logger;
        }
        public IBaseResponse<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>> GetPlatforms(int page, AdvertisingPlatformFilterModel advertisingPlatformFilterModel)
        {
            try
            {
                var allAdvertisingPlatformViewModels = _advertisingPlatformRepository
                        .GetPlatformsWithLocations();
                var advertisingPlatformViewModels = new List<AdvertisingPlatformViewModel>();

                if (!string.IsNullOrEmpty(advertisingPlatformFilterModel?.Location))
                {
                    advertisingPlatformViewModels = allAdvertisingPlatformViewModels
                        .Where(x => x.Key
                            .Contains(advertisingPlatformFilterModel.Location))
                        .Select(x => new AdvertisingPlatformViewModel(x.Key, x.Value))
                        .ToList();
                }
                else
                {
                    advertisingPlatformViewModels = allAdvertisingPlatformViewModels
                        .Select(x => new AdvertisingPlatformViewModel(x.Key, x.Value))
                        .ToList();
                }

                var totalPages = (int)Math.Ceiling((double)advertisingPlatformViewModels.Count / pageSize);

                advertisingPlatformViewModels = advertisingPlatformViewModels
                       .Skip((page - 1) * pageSize)
                       .Take(pageSize)
                       .ToList();

                page = Math.Clamp(page, 1, totalPages > 0 ? totalPages : 1);

                return new BaseResponse<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>>()
                {
                    StatusCode = HttpStatusCode.OK,
                    Description = $"Страница {page} из {totalPages}",
                    Value = new PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>(
                            advertisingPlatformViewModels,
                            totalPages,
                            advertisingPlatformFilterModel
                        ),
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Description = $"Ошибка при получении платформ: {ex.Message}",
                };
            }
        }

        public IBaseResponse<AdvertisingPlatformViewModel> Update(ConcurrentDictionary<string, List<string>> newPlatforms)
        {
            try
            {
                if (newPlatforms == null || !newPlatforms.Any())
                {
                    return new BaseResponse<AdvertisingPlatformViewModel>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Description = "Нет данных для обновления",
                    };
                }

                _advertisingPlatformRepository.Clear();
                _logger.LogInformation("Хранилище с данными очищено");

                _advertisingPlatformRepository.Update(newPlatforms);
                _logger.LogInformation("Хранилище с данными заполнено");

                return new BaseResponse<AdvertisingPlatformViewModel>()
                {
                    StatusCode = HttpStatusCode.OK,
                    Description = $"Данные обновлены успешно!",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<AdvertisingPlatformViewModel>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Description = $"Ошибка при получении платформ: {ex.Message}",
                };
            }
        } 
    }
}
