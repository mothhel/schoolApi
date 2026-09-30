namespace schoolApi.Services
{
    /*
        هذا Interface يحدد وظيفة إنشاء JWT Token
    */
    public interface IJwtService
    {
        string GenerateToken(Models.User user, string roleName);
    }
}