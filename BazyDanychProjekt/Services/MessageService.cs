using BazyDanychProjekt.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BazyDanychProjekt.Services
{
    public class MessageService
    {
        private readonly IMongoCollection<Message> _messages;

        public MessageService(IMongoDatabase database)
        {
            _messages = database.GetCollection<Message>("Messages");
        }

        public async Task AddMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrEmpty(message.ChatId))
                throw new ArgumentException("ChatId cannot be null or empty.");

            if (string.IsNullOrEmpty(message.SenderLogin))
                throw new ArgumentException("SenderLogin cannot be null or empty.");

            if (string.IsNullOrEmpty(message.Text))
                throw new ArgumentException("Message text cannot be null or empty.");

            await _messages.InsertOneAsync(message);
        }

        public async Task<Message> GetMessageByIdAsync(string messageId)
        {
            if (!ObjectId.TryParse(messageId, out _))
            {
                throw new FormatException($"'{messageId}' is not a valid 24 digit hex string.");
            }

            var message = await _messages.Find(m => m.Id == messageId).FirstOrDefaultAsync();

            if (message == null)
            {
                throw new InvalidOperationException($"Message with ID '{messageId}' not found.");
            }

            return message;
        }

        public async Task<List<Message>> GetMessagesByChatIdAsync(string chatId)
        {
            var messages = await _messages.Find(m => m.ChatId == chatId).ToListAsync();
            return messages;
        }

        public async Task DeleteMessageAsync(string messageId)
        {
            if (!ObjectId.TryParse(messageId, out _))
            {
                throw new FormatException($"'{messageId}' is not a valid 24 digit hex string.");
            }

            var result = await _messages.DeleteOneAsync(m => m.Id == messageId);
            if (result.DeletedCount == 0)
            {
                throw new InvalidOperationException($"Message with ID '{messageId}' not found.");
            }
        }

        public async Task UpdateMessageAsync(string messageId, string newText)
        {
            if (!ObjectId.TryParse(messageId, out _))
            {
                throw new FormatException($"'{messageId}' is not a valid 24 digit hex string.");
            }

            var existingMessage = await _messages.Find(m => m.Id == messageId).FirstOrDefaultAsync();
            if (existingMessage == null)
            {
                throw new InvalidOperationException($"Message with ID '{messageId}' not found.");
            }

            var update = Builders<Message>.Update.Set(m => m.Text, newText);

            await _messages.UpdateOneAsync(m => m.Id == messageId, update);
        }
    }
}
