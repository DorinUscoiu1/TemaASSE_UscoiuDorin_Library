// <copyright file="IBookDomain.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Repository interface for BookDomain-specific operations.
    /// </summary>
    public interface IBookDomain
    {
        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns>domains.</returns>
        IEnumerable<BookDomain> GetAll();

        /// <summary>
        /// Gets a domain by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>domain.</returns>
        BookDomain GetById(int id);

        /// <summary>
        /// Gets root domains (without parent).
        /// </summary>
        /// <returns>root domains.</returns>
        IEnumerable<BookDomain> GetRootDomains();

        /// <summary>
        /// Gets subdomains by parent ID.
        /// </summary>
        /// <param name="parentDomainId">id.</param>
        /// <returns>subdomains.</returns>
        IEnumerable<BookDomain> GetSubdomains(int parentDomainId);

        /// <summary>
        /// Adds a new domain.
        /// </summary>
        /// <param name="domain">domain.</param>
        void Add(BookDomain domain);

        /// <summary>
        /// Updates a domain.
        /// </summary>
        /// <param name="domain">domain.</param>
        void Update(BookDomain domain);

        /// <summary>
        /// Deletes a domain.
        /// </summary>
        /// <param name="id">id.</param>
        void Delete(int id);
    }
}
