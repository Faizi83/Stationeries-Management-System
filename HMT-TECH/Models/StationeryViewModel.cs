using System.ComponentModel.DataAnnotations;

namespace HMT_Tech.Models
{
    public class StationeryViewModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Stationery { get; set; }

        public string Manager { get; set; }

        public string Date { get; set; }

        public int Price { get; set; }

        public int Qty { get; set; }

        public string Status { get; set; }

        public int RequestId { get; set; }

        public int Stationery_id { get; set; }



    }
}
