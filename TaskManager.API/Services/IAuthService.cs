using TaskManager.API.DTOs;

namespace TaskManager.API.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(UserRegisterDto dto);
        Task<LoginResponseDto> LoginAsync(UserLoginDto dto);
    }
}
