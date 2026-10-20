namespace do_an_epu.DTOs.VnPay
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; } = "other";
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
        public string BookingId { get; set; }
    }
}
