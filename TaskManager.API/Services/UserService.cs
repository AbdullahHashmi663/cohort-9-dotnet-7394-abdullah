using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("Profile requested for non-existent User ID {UserId}.", userId);
                throw new KeyNotFoundException("User not found.");
            }

            _logger.LogInformation("Profile fetched for User ID {UserId}.", userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                TotalTasks = user.Tasks.Count
            };
        }

        public async Task<IEnumerable<UserOptionDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserOptionDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

        public async Task<UserOptionDto> CreateUserAsync(UserCreateAdminDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existing != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin created User ID {UserId} ({Email}, Role: {Role}).", user.Id, user.Email, user.Role);

            return new UserOptionDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<UserOptionDto> UpdateUserAsync(int userId, UserUpdateAdminDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != userId);
            if (existing != null)
            {
                throw new InvalidOperationException("Email is already in use by another user.");
            }

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin updated User ID {UserId} ({Email}, Role: {Role}).", user.Id, user.Email, user.Role);

            return new UserOptionDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<string> DeleteUserAsync(int userId, int currentUserId)
        {
            if (userId == currentUserId)
            {
                throw new InvalidOperationException("You cannot delete your own admin account.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin User ID {AdminId} deleted User ID {UserId}.", currentUserId, userId);

            return "User deleted successfully.";
        }
    }
}
