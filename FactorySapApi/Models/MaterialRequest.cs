namespace FactorySapApi.Models
{
    public class MaterialRequest
    {
        public string InputString { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
