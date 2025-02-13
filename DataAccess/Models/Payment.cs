namespace DataAccess.Models
{
        public class Payment
        {
            public int Id { get; set; }
            public string CardNumber { get; set; } = string.Empty;
            public string CardHolderName { get; set; } = string.Empty;
            public string ExpiryDate { get; set; } = string.Empty;
            public string CVV { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public bool IsSuccess { get; set; }
            public DateTime PaymentDate { get; set; }
        }
    }


