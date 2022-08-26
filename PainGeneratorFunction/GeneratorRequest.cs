namespace PainGeneratorFunction
{
    public class GeneratorRequest
    {
        public int PaymentQuantity { get; set; }
        public int CreditorQuantity { get; set; }
        public string DebtorAccount { get; set; }
        public string CreditorAccount { get; set; }
    }
}
