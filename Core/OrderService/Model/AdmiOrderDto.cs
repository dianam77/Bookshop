using DataAccess.Enums;

namespace Core.OrderService.Model
{
    public class AdmiOrderDto
    {
        public int Id { get; set; }

        public DateTime Payed { get; set; } = DateTime.MinValue; // Ensure it has a default value

        public string UserId { get; set; } = string.Empty; // Initialize non-nullable string

        public string Address { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public Status Status { get; set; }

        public string UserName { get; set; } = string.Empty;

        public List<string> Items { get; set; } = new List<string>(); // Initialize empty list
    }

}
