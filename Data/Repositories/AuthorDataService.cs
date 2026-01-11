// <copyright file="AuthorDataService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;

    /// <summary>
    /// Repository implementation for Author-specific operations.
    /// </summary>
    public class AuthorDataService : IAuthor
    {
        private readonly LibraryDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorDataService"/> class.
        /// </summary>
        /// <param name="context">The library database context.</param>
        public AuthorDataService(LibraryDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets all authors.
        /// </summary>
        /// <returns>authors.</returns>
        public IEnumerable<Author> GetAll()
        {
            return this.context.Authors.ToList();
        }

        /// <summary>
        /// Gets an author by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>author.</returns>
        public Author GetById(int id)
        {
            return this.context.Authors.Find(id);
        }

        /// <summary>
        /// Gets authors by first name.
        /// </summary>
        /// <param name="firstName">first.</param>
        /// <returns>authors.</returns>
        public IEnumerable<Author> GetByFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return new List<Author>();
            }

            return this.context.Authors
                .Where(a => a.FirstName.Contains(firstName))
                .ToList();
        }

        /// <summary>
        /// Gets authors by last name.
        /// </summary>
        /// <param name="lastName">last.</param>
        /// <returns>authors.</returns>
        public IEnumerable<Author> GetByLastName(string lastName)
        {
            return this.context.Authors
                .Where(a => a.LastName == lastName)
                .ToList();
        }

        /// <summary>
        /// Adds a new author.
        /// </summary>
        /// <param name="author">author.</param>
        public void Add(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            this.context.Authors.Add(author);
            this.context.SaveChanges();
        }

        /// <summary>
        /// Updates an author.
        /// </summary>
        /// <param name="author">author.</param>
        public void Update(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            var existingAuthor = this.context.Authors.Find(author.Id);
            if (existingAuthor != null)
            {
                existingAuthor.FirstName = author.FirstName;
                existingAuthor.LastName = author.LastName;
                this.context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes an author.
        /// </summary>
        /// <param name="id">id.</param>
        public void Delete(int id)
        {
            var author = this.context.Authors.Find(id);
            if (author != null)
            {
                this.context.Authors.Remove(author);
                this.context.SaveChanges();
            }
        }
    }
}
