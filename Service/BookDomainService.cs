// <copyright file="BookDomainService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data.Repositories;
    using Domain.Models;
    using log4net;

    /// <summary>
    /// Service implementation for book domain operations.
    /// Manages domain hierarchy and validates domain relationships.
    /// </summary>
    public class BookDomainService : IBookDomainService
    {
        private readonly IBookDomain bookDomainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookDomainService"/> class.
        /// </summary>
        /// <param name="bookDomainRepository">The book domain repository.</param>
        public BookDomainService(IBookDomain bookDomainRepository)
        {
            this.bookDomainRepository = bookDomainRepository;
        }

        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns>Domains.</returns>
        public IEnumerable<BookDomain> GetAllDomains()
        {
            return this.bookDomainRepository.GetAll();
        }

        /// <summary>
        /// Gets a domain by ID.
        /// </summary>
        /// <param name="domainId">id.</param>
        /// <returns>Domain.</returns>
        public BookDomain GetDomainById(int domainId)
        {
            return this.bookDomainRepository.GetById(domainId);
        }

        /// <summary>
        /// Gets root domains (without parent).
        /// </summary>
        /// <returns>Domains.</returns>
        public IEnumerable<BookDomain> GetRootDomains()
        {
            return this.bookDomainRepository.GetRootDomains();
        }

        /// <summary>
        /// Gets subdomains by parent ID.
        /// </summary>
        /// <param name="parentDomainId">id.</param>
        /// <returns>Domains.</returns>
        public IEnumerable<BookDomain> GetSubdomains(int parentDomainId)
        {
            return this.bookDomainRepository.GetSubdomains(parentDomainId);
        }

        /// <summary>
        /// Creates a new domain with validation.
        /// </summary>
        /// <param name="domain">domain.</param>
        public void CreateDomain(BookDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            // Normalize input: trim and handle null safely
            domain.Name = domain.Name?.Trim();

            if (string.IsNullOrEmpty(domain.Name))
            {
                throw new ArgumentException("Domain name is required and cannot be empty or whitespace.", nameof(domain.Name));
            }

            if (domain.ParentDomainId.HasValue)
            {
                var parentDomain = this.bookDomainRepository.GetById(domain.ParentDomainId.Value);
                if (parentDomain == null)
                {
                    throw new InvalidOperationException("Parent domain not found.");
                }
            }

            this.bookDomainRepository.Add(domain);
        }

        /// <summary>
        /// Updates a domain.
        /// </summary>
        /// <param name="domain">domain.</param>
        public void UpdateDomain(BookDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            if (string.IsNullOrWhiteSpace(domain.Name))
            {
                throw new ArgumentException("Domain name is required.");
            }

            if (domain.ParentDomainId.HasValue && domain.ParentDomainId.Value == domain.Id)
            {
                throw new InvalidOperationException("A domain cannot be its own parent.");
            }

            this.bookDomainRepository.Update(domain);
        }

        /// <summary>
        /// Deletes a domain.
        /// </summary>
        /// <param name="domainId">domainId.</param>
        public void DeleteDomain(int domainId)
        {
            var domain = this.bookDomainRepository.GetById(domainId);
            if (domain != null)
            {
                var subdomains = this.bookDomainRepository.GetSubdomains(domainId);
                if (subdomains.Any())
                {
                    throw new InvalidOperationException("Cannot delete domain with subdomains.");
                }

                if (domain.Books.Any())
                {
                    throw new InvalidOperationException("Cannot delete domain with books.");
                }

                this.bookDomainRepository.Delete(domainId);
            }
        }

        /// <summary>
        /// Gets all ancestor domains for a domain recursively.
        /// </summary>
        /// <param name="domainId">domainId.</param>
        /// <returns>Domains.</returns>
        public IEnumerable<BookDomain> GetAncestorDomains(int domainId)
        {
            var ancestors = new List<BookDomain>();
            var currentDomain = this.bookDomainRepository.GetById(domainId);

            while (currentDomain?.ParentDomainId.HasValue == true)
            {
                var parent = this.bookDomainRepository.GetById(currentDomain.ParentDomainId.Value);
                if (parent != null)
                {
                    ancestors.Add(parent);
                    currentDomain = parent;
                }
                else
                {
                    break;
                }
            }

            return ancestors;
        }

        /// <summary>
        /// Gets all descendant domains for a domain recursively.
        /// </summary>
        /// <param name="domainId">domainId.</param>
        /// <returns>Domains.</returns>
        public IEnumerable<BookDomain> GetDescendantDomains(int domainId)
        {
            var descendants = new List<BookDomain>();
            var subdomains = this.bookDomainRepository.GetSubdomains(domainId);

            foreach (var subdomain in subdomains)
            {
                descendants.Add(subdomain);
                var subDescendants = this.GetDescendantDomains(subdomain.Id);
                descendants.AddRange(subDescendants);
            }

            return descendants;
        }
    }
}
