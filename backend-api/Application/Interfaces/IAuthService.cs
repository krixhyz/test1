using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;
namespace WeatherAPI.Application.Interfaces;
public interface IAuthService {
    Task<ApiResponse<object>> LoginAsync(LoginDto dto);
    Task<ApiResponse<object>> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task SeedRolesAndAdminAsync();
}