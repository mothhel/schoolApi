using school.Domain.Entities;

namespace school.Application.Interfaces
{
    /*
        هذا Interface يحدد وظيفة إنشاء JWT Token
    */
    public interface IJwtService
    {
        string GenerateToken(User user, string roleName);
    }
}