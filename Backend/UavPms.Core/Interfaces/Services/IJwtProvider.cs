using UavPms.Core.Entities;

namespace UavPms.Core.Interfaces.Services;

public interface IJwtProvider
{
    string GenerateToken(AppUser user, IList<string> roles);
}