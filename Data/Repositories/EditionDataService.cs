// <copyright file="EditionDataService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;

    /// <summary>
    /// Repository implementation for Edition-specific operations.
    /// </summary>
    public class EditionDataService : IEdition
    {
        private readonly LibraryDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditionDataService"/> class.
        /// </summary>
        /// <param name="context">The library database context.</param>
        public EditionDataService(LibraryDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all editions.
        /// </summary>
        /// <returns>editions.</returns>
        public IEnumerable<Edition> GetAll()
        {
            return this.context.Editions.ToList();
        }

        /// <summary>
        /// Gets an edition by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>edition.</returns>
        public Edition GetById(int id)
        {
            return this.context.Editions.Find(id);
        }

        /// <summary>
        /// Gets editions by book ID.
        /// </summary>
        /// <param name="bookId">The ID of the book for which to retrieve editions.</param>
        /// <returns>A collection of <see cref="Edition"/> objects associated with the specified book ID.</returns>
        public IEnumerable<Edition> GetByBookId(int bookId)
        {
            return this.context.Editions
                .Where(e => e.BookId == bookId)
                .ToList();
        }

        /// <summary>
        /// Gets editions by publisher.
        /// </summary>
        /// <param name="publisher">publisher.</param>
        /// <returns>editions.</returns>
        public IEnumerable<Edition> GetByPublisher(string publisher)
        {
            return this.context.Editions
                .Where(e => e.Publisher == publisher)
                .ToList();
        }

        /// <summary>
        /// Adds a new edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        public void Add(Edition edition)
        {
            if (edition == null)
            {
                throw new ArgumentNullException(nameof(edition));
            }

            this.context.Editions.Add(edition);
            this.context.SaveChanges();
        }

        /// <summary>
        /// Updates an edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        public void Update(Edition edition)
        {
            if (edition == null)
            {
                throw new ArgumentNullException(nameof(edition));
            }

            var existingEdition = this.context.Editions.Find(edition.Id);
            if (existingEdition != null)
            {
                existingEdition.Publisher = edition.Publisher;
                existingEdition.Year = edition.Year;
                existingEdition.EditionNumber = edition.EditionNumber;
                existingEdition.PageCount = edition.PageCount;
                existingEdition.BookType = edition.BookType;
                this.context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes an edition.
        /// </summary>
        /// <param name="id">id.</param>
        public void Delete(int id)
        {
            var edition = this.context.Editions.Find(id);
            if (edition != null)
            {
                this.context.Editions.Remove(edition);
                this.context.SaveChanges();
            }
        }
    }
}
