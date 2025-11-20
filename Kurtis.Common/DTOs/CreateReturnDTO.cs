namespace Kurtis.Common.DTOs
{
    /// <summary>
    /// DTO for creating a return request
    /// </summary>
    public class CreateReturnDTO
    {
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public string Reason { get; set; } = null!;
    }
}
