namespace DataAccess.Models
{
        public class Payment
        {
            public int Id { get; set; }
            public string CardNumber { get; set; }
            public string CardHolderName { get; set; }
            public string ExpiryDate { get; set; }
            public string CVV { get; set; }
            public decimal Amount { get; set; }
            public bool IsSuccess { get; set; }
            public DateTime PaymentDate { get; set; }
        }
    }


