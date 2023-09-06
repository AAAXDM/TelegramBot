using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace WebApplication1
{
    [ApiController]
    [Route("api/message/update")]
    public class TelegramBotController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandler handleUpdateService,
         CancellationToken cancellationToken)
        {
            await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
            return Ok();
        }
    }
}
