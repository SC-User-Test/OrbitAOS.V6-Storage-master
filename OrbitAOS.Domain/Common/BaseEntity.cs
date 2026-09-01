namespace OrbitAOS.Domain.Common
{
    /// <summary>
    /// Base entity class providing common audit fields for all domain entities.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>Gets or sets the unique identifier for the entity.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the date and time the entity was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Gets or sets the date and time the entity was last updated.</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
