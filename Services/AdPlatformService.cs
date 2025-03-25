using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AdPlatformService.Services
{
    public class AdPlatformServiceHandler
    {
        private ConcurrentDictionary<string, List<string>> _adPlatforms = new();
        private readonly ConcurrentDictionary<string, List<string>> _buffer = new();
        private readonly ILogger<AdPlatformServiceHandler> _logger;

        public AdPlatformServiceHandler(ILogger<AdPlatformServiceHandler> logger)
        {
            _logger = logger;
        }

        public async Task UploadFileAsync(Stream fileStream)
        {
            _logger.LogInformation("Начало обработки файла");

            try
            {
                fileStream.Position = 0;

                using var reader = new StreamReader(fileStream, Encoding.UTF8, leaveOpen: true);
                _buffer.Clear();

                int lineNumber = 0;
                int processedLocations = 0;
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    try
                    {
                        var parts = line.Split(':');
                        if (parts.Length != 2)
                        {
                            _logger.LogWarning("Строка {LineNumber} имеет неверный формат", lineNumber);
                            continue;
                        }

                        var platform = parts[0].Trim();
                        var locations = parts[1].Split(',')
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

                        foreach (var location in locations)
                        {
                            _buffer.AddOrUpdate(location,
                                new List<string> { platform },
                                (key, list) =>
                                {
                                    if (!list.Contains(platform))
                                        list.Add(platform);
                                    return list;
                                });
                            processedLocations++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка в строке {LineNumber}: {Line}", lineNumber, line);
                    }
                }

                Interlocked.Exchange(ref _adPlatforms, _buffer);

                _logger.LogInformation("Обработано строк: {Lines}, локаций: {Locations}",
                    lineNumber, processedLocations);

                if (processedLocations == 0)
                {
                    _logger.LogError("Файл не содержит валидных данных. Пример ожидаемого формата: 'Платформа1:/loc1,/loc2'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка обработки файла");
                throw;
            }
        }

        public Task<List<string>> SearchAsync(string location)
        {
            _logger.LogDebug("Поиск площадок для локации: {Location}", location);

            if (string.IsNullOrWhiteSpace(location))
                return Task.FromResult(new List<string>());

            var result = new List<string>();

            // Поиск точного совпадения (O(1))
            if (_adPlatforms.TryGetValue(location, out var platforms))
            {
                result.AddRange(platforms);
            }

            // Поиск вложенных локаций
            var parts = location.Split('/');
            for (int i = parts.Length - 1; i > 0; i--)
            {
                var parentLocation = string.Join('/', parts.Take(i));
                if (_adPlatforms.TryGetValue(parentLocation, out var parentPlatforms))
                {
                    result.AddRange(parentPlatforms);
                }
            }

            _logger.LogDebug("Найдено {Count} площадок для локации {Location}",
                result.Distinct().Count(), location);

            return Task.FromResult(result.Distinct().ToList());
        }
    }
}