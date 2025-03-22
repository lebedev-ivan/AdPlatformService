using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Text;
using AdPlatformService.Models;
using System.IO.Pipes;

namespace AdPlatformService.Services
{
    public class AdPlatformServiceHandler
    {
        private readonly ConcurrentDictionary<string, List<string>> _adPlatforms = new();

        // Загружаем рекламные площадки из файла
        public void UploadFile(Stream fileStream)
        {
            Console.OutputEncoding = Encoding.UTF8;

            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            string? line;
            _adPlatforms.Clear();

            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Чтение строки: {line}");

                var parts = line.Split(':');
                if (parts.Length != 2) continue;

                var platform = parts[0].Trim();
                var locations = parts[1].Split(',').Select(l => l.Trim()).ToList();

                Console.WriteLine($"Platform: {platform}, Locations: {string.Join(", ", locations)}");

                foreach (var location in locations)
                {
                    _adPlatforms.AddOrUpdate(location, new List<string> { platform },
                        (key, list) => { list.Add(platform); return list; });
                }
            }

            Console.WriteLine("Содержимое _adPlatforms:");

            foreach (var kvp in _adPlatforms)
            {
                Console.WriteLine($"Location: {kvp.Key}, Platforms: {string.Join(", ", kvp.Value)}");
            }
        }

        // Поиск рекламных площадок по локации
        public List<string> Search(string location)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Если location не пустой, то точное совпадение
            if (string.IsNullOrWhiteSpace(location))
                return new List<string>();

            var result = _adPlatforms
                .Where(kvp => kvp.Key.Contains(location, System.StringComparison.OrdinalIgnoreCase))
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .ToList();

            Console.WriteLine($"Поиск по локации '{location}': {string.Join(", ", result)}");

            return result;
        }
    }
}