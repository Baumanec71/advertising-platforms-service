using advertising_platforms_service.DAL.Interfaces;
using advertising_platforms_service.Domain.ViewModels;
using advertising_platforms_service.Service.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace advertising_platforms_service.Implementations
{
    public class AdvertisingPlatformServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<IAdvertisingPlatformRepository> _mockRepository;
        private readonly Mock<ILogger<AdvertisingPlatformService>> _mockLogger;
        private readonly AdvertisingPlatformService _service;

        public AdvertisingPlatformServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockRepository = new Mock<IAdvertisingPlatformRepository>();
            _mockLogger = new Mock<ILogger<AdvertisingPlatformService>>();
            _service = new AdvertisingPlatformService(_mockRepository.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData("", 4, 1)]
        [InlineData("Country/City/District", 3, 1)]
        public void GetPlatformsWhenData(string location, int expectedCount, int expectedTotalPages)
        {
            var testData = new ConcurrentDictionary<string, List<string>>
            {
                ["Country"] = new List<string> { "PlatformA" },
                ["Country/City"] = new List<string> { "PlatformB" },
                ["Country/City/District"] = new List<string> { "PlatformC" },
                ["Country/OtherCity"] = new List<string> { "PlatformD" }
            };

            _mockRepository
                .Setup(r => r.GetPlatformsWithLocations())
                .Returns(testData);

            var filterModel = new AdvertisingPlatformFilterModel { Location = location };
            int page = 1;

            var result = _service.GetPlatforms(page, filterModel);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedCount, result.Value.viewModels.Count);
            Assert.Equal(expectedTotalPages, result.Value.totalPages);

            _mockRepository.Verify(r => r.GetPlatformsWithLocations(), Times.Once);
        }

        [Theory]
        [InlineData("Country/City", 2)]
        [InlineData("Country/City/District", 3)]
        [InlineData("Non/Existent/Location", 0)]
        public void SearchForLocationReturnsPlatforms(string location, int expectedCount)
        {
            var testData = new ConcurrentDictionary<string, List<string>>
            {
                ["Country"] = new List<string> { "PlatformA" },
                ["Country/City"] = new List<string> { "PlatformB" },
                ["Country/City/District"] = new List<string> { "PlatformC" }
            };

            var service = new AdvertisingPlatformService(_mockRepository.Object, _mockLogger.Object);
            var method = typeof(AdvertisingPlatformService).GetMethod("SearchForLocation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.NotNull(method);

            var result = method.Invoke(service, new object[] { location, testData })
                as List<AdvertisingPlatformViewModel>;

            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count);
        }

        [Theory]
        [InlineData(1, 10, 2)]
        [InlineData(2, 5, 2)]
        [InlineData(-1, 10, 2)]
        [InlineData(3, 0, 2)]
        [InlineData(0, 10, 2)]
        public void GetPlatformsWithPagination(int page, int expectedCount, int expectedTotalPages)
        {
            var testData = new ConcurrentDictionary<string, List<string>>();
            var platforms = new List<string>();

            for (int i = 1; i <= 15; i++)
            {
                platforms.Add($"Platform{i:00}");
            }

            testData["Location"] = platforms;

            _mockRepository
                .Setup(r => r.GetPlatformsWithLocations())
                .Returns(testData);

            var filterModel = new AdvertisingPlatformFilterModel { Location = null };

            var result = _service.GetPlatforms(page, filterModel);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedCount, result.Value.viewModels.Count);
            Assert.Equal(expectedTotalPages, result.Value.totalPages);
        }

        [Fact]
        public void GetPlatformsReturnsEmptyWhenNoData()
        {
            var emptyData = new ConcurrentDictionary<string, List<string>>();
            _mockRepository
                .Setup(r => r.GetPlatformsWithLocations())
                .Returns(emptyData);

            var filterModel = new AdvertisingPlatformFilterModel { Location = null };
            int page = 1;

            var result = _service.GetPlatforms(page, filterModel);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.viewModels);
            Assert.Equal(0, result.Value.totalPages);
            Assert.Equal("Страница 1 из 0", result.Description);
        }

        [Fact]
        public void GetPlatformsReturnsErrorWhenRepositoryFails()
        {
            _mockRepository
                .Setup(r => r.GetPlatformsWithLocations())
                .Throws(new Exception("Database connection failed"));

            var filterModel = new AdvertisingPlatformFilterModel { Location = null };
            int page = 1;

            var result = _service.GetPlatforms(page, filterModel);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Contains("Ошибка при получении платформ: Database connection failed", result.Description);
            Assert.Null(result.Value);

            _mockRepository.Verify(r => r.GetPlatformsWithLocations(), Times.Once);
        }

        [Fact]
        public void UpdateSuccessWhenValidData()
        {
            var newData = new ConcurrentDictionary<string, List<string>>
            {
                ["Location1"] = new List<string> { "PlatformA", "PlatformB" },
                ["Location2"] = new List<string> { "PlatformC" }
            };

            _mockRepository.Setup(r => r.Clear());
            _mockRepository.Setup(r => r.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()));

            var result = _service.Update(newData);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Данные обновлены успешно!", result.Description);

            _mockRepository.Verify(r => r.Clear(), Times.Once);
            _mockRepository.Verify(r => r.Update(newData), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("empty")]
        public void UpdateReturnsErrorWhenInvalidData(string? dataType)
        {
            ConcurrentDictionary<string, List<string>>? invalidData = dataType == "empty"
                ? new ConcurrentDictionary<string, List<string>>()
                : null;

            var result = _service.Update(invalidData!);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Нет данных для обновления", result.Description);

            _mockRepository.Verify(r => r.Clear(), Times.Never);
            _mockRepository.Verify(r => r.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()), Times.Never);
        }

        [Fact]
        public void UpdateReturnsErrorWhenRepositoryFails()
        {
            var newData = new ConcurrentDictionary<string, List<string>>
            {
                ["Location"] = new List<string> { "PlatformA" }
            };

            _mockRepository
                .Setup(r => r.Clear())
                .Throws(new Exception("Clear operation failed"));

            var result = _service.Update(newData);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Contains("Ошибка при получении платформ: Clear operation failed", result.Description);

            _mockRepository.Verify(r => r.Clear(), Times.Once);
            _mockRepository.Verify(r => r.Update(It.IsAny<ConcurrentDictionary<string, List<string>>>()), Times.Never);
        }
    }
}
