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

        // Загружаем рекламные площадки из файла
        [HttpPost("upload")]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл отсутствует или пуст.");
            }

            using var stream = file.OpenReadStream();
            _adPlatformService.UploadFile(stream);

            return Ok("Файл загружен и обработан.");
        }

        // Ищем рекламные площадки по локации
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest("Локация не может быть пустой.");
            }

            var result = _adPlatformService.Search(location);
            return Content(JsonSerializer.Serialize(result), "application/json; charset=utf-8");
        }
    }
}