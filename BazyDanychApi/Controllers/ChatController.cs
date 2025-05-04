using BazyDanychProjekt.Models;
using BazyDanychProjekt.Services;
using Microsoft.AspNetCore.Mvc;

namespace BazyDanychApi.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Chat>>> GetAllChats()
        {
            var chats = await _chatService.GetAllChatsAsync();
            return Ok(chats);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> GetChatById(string id)
        {
            try
            {
                var chat = await _chatService.GetChatByIdAsync(id);
                return Ok(chat);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddChat([FromBody] Chat chat)
        {
            Chat chat2 = new Chat
            {
                CreatedAt = chat.CreatedAt,
                Participants = chat.Participants,
                Id = Chat.GenerateChatId(chat.Participants),
            };

            try
            {
                await _chatService.AddChatAsync(chat);
                return CreatedAtAction(nameof(GetChatById), new { id = chat.Id }, chat);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateChat(string id, [FromBody] List<string> newParticipants)
        {
            try
            {
                await _chatService.UpdateChatAsync(id, newParticipants);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteChat(string id)
        {
            try
            {
                await _chatService.DeleteChatAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
