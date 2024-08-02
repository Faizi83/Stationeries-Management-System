using System.ComponentModel.DataAnnotations;

namespace HMT_Tech.Models
{
    public class Register
    {
        [Key]
        public int Id { get; set; } 

        public string Name { get; set; }

        public string Identity { get; set; }

        public string Gender { get; set; }

        public string Role { get; set; }

        public string Manager { get; set; }

        public int Balance { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Image{ get; set; }



    }
}
