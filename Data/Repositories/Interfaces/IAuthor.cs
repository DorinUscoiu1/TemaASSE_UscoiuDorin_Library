// <copyright file="IAuthor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Repository interface for Author-specific operations.
    /// </summary>
    public interface IAuthor
    {
        /// <summary>
        /// Gets all authors.
        /// </summary>
        /// <returns>Authors.</returns>
        IEnumerable<Author> GetAll();

        /// <summary>
        /// Gets an author by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>Author.</returns>
        Author GetById(int id);

        /// <summary>
        /// Gets authors by first name.
        /// </summary>
        /// <param name="firstName">firstName.</param>
        /// <returns>Authors.</returns>
        IEnumerable<Author> GetByFirstName(string firstName);

        /// <summary>
        /// Gets authors by last name.
        /// </summary>
        /// <returns>Authors.</returns>
        /// <param name="lastName">lastName.</param>
        IEnumerable<Author> GetByLastName(string lastName);

        /// <summary>
        /// Adds a new author.
        /// </summary>
        /// <param name="author">author.</param>
        void Add(Author author);

        /// <summary>
        /// Updates an author.
        /// </summary>
        /// <param name="author">author.</param>
        void Update(Author author);

        /// <summary>
        /// Deletes an author.
        /// </summary>
        /// <param name="id">id.</param>
        void Delete(int id);
    }
}
