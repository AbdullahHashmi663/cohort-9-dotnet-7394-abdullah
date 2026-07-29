using TaskManager.API.DTOs;

namespace TaskManager.API.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<IEnumerable<UserOptionDto>> GetAllUsersAsync();
        Task<UserOptionDto> CreateUserAsync(UserCreateAdminDto dto);
        Task<UserOptionDto> UpdateUserAsync(int userId, UserUpdateAdminDto dto);
        Task<string> DeleteUserAsync(int userId, int currentUserId);
    }
}
