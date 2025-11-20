using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models;

/// <summary>
/// Represents a product return request
/// </summary>
public class Return
{
    /// <summary>
    /// Primary key for the return record
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Order table
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to the Order
    /// </summary>
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    /// <summary>
    /// Foreign key to the OrderItem table
    /// </summary>
    [Required]
    public int OrderItemId { get; set; }

    /// <summary>
    /// Navigation property to the OrderItem
    /// </summary>
    [ForeignKey(nameof(OrderItemId))]
    public OrderItem? OrderItem { get; set; }

    /// <summary>
    /// Reason for the return
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Status of the return (pending, approved, rejected, completed)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Timestamp when the return was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the return was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
