using System.ComponentModel.DataAnnotations;

namespace HMT_Tech.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }


        public string Comment { get; set; }

    }
}
