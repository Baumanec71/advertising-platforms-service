using advertising_platforms_service.Domain.Responses;
using advertising_platforms_service.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace advertising_platforms_service.Service.Implementations
{
    public class WorkToFile : IWorkToTxtFile
    {
        private readonly ILogger _logger;
        public WorkToFile(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<IBaseResponse<ConcurrentDictionary<string, List<string>>>> ReadFile(string path)
        {
            try
            {
                var locationsWithPlatforms = new ConcurrentDictionary<string, List<string>>();

                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    throw new FileNotFoundException($"Файл не найден: {path}");
                }

                _logger.LogInformation($"Начало чтения файла: {path}");

                using (StreamReader reader = new StreamReader(path))
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
                                //if (!locationsWithPlatforms.ContainsKey(location))
                                //    locationsWithPlatforms[location] = new List<string>();
                                //if (!locationsWithPlatforms[location].Contains(platformName))
                                //    locationsWithPlatforms[location].Add(platformName);
                                locationsWithPlatforms[location].Add(platformName);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Чтение файла: {path} закончено");

                return new BaseResponse<ConcurrentDictionary<string, List<string>>>()
                {
                    StatusCode = HttpStatusCode.OK,
                    Description = $"Чтение файла: {path} закончено",
                    Value = locationsWithPlatforms
                };
            }
            catch(Exception ex) 
            {
                _logger.LogError($"Ошибка при чтении файла {path}: {ex.Message}");

                return new BaseResponse<ConcurrentDictionary<string, List<string>>>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Description = $"Ошибка при чтении файла {path}: {ex.Message}",
                };
            }
        }
    }
}
