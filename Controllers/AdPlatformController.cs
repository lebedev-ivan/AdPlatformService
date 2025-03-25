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
            _logger.LogInformation("������ �������� �����");

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("������� ��������� ������ ����");
                return BadRequest("���� ����������� ��� ����.");
            }

            try
            {
                // ������ ���������� � ������
                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                string fileContent = await reader.ReadToEndAsync();

                // ������� ����� ����� �� ������
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                await _adPlatformService.UploadFileAsync(stream);

                _logger.LogInformation("���� ������� ��������");
                return Ok("���� �������� � ���������.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ��� �������� �����");
                return StatusCode(500, "��������� ������ ��� ��������� �����");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string location)
        {
            const string logFileName = "search_results.log";

            _logger.LogInformation("����� �� �������: {Location}", location);

            if (string.IsNullOrWhiteSpace(location))
            {
                _logger.LogWarning("������ ������� � �������");
                await LogToFile(logFileName, $"WARN | ������ ������� � ������� | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                return BadRequest("������� �� ����� ���� ������.");
            }

            try
            {
                var result = await _adPlatformService.SearchAsync(location);

                // ��������� ������ ��� ����
                var logEntry = $"INFO | �����: {location} | ���������: {string.Join(", ", result)} | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                // �������� � ����
                await LogToFile(logFileName, logEntry);

                // �������� � �������
                _logger.LogInformation("�������� �����. {LogEntry}", logEntry);

                return Content(JsonSerializer.Serialize(result),
                    "application/json; charset=utf-8");
            }
            catch (Exception ex)
            {
                var errorMessage = $"ERROR | �����: {location} | ������: {ex.Message} | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                // �������� ������ � ����
                await LogToFile(logFileName, errorMessage);

                // �������� � �������
                _logger.LogError(ex, "������ ��� ������. {ErrorMessage}", errorMessage);

                return StatusCode(500, "��������� ������ ��� ������");
            }
        }

        private async Task LogToFile(string filename, string message)
        {
            try
            {
                var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

                // ������� ���������� ���� �� ����������
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }

                var fullPath = Path.Combine(logsDirectory, filename);

                // ����� �������� System.IO.File ��� ��������� ���������
                await System.IO.File.AppendAllTextAsync(fullPath, $"{message}{Environment.NewLine}", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�� ������� �������� � ���� ����");
            }
        }
    }
}