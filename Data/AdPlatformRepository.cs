using System.Collections.Concurrent;
using AdPlatformService.Models;

namespace AdPlatformService.Data
{
    public class AdPlatformRepository
    {
        private readonly ConcurrentDictionary<string, List<string>> _adPlatforms = new();

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден", filePath);

            using var reader = new StreamReader(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(':');
                if (parts.Length != 2) continue;

                var platform = parts[0].Trim();
                var locations = parts[1].Split(',').Select(l => l.Trim()).ToList();

                foreach (var location in locations)
                {
                    _adPlatforms.AddOrUpdate(location, new List<string> { platform },
                        (key, list) => { list.Add(platform); return list; });
                }
            }
        }

        public List<string> GetPlatformsForLocation(string location)
        {
            return _adPlatforms
                .Where(kvp => location.StartsWith(kvp.Key))
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .ToList();
        }
    }
}
