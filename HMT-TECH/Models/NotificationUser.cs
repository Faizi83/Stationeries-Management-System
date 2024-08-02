using System.ComponentModel.DataAnnotations;

namespace HMT_Tech.Models
{
    public class NotificationUser { 

        [Key]
        public int Id { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }

        public string Image { get; set; }

        public TimeSpan Time { get; set; }

        public string Stationery { get; set; }




    }
}
