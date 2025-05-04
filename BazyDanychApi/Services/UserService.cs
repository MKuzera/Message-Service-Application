using BazyDanychProjekt.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BazyDanychProjekt.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        public async Task AddUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            User user2 = new User
            {
                Login = user.Login,
                Password = user.Password,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            if (string.IsNullOrWhiteSpace(user2.Login))
                throw new ArgumentException("Login is required.", nameof(user2.Login));

            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required.", nameof(user2.Password));

            var existingUser = await _users.Find(u => u.Login == user2.Login).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with login '{user2.Login}' already exists.");
            }

            await _users.InsertOneAsync(user2);
        }

        public async Task RemoveUserAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
            {
                throw new FormatException("Invalid user ID format.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var result = await _users.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }
        }

        public async Task EditUserAsync(string userId, User updatedUser)
        {
            if (!ObjectId.TryParse(userId, out _))
            {
                throw new FormatException("Invalid user ID format.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var existingUser = await _users.Find(filter).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            var loginFilter = Builders<User>.Filter.Eq(u => u.Login, updatedUser.Login);
            var existingLoginUser = await _users.Find(loginFilter).FirstOrDefaultAsync();

            if (existingLoginUser != null && existingLoginUser.Id != userId)
            {
                throw new InvalidOperationException($"Login '{updatedUser.Login}' is already taken.");
            }

            var update = Builders<User>.Update
                .Set(u => u.Login, updatedUser.Login)
                .Set(u => u.Password, updatedUser.Password)
                .Set(u => u.Email, updatedUser.Email)
                .Set(u => u.FirstName, updatedUser.FirstName)
                .Set(u => u.LastName, updatedUser.LastName);

            await _users.UpdateOneAsync(filter, update);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
            {
                throw new FormatException("Invalid user ID format.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var user = await _users.Find(filter).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            return user;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }
    }
}
