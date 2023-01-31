using System.ComponentModel.DataAnnotations;

namespace UserProfile.UserDto
{
    public class LoginUserDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Password { get; set; }
    }
    public class UserDto : LoginUserDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Status { get; set; }

        public string PathOfImage { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}
