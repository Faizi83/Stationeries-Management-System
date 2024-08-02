using System.ComponentModel.DataAnnotations;

namespace HMT_Tech.Models
{
    public class Notification { 

        [Key]
        public int Id { get; set; }

        public int SenderId { get; set; }
        public string Name { get; set; }

        public string Image { get; set; }

        public TimeSpan Time { get; set; }

        public string Stationery { get; set; }




    }
}
