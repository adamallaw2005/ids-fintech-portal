using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IJwtTokenService
{
    AuthResponse CreateToken(UserAccount user);
}
