using TaskManager.API.DTOs;

namespace TaskManager.API.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
    }
}
