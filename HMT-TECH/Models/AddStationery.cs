using System.ComponentModel.DataAnnotations;

namespace HMT_Tech.Models
{
    public class AddStationery
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public int Price { get; set;}

        public int Quantity { get; set; }

    }
}
