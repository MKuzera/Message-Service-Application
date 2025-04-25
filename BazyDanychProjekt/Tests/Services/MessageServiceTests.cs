using BazyDanychProjekt.Models;
using BazyDanychProjekt.Services;
using MongoDB.Driver;
using NUnit.Framework;
using FluentAssertions;

namespace BazyDanychProjekt.Tests.Services
{
    public class MessageServiceTests
    {
        private IMongoDatabase _database;
        private MessageService _messageService;
        private IMongoCollection<Message> _messageCollection;

        [SetUp]
        public void Setup()
        {
            var client = new MongoClient("mongodb://admin:admin123@localhost:27017/?authSource=admin");
            _database = client.GetDatabase("TestDatabase");

            _messageCollection = _database.GetCollection<Message>("Messages");
            _messageService = new MessageService(_database);

            _messageCollection.DeleteMany(_ => true);
        }

        [Test]
        public async Task AddMessageAsync_ShouldAddMessageToDatabase()
        {
            var message = new Message
            {
                ChatId = "chat_1",
                SenderLogin = "user1",
                Text = "Hello!"
            };

            await _messageService.AddMessageAsync(message);

            var addedMessage = await _messageCollection.Find(m => m.ChatId == message.ChatId).FirstOrDefaultAsync();
            addedMessage.Should().NotBeNull();
            addedMessage.SenderLogin.Should().Be(message.SenderLogin);
            addedMessage.Text.Should().Be(message.Text);
        }

        [Test]
        public async Task GetMessageByIdAsync_ShouldReturnMessage_WhenExists()
        {
            var message = new Message
            {
                ChatId = "chat_2",
                SenderLogin = "user2",
                Text = "Hi there!"
            };

            await _messageService.AddMessageAsync(message);

            var retrievedMessage = await _messageService.GetMessageByIdAsync(message.Id);
            retrievedMessage.Should().NotBeNull();
            retrievedMessage.SenderLogin.Should().Be(message.SenderLogin);
            retrievedMessage.Text.Should().Be(message.Text);
        }

        [Test]
        public async Task GetMessageByIdAsync_ShouldThrowInvalidOperationException_WhenNotFound()
        {
            var validId = "507f191e810c19729de860ea";

            Func<Task> action = async () => await _messageService.GetMessageByIdAsync(validId);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Message with ID '{validId}' not found.");
        }

        [Test]
        public async Task GetMessagesByChatIdAsync_ShouldReturnMessages_WhenMessagesExist()
        {
            var message1 = new Message
            {
                ChatId = "chat_3",
                SenderLogin = "user3",
                Text = "Message 1"
            };
            var message2 = new Message
            {
                ChatId = "chat_3",
                SenderLogin = "user4",
                Text = "Message 2"
            };

            await _messageService.AddMessageAsync(message1);
            await _messageService.AddMessageAsync(message2);

            var messages = await _messageService.GetMessagesByChatIdAsync("chat_3");
            messages.Should().HaveCount(2);
            messages.Should().Contain(m => m.SenderLogin == "user3" && m.Text == "Message 1");
            messages.Should().Contain(m => m.SenderLogin == "user4" && m.Text == "Message 2");
        }

        [Test]
        public async Task DeleteMessageAsync_ShouldRemoveMessage_WhenExists()
        {
            var message = new Message
            {
                ChatId = "chat_4",
                SenderLogin = "user5",
                Text = "Delete me"
            };

            await _messageService.AddMessageAsync(message);
            var addedMessage = await _messageCollection.Find(m => m.Id == message.Id).FirstOrDefaultAsync();
            addedMessage.Should().NotBeNull();

            await _messageService.DeleteMessageAsync(message.Id);

            var deletedMessage = await _messageCollection.Find(m => m.Id == message.Id).FirstOrDefaultAsync();
            deletedMessage.Should().BeNull();
        }

        [Test]
        public async Task DeleteMessageAsync_ShouldThrowInvalidOperationException_WhenNotFound()
        {
            var validId = "507f191e810c19729de860ea";

            Func<Task> action = async () => await _messageService.DeleteMessageAsync(validId);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Message with ID '{validId}' not found.");
        }

        [Test]
        public async Task UpdateMessageAsync_ShouldUpdateMessageText_WhenExists()
        {
            var message = new Message
            {
                ChatId = "chat_5",
                SenderLogin = "user6",
                Text = "Old text"
            };

            await _messageService.AddMessageAsync(message);

            var updatedText = "Updated text";
            await _messageService.UpdateMessageAsync(message.Id, updatedText);

            var updatedMessage = await _messageCollection.Find(m => m.Id == message.Id).FirstOrDefaultAsync();
            updatedMessage.Should().NotBeNull();
            updatedMessage.Text.Should().Be(updatedText);
        }

        [Test]
        public async Task UpdateMessageAsync_ShouldThrowInvalidOperationException_WhenNotFound()
        {
            var validId = "507f191e810c19729de860ea";

            Func<Task> action = async () => await _messageService.UpdateMessageAsync(validId, "New text");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Message with ID '{validId}' not found.");
        }
    }
}
