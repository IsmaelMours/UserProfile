using Microsoft.AspNetCore.Identity;

namespace UserProfile.Model
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
        public string PathOfImage { get; set; }
        public object Password { get; internal set; }
        public IEnumerable<object> Roles { get; internal set; }
    }
}
