namespace NavyBule.Core.Domain
{
    /// <summary>
    /// Class BaseEntity. If ORM is EF, I recommended to inherit the class. 
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
    }
}
