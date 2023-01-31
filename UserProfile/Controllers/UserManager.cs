using System.Data.Entity;
using UserProfile.Model;

namespace UserProfile.Controllers
{
    public class UserManager
    {
        internal Task<bool> CheckPasswordAsync(User user, string password)
        {
            throw new NotImplementedException();
        }

        internal Task<User> FindByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        internal Task GetRolesAsync(User user)
        {
            throw new NotImplementedException();
        }

        public static implicit operator DbContext(UserManager v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator UserManager(User v)
        {
            throw new NotImplementedException();
        }
    }
}