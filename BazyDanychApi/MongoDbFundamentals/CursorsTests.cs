using BazyDanychProjekt.Models;
using BazyDanychProjekt.Services;
using FluentAssertions;
using MongoDB.Driver;
using NUnit.Framework;

namespace BazyDanychProjekt.MongoDbFundamentals;

[TestFixture]
public class CursorsTests
{
        private IMongoCollection<User> _usersCollection;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            var client = new MongoClient("mongodb://admin:admin123@localhost:27017/?authSource=admin");
            var database = client.GetDatabase("TestDatabase");
            _usersCollection = database.GetCollection<User>("Users");

            _userService = new UserService(database);

            _usersCollection.DeleteMany(Builders<User>.Filter.Empty);
        }

    [Test]
    public async Task FindUsersByFirstName_WithCursorMoveNext_ShouldReturnUsersNamedPawel()
    {
        var users = new[]
        {
        new User { Login = "pawel_1", Password = "pass1", Email = "pawel1@example.com", FirstName = "Pawel", LastName = "Nowak" },
        new User { Login = "pawel_2", Password = "pass2", Email = "pawel2@example.com", FirstName = "Pawel", LastName = "Kowalski" },
        new User { Login = "john_doe", Password = "pass", Email = "john@example.com", FirstName = "John", LastName = "Doe" }
        };

        await _usersCollection.InsertManyAsync(users);

        var filter = Builders<User>.Filter.Eq(u => u.FirstName, "Pawel");
        using var cursor = await _usersCollection.FindAsync(filter);

        var pawels = new List<User>();

        while (await cursor.MoveNextAsync())
        {
            foreach (var doc in cursor.Current)
            {
                pawels.Add(doc);
            }
        }

        pawels.Should().NotBeEmpty();
        pawels.Should().OnlyContain(u => u.FirstName == "Pawel");
        pawels.Should().HaveCount(2);
    }

    [Test]
    public async Task FindUsersSortedPaged_ShouldReturnCorrectPageOfSortedUsers()
    {
        var users = new[]
        {
        new User { Login = "adam", FirstName = "Adam", LastName = "Zalewski", Email = "adam@example.com", Password = "pass" },
        new User { Login = "bartek", FirstName = "Bartek", LastName = "Nowak", Email = "bartek@example.com", Password = "pass" },
        new User { Login = "celina", FirstName = "Celina", LastName = "Kowalska", Email = "celina@example.com", Password = "pass" },
        new User { Login = "daria", FirstName = "Daria", LastName = "Adamska", Email = "daria@example.com", Password = "pass" },
        new User { Login = "edward", FirstName = "Edward", LastName = "Białek", Email = "edward@example.com", Password = "pass" }
        };

        await _usersCollection.InsertManyAsync(users);

        var filter = Builders<User>.Filter.Empty;
        var sort = Builders<User>.Sort.Ascending(u => u.FirstName);

        var pagedUsers = await _usersCollection
            .Find(filter)
            .Sort(sort)
            .Skip(2)
            .Limit(2)
            .ToListAsync();

        pagedUsers.Should().HaveCount(2);
        pagedUsers[0].FirstName.Should().Be("Celina");
        pagedUsers[1].FirstName.Should().Be("Daria");
    }
}
