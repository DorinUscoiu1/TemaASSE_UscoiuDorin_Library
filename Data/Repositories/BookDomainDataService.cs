// <copyright file="BookDomainDataService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;

    /// <summary>
    /// Repository implementation for BookDomain-specific operations.
    /// </summary>
    public class BookDomainDataService : IBookDomain
    {
        private readonly LibraryDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookDomainDataService"/> class.
        /// </summary>
        /// <param name="context">The library database context.</param>
        public BookDomainDataService(LibraryDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns>domains.</returns>
        public IEnumerable<BookDomain> GetAll()
        {
            return this.context.Domains.ToList();
        }

        /// <summary>
        /// Gets a domain by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>domain.</returns>
        public BookDomain GetById(int id)
        {
            return this.context.Domains.Find(id);
        }

        /// <summary>
        /// Gets root domains (without parent).
        /// </summary>
        /// <returns>root domains.</returns>
        public IEnumerable<BookDomain> GetRootDomains()
        {
            return this.context.Domains
                .Where(d => d.ParentDomainId == null)
                .ToList();
        }

        /// <summary>
        /// Gets subdomains by parent ID.
        /// </summary>
        /// <param name="parentDomainId">parentDomainId.</param>
        /// <returns>subdomains.</returns>
        public IEnumerable<BookDomain> GetSubdomains(int parentDomainId)
        {
            return this.context.Domains
                .Where(d => d.ParentDomainId == parentDomainId)
                .ToList();
        }

        /// <summary>
        /// Gets domain by name.
        /// </summary>
        /// <param name="name">name.</param>
        /// <returns>domain.</returns>
        public BookDomain GetByName(string name)
        {
            return this.context.Domains
                .FirstOrDefault(d => d.Name == name);
        }

        /// <summary>
        /// Adds a new domain.
        /// </summary>
        /// <param name="domain">domain.</param>
        public void Add(BookDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            this.context.Domains.Add(domain);
            this.context.SaveChanges();
        }

        /// <summary>
        /// Updates a domain.
        /// </summary>
        /// <param name="domain">domain.</param>
        public void Update(BookDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            var existingDomain = this.context.Domains.Find(domain.Id);
            if (existingDomain != null)
            {
                existingDomain.Name = domain.Name;
                existingDomain.ParentDomainId = domain.ParentDomainId;
                this.context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes a domain.
        /// </summary>
        /// <param name="id">id.</param>
        public void Delete(int id)
        {
            var domain = this.context.Domains.Find(id);
            if (domain != null)
            {
                this.context.Domains.Remove(domain);
                this.context.SaveChanges();
            }
        }
    }
}
