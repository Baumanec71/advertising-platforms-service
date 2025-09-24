using advertising_platforms_service.Domain.Responses;
using advertising_platforms_service.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace advertising_platforms_service.Service.Implementations
{
    public class WorkToFile : IWorkToTxtFile
    {
        private readonly ILogger<WorkToFile> _logger;
        private readonly string _filePath;
        public WorkToFile(ILogger<WorkToFile> logger, IConfiguration configuration)
        {
            _logger = logger;
            _filePath = configuration["FileStorage:Path"]!;

            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = "C:/Users/user/Desktop/advertising-platforms-service/platforms.txt";
                _logger.LogWarning("Путь к файлу не настроен. Используется путь по умолчанию: {FilePath}", _filePath);
            }

            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        public async Task<IBaseResponse<ConcurrentDictionary<string, List<string>>>> ReadFile()
        {
            try
            {
                var locationsWithPlatforms = new ConcurrentDictionary<string, List<string>>();

                if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                {
                    throw new FileNotFoundException($"Файл не найден: {_filePath}");
                }

                _logger.LogInformation("Чтение файла: {FilePath}", _filePath);

                using (StreamReader reader = new StreamReader(_filePath))
                {
                    string? line;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line
                            .Split(':', 2);

                        if (parts.Length != 2) continue;

                        var platformName = parts[0].Trim();
                        var locations = parts[1].Split(',')
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

                        if (!string.IsNullOrEmpty(platformName) && locations.Any())
                        {
                            foreach (var location in locations)
                            {
                                if (!locationsWithPlatforms.ContainsKey(location))
                                    locationsWithPlatforms[location] = new List<string>();

                                if (!locationsWithPlatforms[location].Contains(platformName))
                                    locationsWithPlatforms[location].Add(platformName);
                            }
                        }
                    }
                }

                _logger.LogInformation("Файл прочитан. Локаций: {Count}", locationsWithPlatforms.Count);

                return new BaseResponse<ConcurrentDictionary<string, List<string>>>()
                {
                    StatusCode = HttpStatusCode.OK,
                    Description = $"Чтение файла: {_filePath} закончено",
                    Value = locationsWithPlatforms
                };
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Ошибка чтения файла {FilePath}", _filePath);

                return new BaseResponse<ConcurrentDictionary<string, List<string>>>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Description = $"Ошибка при чтении файла {_filePath}: {ex.Message}",
                };
            }
        }
    }
}
