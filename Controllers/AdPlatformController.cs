using Microsoft.AspNetCore.Mvc;
using AdPlatformService.Services;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text;

namespace AdPlatformService.Controllers
{
    [ApiController]
    [Route("api/ad-platforms")]
    public class AdPlatformController : ControllerBase
    {
        private readonly AdPlatformServiceHandler _adPlatformService;
        private readonly ILogger<AdPlatformController> _logger;

        public AdPlatformController(
            AdPlatformServiceHandler adPlatformService,
            ILogger<AdPlatformController> logger)
        {
            _adPlatformService = adPlatformService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            _logger.LogInformation("Начало загрузки файла");

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Попытка загрузить пустой файл");
                return BadRequest("Файл отсутствует или пуст.");
            }

            try
            {
                // Читаем содержимое в строку
                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                string fileContent = await reader.ReadToEndAsync();

                // Создаем новый поток из строки
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                await _adPlatformService.UploadFileAsync(stream);

                _logger.LogInformation("Файл успешно загружен");
                return Ok("Файл загружен и обработан.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке файла");
                return StatusCode(500, "Произошла ошибка при обработке файла");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string location)
        {
            const string logFileName = "search_results.log";

            _logger.LogInformation("Поиск по локации: {Location}", location);

            if (string.IsNullOrWhiteSpace(location))
            {
                _logger.LogWarning("Пустая локация в запросе");
                await LogToFile(logFileName, $"WARN | Пустая локация в запросе | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                return BadRequest("Локация не может быть пустой.");
            }

            try
            {
                var result = await _adPlatformService.SearchAsync(location);

                // Формируем строку для лога
                var logEntry = $"INFO | Поиск: {location} | Результат: {string.Join(", ", result)} | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                // Логируем в файл
                await LogToFile(logFileName, logEntry);

                // Логируем в систему
                _logger.LogInformation("Успешный поиск. {LogEntry}", logEntry);

                return Content(JsonSerializer.Serialize(result),
                    "application/json; charset=utf-8");
            }
            catch (Exception ex)
            {
                var errorMessage = $"ERROR | Поиск: {location} | Ошибка: {ex.Message} | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                // Логируем ошибку в файл
                await LogToFile(logFileName, errorMessage);

                // Логируем в систему
                _logger.LogError(ex, "Ошибка при поиске. {ErrorMessage}", errorMessage);

                return StatusCode(500, "Произошла ошибка при поиске");
            }
        }

        private async Task LogToFile(string filename, string message)
        {
            try
            {
                var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

                // Создаем директорию если не существует
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }

                var fullPath = Path.Combine(logsDirectory, filename);

                // Явное указание System.IO.File для избежания конфликта
                await System.IO.File.AppendAllTextAsync(fullPath, $"{message}{Environment.NewLine}", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось записать в файл лога");
            }
        }
    }
}