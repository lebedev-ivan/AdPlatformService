using Microsoft.AspNetCore.Mvc;
using AdPlatformService.Services;
using System.Text.Json;

namespace AdPlatformService.Controllers
{
    [ApiController]
    [Route("api/ad-platforms")]
    public class AdPlatformController : ControllerBase
    {
        private readonly AdPlatformServiceHandler _adPlatformService;

        public AdPlatformController(AdPlatformServiceHandler adPlatformService)
        {
            _adPlatformService = adPlatformService;
        }

        // ��������� ��������� �������� �� �����
        [HttpPost("upload")]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("���� ����������� ��� ����.");
            }

            using var stream = file.OpenReadStream();
            _adPlatformService.UploadFile(stream);

            return Ok("���� �������� � ���������.");
        }

        // ���� ��������� �������� �� �������
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest("������� �� ����� ���� ������.");
            }

            var result = _adPlatformService.Search(location);
            return Content(JsonSerializer.Serialize(result), "application/json; charset=utf-8");
        }
    }
}