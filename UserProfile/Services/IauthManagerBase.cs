using UserProfile.UserDto;

namespace UserProfile.Services
{
    public abstract class IauthManagerBase
    {
        internal abstract Task<bool> ValidateUser(LoginUserDto UserDto);
        internal abstract Task<bool> ValidateUser(object userDTO);
        private abstract Task<string> CreateToken();
        private abstract Task<bool> ValidateUser(LoginUserDto UserDto);
    }
}