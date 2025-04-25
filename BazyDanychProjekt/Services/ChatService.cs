using BazyDanychProjekt.Models;
using MongoDB.Driver;

namespace BazyDanychProjekt.Services;

public class ChatService
{
    private readonly IMongoCollection<Chat> _chats;

    public ChatService(IMongoDatabase database)
    {
        _chats = database.GetCollection<Chat>("Chats");
    }

    public async Task<List<Chat>> GetAllChatsAsync()
    {
        return await _chats.Find(_ => true).ToListAsync();
    }

    public async Task<Chat> GetChatByIdAsync(string chatId)
    {
        var chat = await _chats.Find(c => c.Id == chatId).FirstOrDefaultAsync();
        if (chat == null)
        {
            throw new InvalidOperationException($"Chat with ID '{chatId}' not found.");
        }
        return chat;
    }

    public async Task AddChatAsync(Chat chat)
    {
        if (chat == null || chat.Participants == null || !chat.Participants.Any())
            throw new ArgumentException("Chat must have at least one participant.");

        var existing = await _chats.Find(c => c.Id == chat.Id).FirstOrDefaultAsync();
        if (existing != null)
            throw new InvalidOperationException($"Chat with ID '{chat.Id}' already exists.");

        await _chats.InsertOneAsync(chat);
    }

    public async Task UpdateChatAsync(string chatId, List<string> newParticipants)
    {
        var existingChat = await _chats.Find(c => c.Id == chatId).FirstOrDefaultAsync();
        if (existingChat == null)
        {
            throw new InvalidOperationException($"Chat with ID '{chatId}' not found.");
        }

        var update = Builders<Chat>.Update
            .Set(c => c.Participants, newParticipants);

        await _chats.UpdateOneAsync(c => c.Id == chatId, update);
    }

    public async Task DeleteChatAsync(string chatId)
    {
        var result = await _chats.DeleteOneAsync(c => c.Id == chatId);
        if (result.DeletedCount == 0)
        {
            throw new InvalidOperationException($"Chat with ID '{chatId}' not found.");
        }
    }
}
