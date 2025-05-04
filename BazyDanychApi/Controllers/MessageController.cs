using BazyDanychProjekt.Models;
using BazyDanychProjekt.Services;
using Microsoft.AspNetCore.Mvc;

namespace BazyDanychApi.Controllers
{
    [ApiController]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<ActionResult> AddMessage([FromBody] Message message)
        {
            Message message1 = new Message
            {
                ChatId = message.ChatId,
                SenderLogin = message.SenderLogin,
                Text = message.Text,
                Timestamp = message.Timestamp,
            };

            try
            {
                await _messageService.AddMessageAsync(message1);
                return CreatedAtAction(nameof(GetMessageById), new { id = message.Id }, message1);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessageById(string id)
        {
            try
            {
                var message = await _messageService.GetMessageByIdAsync(id);
                return Ok(message);
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("chat/{chatId}")]
        public async Task<ActionResult<List<Message>>> GetMessagesByChatId(string chatId)
        {
            var messages = await _messageService.GetMessagesByChatIdAsync(chatId);
            return Ok(messages);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMessage(string id, [FromBody] string newText)
        {


            try
            {
                await _messageService.UpdateMessageAsync(id, newText);
                return NoContent();
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(string id)
        {
            try
            {
                await _messageService.DeleteMessageAsync(id);
                return NoContent();
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
