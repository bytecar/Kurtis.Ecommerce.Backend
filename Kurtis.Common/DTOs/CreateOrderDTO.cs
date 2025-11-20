using Kurtis.Common.Models;

namespace Kurtis.common.DTOs
{
    public class CreateOrderDTO
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Address { get; set; }
        public int UserId { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = [];
    }
}