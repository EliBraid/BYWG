using BYWG.Shared.Models;

namespace BYWG.Auth.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    bool ValidateToken(string token);
    User? GetUserFromToken(string token);
}
