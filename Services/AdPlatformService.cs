using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Text;
using AdPlatformService.Models;

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
                try
                {
                    Console.WriteLine($"Чтение строки: {line}");

                    var parts = line.Split(':');
                    if (parts.Length != 2) continue;

                    var platform = parts[0].Trim();
                    var locations = parts[1].Split(',')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l))
                        .ToList();

                    if (locations.Count == 0) continue;

                    foreach (var location in locations)
                    {
                        _adPlatforms.AddOrUpdate(location, new List<string> { platform },
                            (key, list) => { list.Add(platform); return list; });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке строки: {line}. Ошибка: {ex.Message}");
                }
            }

            Console.WriteLine("Содержимое _adPlatforms:");
            foreach (var kvp in _adPlatforms)
            {
                Console.WriteLine($"Location: {kvp.Key}, Platforms: {string.Join(", ", kvp.Value)}");
            }
        }

        // Выполняем поиск рекламных площадок по локации
        public List<string> Search(string location)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (string.IsNullOrWhiteSpace(location))
                return new List<string>();

            // Ищем площадки, которые точно соответствуют локации
            var exactMatch = _adPlatforms
                .Where(kvp => kvp.Key == location)
                .SelectMany(kvp => kvp.Value)
                .ToList();

            // Ищем площадки для вложенных локаций
            var nestedMatches = _adPlatforms
                .Where(kvp => location.StartsWith(kvp.Key + "/"))
                .SelectMany(kvp => kvp.Value)
                .ToList();

            var result = exactMatch.Concat(nestedMatches).Distinct().ToList();

            Console.WriteLine($"Поиск по локации '{location}': {string.Join(", ", result)}");

            return result;
        }
    }
}