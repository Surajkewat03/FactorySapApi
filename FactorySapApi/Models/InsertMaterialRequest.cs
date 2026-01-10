namespace FactorySapApi.Models
{
    public class InsertMaterialRequest
    {
        public string BatchNo { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public decimal Quantity { get; set; }
    }
}
