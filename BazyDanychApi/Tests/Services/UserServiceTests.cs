using BazyDanychProjekt.Models;
using BazyDanychProjekt.Services;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace BazyDanychProjekt.Tests.Services;

[TestFixture]
public class UserServiceTests
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
    public async Task GetUserByIdAsync_UserExists_ShouldReturnUser()
    {
        var user = new User
        {
            Login = "john_doe",
            Password = "pass",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        await _userService.AddUserAsync(user);

        var result = await _userService.GetUserByIdAsync(user.Id.ToString());

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Login.Should().Be("john_doe");
    }

    [Test]
    public async Task GetUserByIdAsync_UserNotExists_ShouldThrowException()
    {
        var fakeId = ObjectId.GenerateNewId().ToString();

        Func<Task> act = async () => await _userService.GetUserByIdAsync(fakeId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with ID '{fakeId}' not found.");
    }

    [Test]
    public async Task AddUserAsync_UserNotExists_ShouldAddUser()
    {
        var user = new User
        {
            Login = "john_doe",
            Password = "hashedPassword123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        await _userService.AddUserAsync(user);

        var result = await _usersCollection.Find(u => u.Login == "john_doe").FirstOrDefaultAsync();
        result.Should().NotBeNull();
        result.Login.Should().Be("john_doe");
    }

    [Test]
    public async Task AddUserAsync_UserWithExistingLogin_ShouldThrowException()
    {
        var user1 = new User
        {
            Login = "john_doe",
            Password = "hashedPassword123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var user2 = new User
        {
            Login = "john_doe",
            Password = "anotherPassword",
            Email = "another.email@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        await _userService.AddUserAsync(user1);

        Func<Task> addUserAction = async () => await _userService.AddUserAsync(user2);
        await addUserAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with login 'john_doe' already exists.");
    }

    [Test]
    public async Task RemoveUserAsync_UserExists_ShouldRemoveUser()
    {
        var user = new User
        {
            Login = "john_doe",
            Password = "hashedPassword123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        await _userService.AddUserAsync(user);

        await _userService.RemoveUserAsync(user.Id);

        var result = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
        result.Should().BeNull();
    }

    [Test]
    public async Task RemoveUserAsync_UserNotExists_ShouldThrowException()
    {
        var fakeObjectId = ObjectId.GenerateNewId().ToString();

        Func<Task> removeUserAction = async () => await _userService.RemoveUserAsync(fakeObjectId);
        await removeUserAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with ID '{fakeObjectId}' not found.");
    }

    [Test]
    public async Task EditUserAsync_UserExists_ShouldEditUser()
    {
        var user = new User
        {
            Login = "john_doe",
            Password = "hashedPassword123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        await _userService.AddUserAsync(user);

        var updatedUser = new User
        {
            Id = user.Id,
            Login = "john_doe_updated",
            Password = "newHashedPassword",
            Email = "new.email@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        await _userService.EditUserAsync(user.Id, updatedUser);

        var result = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
        result.Should().NotBeNull();
        result.Login.Should().Be("john_doe_updated");
    }

    [Test]
    public async Task EditUserAsync_UsernameAlreadyTaken_ShouldThrowException()
    {
        var user1 = new User
        {
            Login = "john_doe",
            Password = "hashedPassword123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var user2 = new User
        {
            Login = "jane_doe",
            Password = "anotherPassword",
            Email = "jane.doe@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };
        await _userService.AddUserAsync(user1);
        await _userService.AddUserAsync(user2);

        var updatedUser = new User
        {
            Id = user1.Id,
            Login = "jane_doe",
            Password = "newHashedPassword",
            Email = "new.email@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        Func<Task> editUserAction = async () => await _userService.EditUserAsync(user1.Id, updatedUser);
        await editUserAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Login 'jane_doe' is already taken.");
    }

    [Test]
    public async Task EditUserAsync_UserNotExists_ShouldThrowException()
    {
        var updatedUser = new User
        {
            Login = "non_existing_user",
            Password = "newPassword",
            Email = "non.existing@example.com",
            FirstName = "Non",
            LastName = "Existing"
        };

        var fakeObjectId = ObjectId.GenerateNewId().ToString();

        Func<Task> editUserAction = async () => await _userService.EditUserAsync(fakeObjectId, updatedUser);
        await editUserAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with ID '{fakeObjectId}' not found.");
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldReturnUsers()
    {
        var usersList = new List<User>
        {
            new User { Login = "john_doe", FirstName = "John", LastName = "Doe" },
            new User { Login = "jane_doe", FirstName = "Jane", LastName = "Doe" }
        };
        foreach (var user in usersList)
        {
            await _userService.AddUserAsync(user);
        }

        var result = await _userService.GetAllUsersAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Login == "john_doe");
        result.Should().Contain(u => u.Login == "jane_doe");
    }
}
