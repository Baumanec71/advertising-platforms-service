using advertising_platforms_service.Controllers;
using advertising_platforms_service.Domain.Responses;
using advertising_platforms_service.Domain.ViewModels;
using advertising_platforms_service.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Testing.Platform.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Net;
using Xunit;
using Assert = Xunit.Assert;

namespace advertising_platforms_service.ControllersTests
{
    public class AdvertisingPlatformControllerTests
    {
        private readonly Mock<IAdvertisingPlatformService> _mockService;
        private readonly Mock<IWorkToTxtFile> _mockFileService;
        private readonly Mock<ILogger<AdvertisingPlatformController>> _mockLogger;
        private readonly AdvertisingPlatformController _controller;

        public AdvertisingPlatformControllerTests()
        {
            _mockService = new Mock<IAdvertisingPlatformService>();
            _mockFileService = new Mock<IWorkToTxtFile>();
            _mockLogger = new Mock<ILogger<AdvertisingPlatformController>>();
            _controller = new AdvertisingPlatformController(_mockService.Object, _mockFileService.Object);
        }

        [Fact]
        public void GetPlatformsWithLocationsWithValidRequestShouldReturnOk()
        {
            var expectedData = new PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>(
                new List<AdvertisingPlatformViewModel> { new("Платформа1") }, 1, null);

            var expectedResponse = new BaseResponse<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>>
            {
                Value = expectedData,
                StatusCode = HttpStatusCode.OK
            };

            _mockService
                .Setup(s => s.GetPlatforms(It.IsAny<int>(), It.IsAny<AdvertisingPlatformFilterModel>()))
                .Returns(expectedResponse);

            var result = _controller.GetPlatformsWithLocations();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>>(okResult.Value);

            Assert.NotNull(response);
            Assert.Single(response.viewModels);
            Assert.Equal("Платформа1", response.viewModels.First().name);

            _mockService
                .Verify(x => x.GetPlatforms(It.IsAny<int>(), It.IsAny<AdvertisingPlatformFilterModel>()), Times.Once);
        }

        [Fact]
        public void GetPlatformsWithLocationsWithErrorShouldReturnBadRequest()
        {
            var errorResponse = new BaseResponse<PaginatedViewModelResponse<AdvertisingPlatformViewModel, AdvertisingPlatformFilterModel>>
            {
                Description = "Ошибка",
                StatusCode = HttpStatusCode.BadRequest
            };

            _mockService
                .Setup(s => s.GetPlatforms(It.IsAny<int>(), It.IsAny<AdvertisingPlatformFilterModel>()))
                .Returns(errorResponse);

            var result = _controller.GetPlatformsWithLocations();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("Ошибка", response);

            _mockService
                .Verify(x => x.GetPlatforms(It.IsAny<int>(), It.IsAny<AdvertisingPlatformFilterModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePlatformsWithSuccessfulReadAndUpdateShouldReturnOk()
        {
            var readResult = new BaseResponse<ConcurrentDictionary<string, List<string>>>
            {
                StatusCode = HttpStatusCode.OK,
                Value = new ConcurrentDictionary<string, List<string>>()
            };

            var updateResult = new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                Description = "Обновление завершено"
            };

            _mockFileService
                .Setup(f => f.ReadFile())
                .ReturnsAsync(readResult);
            _mockService
                .Setup(s => s.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()))
                .Returns(updateResult);

            var result = await _controller.UpdatePlatforms();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var responseType = okResult.Value!.GetType();
            var descriptionProperty = responseType.GetProperty("description");
            Assert.NotNull(descriptionProperty);

            var descriptionValue = descriptionProperty.GetValue(okResult.Value) as string;
            Assert.Equal("Обновление завершено", descriptionValue);

            _mockFileService.Verify(f => f.ReadFile(), Times.Once);
            _mockService.Verify(s => s.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePlatformsWithFileReadErrorShouldReturnError()
        {
            var readResult = new BaseResponse<ConcurrentDictionary<string, List<string>>>
            {
                StatusCode = HttpStatusCode.NotFound,
                Description = "Файл не найден"
            };

            _mockFileService.Setup(f => f.ReadFile()).ReturnsAsync(readResult);

            var result = await _controller.UpdatePlatforms();

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, statusCodeResult.StatusCode);
            var response = Assert.IsType<string>(statusCodeResult.Value);
            Assert.Equal("Файл не найден", response);

            _mockFileService
                .Verify(f => f.ReadFile(), Times.Once);
            _mockService
                .Verify(s => s.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePlatformsWithUpdateErrorShouldReturnBadRequest()
        {
            var readResult = new BaseResponse<ConcurrentDictionary<string, List<string>>>
            {
                StatusCode = HttpStatusCode.OK,
                Value = new ConcurrentDictionary<string, List<string>>()
            };

            var updateResult = new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Description = "Ошибка обновления"
            };

            _mockFileService
                .Setup(f => f.ReadFile())
                .ReturnsAsync(readResult);
            _mockService
                .Setup(s => s.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()))
                .Returns(updateResult);

            var result = await _controller.UpdatePlatforms();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("Ошибка обновления", response);

            _mockFileService
                .Verify(f => f.ReadFile(), Times.Once);
            _mockService
                .Verify(s => s.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()), Times.Once);
        }
    }
}
