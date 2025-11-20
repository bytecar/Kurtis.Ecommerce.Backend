using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models;

/// <summary>
/// Stores user preferences for personalized shopping experience
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// Primary key for the user preferences record
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the User table (one-to-one relationship)
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to the User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    /// <summary>
    /// JSON array of favorite category names
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string FavoriteCategoriesJson { get; set; } = "[]";

    /// <summary>
    /// JSON array of favorite color preferences
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string FavoriteColorsJson { get; set; } = "[]";

    /// <summary>
    /// JSON array of favorite occasion types
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string FavoriteOccasionsJson { get; set; } = "[]";

    /// <summary>
    /// Minimum price range preference
    /// </summary>
    public int? PriceRangeMin { get; set; }

    /// <summary>
    /// Maximum price range preference
    /// </summary>
    public int? PriceRangeMax { get; set; }

    /// <summary>
    /// Timestamp when preferences were last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
