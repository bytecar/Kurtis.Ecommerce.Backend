namespace Kurtis.Common.DTOs
{
    /// <summary>
    /// DTO for updating user preferences
    /// </summary>
    public class UpdateUserPreferencesDTO
    {
        public string[]? FavoriteCategories { get; set; }
        public string[]? FavoriteColors { get; set; }
        public string[]? FavoriteOccasions { get; set; }
        public int? PriceRangeMin { get; set; }
        public int? PriceRangeMax { get; set; }
    }
}
