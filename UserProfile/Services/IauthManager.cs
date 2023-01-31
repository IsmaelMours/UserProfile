using UserProfile.UserDto;
using System.Security.Claims;

namespace UserProfile.Services
{
    public class IauthManager 
    {
        internal Task<bool> ValidateUser(LoginUserDto UserDto)
        {
            throw new NotImplementedException();
        }

        internal Task<bool> ValidateUser(object userDTO)
        {
            throw new NotImplementedException();
        }
    }
}
