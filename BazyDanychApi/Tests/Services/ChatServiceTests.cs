using BazyDanychProjekt.Models;
using BazyDanychProjekt.Services;
using FluentAssertions;
using MongoDB.Driver;
using NUnit.Framework;


namespace BazyDanychProjekt.Tests.Services
{
    [TestFixture]
    public class ChatServiceTests
    {
        private IMongoCollection<Chat> _chatsCollection;
        private ChatService _chatService;
        private MongoClient _client;
        private IMongoDatabase _database;

        [SetUp]
        public void SetUp()
        {
            _client = new MongoClient("mongodb://admin:admin123@localhost:27017/?authSource=admin");
            _database = _client.GetDatabase("TestChatDatabase");
            _chatsCollection = _database.GetCollection<Chat>("Chats");

            _chatService = new ChatService(_database);

            _chatsCollection.DeleteMany(Builders<Chat>.Filter.Empty);
        }

        [Test]
        public async Task AddChatAsync_ShouldAddChat()
        {
            // Arrange
            var participants = new List<string> { "userMateusz", "userKacper" };
            var chat = new Chat
            {
                Id = Chat.GenerateChatId(participants),
                Participants = participants,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _chatService.AddChatAsync(chat);

            // Assert
            var result = await _chatsCollection.Find(c => c.Id == chat.Id).FirstOrDefaultAsync();
            result.Should().NotBeNull();
            result.Participants.Should().Equal(participants);
        }

        [Test]
        public async Task GetChatByIdAsync_ShouldReturnChat()
        {
            // Arrange
            var participants = new List<string> { "userMateusz", "userKacper" };
            var chat = new Chat
            {
                Id = Chat.GenerateChatId(participants),
                Participants = participants,
                CreatedAt = DateTime.UtcNow
            };
            await _chatService.AddChatAsync(chat);

            // Act
            var result = await _chatService.GetChatByIdAsync(chat.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(chat.Id);
            result.Participants.Should().Equal(participants);
        }

        [Test]
        public async Task GetChatByIdAsync_ChatNotFound_ShouldThrowException()
        {
            // Act
            Func<Task> act = async () => await _chatService.GetChatByIdAsync("non_existing_chat_id");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Chat with ID 'non_existing_chat_id' not found.");
        }

        [Test]
        public async Task GetAllChatsAsync_ShouldReturnAllChats()
        {
            // Arrange
            var chat1 = new Chat
            {
                Id = Chat.GenerateChatId(new List<string> { "userMateusz", "userKacper" }),
                Participants = new List<string> { "userMateusz", "userKacper" },
                CreatedAt = DateTime.UtcNow
            };
            var chat2 = new Chat
            {
                Id = Chat.GenerateChatId(new List<string> { "userKacper", "userAnia" }),
                Participants = new List<string> { "userKacper", "userAnia" },
                CreatedAt = DateTime.UtcNow
            };
            await _chatService.AddChatAsync(chat1);
            await _chatService.AddChatAsync(chat2);

            // Act
            var result = await _chatService.GetAllChatsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Id == chat1.Id);
            result.Should().Contain(c => c.Id == chat2.Id);
        }

        [Test]
        public async Task UpdateChatAsync_ShouldUpdateChatParticipants()
        {
            // Arrange
            var participants = new List<string> { "userMateusz", "userKacper" };
            var chat = new Chat
            {
                Id = Chat.GenerateChatId(participants),
                Participants = participants,
                CreatedAt = DateTime.UtcNow
            };
            await _chatService.AddChatAsync(chat);

            var updatedParticipants = new List<string> { "userMateusz", "userKacper", "userAnia" };

            // Act
            await _chatService.UpdateChatAsync(chat.Id, updatedParticipants);

            // Assert
            var result = await _chatsCollection.Find(c => c.Id == chat.Id).FirstOrDefaultAsync();
            result.Should().NotBeNull();
            result.Participants.Should().Equal(updatedParticipants);
        }

        [Test]
        public async Task UpdateChatAsync_ChatNotFound_ShouldThrowException()
        {
            // Act
            Func<Task> act = async () => await _chatService.UpdateChatAsync("non_existing_chat_id", new List<string> { "userMateusz", "userKacper" });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Chat with ID 'non_existing_chat_id' not found.");
        }

        [Test]
        public async Task DeleteChatAsync_ShouldDeleteChat()
        {
            // Arrange
            var participants = new List<string> { "userMateusz", "userKacper" };
            var chat = new Chat
            {
                Id = Chat.GenerateChatId(participants),
                Participants = participants,
                CreatedAt = DateTime.UtcNow
            };
            await _chatService.AddChatAsync(chat);

            // Act
            await _chatService.DeleteChatAsync(chat.Id);

            // Assert
            var result = await _chatsCollection.Find(c => c.Id == chat.Id).FirstOrDefaultAsync();
            result.Should().BeNull();
        }

        [Test]
        public async Task DeleteChatAsync_ChatNotFound_ShouldThrowException()
        {
            // Act
            Func<Task> act = async () => await _chatService.DeleteChatAsync("non_existing_chat_id");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Chat with ID 'non_existing_chat_id' not found.");
        }
    }
}
