// <copyright file="IBookDomainService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Service interface for book domain operations with business rule validation.
    /// </summary>
    public interface IBookDomainService
    {
        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns>All book domains.</returns>
        IEnumerable<BookDomain> GetAllDomains();

        /// <summary>
        /// Gets a domain by ID.
        /// </summary>
        /// <param name="domainId">id.</param>
        /// <returns>Book domain.</returns>
        BookDomain GetDomainById(int domainId);

        /// <summary>
        /// Gets root domains (without parent).
        /// </summary>
        /// <returns>Root book domains.</returns>
        IEnumerable<BookDomain> GetRootDomains();

        /// <summary>
        /// Gets subdomains by parent ID.
        /// </summary>
        /// <param name="parentDomainId">Parent domain ID.</param>
        /// <returns>Subdomains.</returns>
        IEnumerable<BookDomain> GetSubdomains(int parentDomainId);

        /// <summary>
        /// Creates a new domain.
        /// </summary>
        /// <param name="domain">Domain to create.</param>
        void CreateDomain(BookDomain domain);

        /// <summary>
        /// Updates a domain.
        /// </summary>
        /// <param name="domain">Domain to update.</param>
        void UpdateDomain(BookDomain domain);

        /// <summary>
        /// Deletes a domain.
        /// </summary>
        /// <param name="domainId">Domain ID to delete.</param>
        void DeleteDomain(int domainId);

        /// <summary>
        /// Gets all ancestor domains for a domain.
        /// </summary>
        /// <param name="domainId">Domain ID.</param>
        /// <returns>Ancestor domains.</returns>
        IEnumerable<BookDomain> GetAncestorDomains(int domainId);

        /// <summary>
        /// Gets all descendant domains for a domain.
        /// </summary>
        /// <param name="domainId">Domain ID.</param>
        /// <returns>Descendant domains.</returns>
        IEnumerable<BookDomain> GetDescendantDomains(int domainId);
    }
}
