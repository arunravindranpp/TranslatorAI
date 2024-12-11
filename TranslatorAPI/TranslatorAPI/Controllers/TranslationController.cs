using Microsoft.AspNetCore.Mvc;
using Serilog;
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
        [HttpGet("check")]
        public IActionResult GetCheckMethod()
        {
            var dummyData = new
            {
                Id = 1,
                OriginalText = "Hello, world!",
                TranslatedText = "Bonjour le monde!",
                Language = "French"
            };

            return Ok(dummyData);
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
        [HttpGet]
        [Route("GetMessages")]
        public async Task<IActionResult> GetMessages()
        {
            Log.Information("GetMessages started");
            var messages = await _translatorService.GetMessages();
            Log.Information("GetMessages ended");
            return Ok(messages);
        }
        [HttpGet]
        [Route("GetMessagesByReceiver")]
        public async Task<IActionResult> GetMessagesByReceiver(string user,string receiver)
        {
            Log.Information("GetMessages started");
            var messages = await _translatorService.GetMessagesByReceiver(user,receiver);
            Log.Information("GetMessages ended");
            return Ok(messages);
        }
    }

    public class TranslationRequest
    {
        public string Text { get; set; }
    }
}
