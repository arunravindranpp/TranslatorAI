using Microsoft.AspNetCore.Mvc;
using TranslatorAPI.Services;

namespace TranslatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly TranslatorService _translatorService;

        public TranslationController(TranslatorService translatorService)
        {
            _translatorService = translatorService;
        }

        [HttpPost]
        [Route("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Text to translate cannot be empty.");

            var translatedText = await _translatorService.TranslateTextAsync(request.Text);
            return Ok(new { TranslatedText = translatedText });
        }
    }

    public class TranslationRequest
    {
        public string Text { get; set; }
    }
}
