using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AdPlatformService.Data
{
    public class AdPlatformRepository
    {
        private ConcurrentDictionary<string, List<string>> _adPlatforms = new();
        private readonly ConcurrentDictionary<string, List<string>> _buffer = new();
        private readonly ILogger<AdPlatformRepository> _logger;

        public AdPlatformRepository(ILogger<AdPlatformRepository> logger)
        {
            _logger = logger;
        }

        public async Task LoadFromFileAsync(string filePath)
        {
            _logger.LogInformation("Загрузка данных из файла: {FilePath}", filePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден", filePath);

            using var reader = new StreamReader(filePath);
            _buffer.Clear();

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                try
                {
                    var parts = line.Split(':');
                    if (parts.Length != 2) continue;

                    var platform = parts[0].Trim();
                    var locations = parts[1].Split(',').Select(l => l.Trim()).ToList();

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
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки строки: {Line}", line);
                }
            }

            Interlocked.Exchange(ref _adPlatforms, _buffer);
            _buffer.Clear();

            _logger.LogInformation("Данные успешно загружены. Локаций: {Count}",
                _adPlatforms.Count);
        }

        public Task<List<string>> GetPlatformsForLocationAsync(string location)
        {
            _logger.LogDebug("Поиск по локации: {Location}", location);

            if (string.IsNullOrWhiteSpace(location))
                return Task.FromResult(new List<string>());

            var result = new List<string>();

            if (_adPlatforms.TryGetValue(location, out var platforms))
            {
                result.AddRange(platforms);
            }

            var parts = location.Split('/');
            for (int i = parts.Length - 1; i > 0; i--)
            {
                var parentLocation = string.Join('/', parts.Take(i));
                if (_adPlatforms.TryGetValue(parentLocation, out var parentPlatforms))
                {
                    result.AddRange(parentPlatforms);
                }
            }

            return Task.FromResult(result.Distinct().ToList());
        }
    }
}