public class UpdateQtyRequest
{
    public int Id { get; set; }
    public decimal NewQty { get; set; }
    public string Remarks { get; set; }
    public string Condition { get; set; }   // 🔴 SAME NAME
}
