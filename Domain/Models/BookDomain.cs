// <copyright file="BookDomain.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a domain/category of books (e.g., Science, Mathematics).
    /// Domains can have subdomains forming a hierarchical structure.
    /// </summary>
    public class BookDomain
    {
        /// <summary>
        /// Gets or sets the unique identifier for the domain.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the domain.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the parent domain (null if this is a root domain).
        /// </summary>
        public int? ParentDomainId { get; set; }

        /// <summary>
        /// Gets or sets the parent domain (null if this is a root domain).
        /// </summary>
        public virtual BookDomain ParentDomain { get; set; }

        /// <summary>
        /// Gets or sets the collection of subdomains.
        /// </summary>
        public virtual ICollection<BookDomain> Subdomains { get; set; } = new List<BookDomain>();

        /// <summary>
        /// Gets or sets the collection of books in this domain.
        /// </summary>
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        /// <summary>
        /// Gets all ancestor domains including this one.
        /// </summary>
        /// <returns>List of ancestor domains.</returns>
        public List<BookDomain> GetAncestors()
        {
            var ancestors = new List<BookDomain> { this };
            var current = this.ParentDomain;
            while (current != null)
            {
                ancestors.Add(current);
                current = current.ParentDomain;
            }

            return ancestors;
        }

        /// <summary>
        /// Checks if this domain is an ancestor of another domain.
        /// </summary>
        /// <param name="other">The other domain.</param>
        /// <returns>True if this is an ancestor of other.</returns>
        public bool IsAncestorOf(BookDomain other)
        {
            var current = other.ParentDomain;
            while (current != null)
            {
                if (current.Id == this.Id)
                {
                    return true;
                }

                current = current.ParentDomain;
            }

            return false;
        }
    }
}
