// <copyright file="IEdition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Repository interface for Edition-specific operations.
    /// </summary>
    public interface IEdition
    {
        /// <summary>
        /// Gets all editions.
        /// </summary>
        /// <returns>editions.</returns>
        IEnumerable<Edition> GetAll();

        /// <summary>
        /// Gets an edition by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>edition.</returns>
        Edition GetById(int id);

        /// <summary>
        /// Gets editions by book ID.
        /// </summary>
        /// <param name="bookId">bookId.</param>
        /// <returns>editions.</returns>
        IEnumerable<Edition> GetByBookId(int bookId);

        /// <summary>
        /// Gets editions by publisher.
        /// </summary>
        /// <param name="publisher">publisher.</param>
        /// <returns>editions.</returns>
        IEnumerable<Edition> GetByPublisher(string publisher);

        /// <summary>
        /// Adds a new edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        void Add(Edition edition);

        /// <summary>
        /// Updates an edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        void Update(Edition edition);

        /// <summary>
        /// Deletes an edition.
        /// </summary>
        /// <param name="id">id.</param>
        void Delete(int id);
    }
}
