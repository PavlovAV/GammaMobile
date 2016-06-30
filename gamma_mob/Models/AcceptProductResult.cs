namespace gamma_mob.Models
{
    public class AcceptProductResult
    {
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string ResultMessage { get; set; }
        public bool AlreadyAccepted { get; set; }
        public string SourcePlace { get; set; }
    }
}