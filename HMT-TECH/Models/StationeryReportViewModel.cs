

namespace HMT_Tech.Models
{
    public class StationeryReportViewModel
    {
        public string StationeryName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }
        public double CostPercentage { get; set; }
        public int HeadCount { get; set; }
        public decimal CumulativeCost { get; set; }
    }
}