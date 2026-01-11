// <copyright file="IAuthorService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Service interface for author operations with business rule validation.
    /// </summary>
    public interface IAuthorService
    {
        /// <summary>
        /// Gets all authors.
        /// </summary>
        /// <returns>authors.</returns>
        IEnumerable<Author> GetAllAuthors();

        /// <summary>
        /// Gets an author by ID.
        /// </summary>
        /// <param name="authorId">id.</param>
        /// <returns>author.</returns>
        Author GetAuthorById(int authorId);

        /// <summary>
        /// Gets authors by first name.
        /// </summary>
        /// <param name="firstName">name.</param>
        /// <returns>authors.</returns>
        IEnumerable<Author> GetAuthorsByFirstName(string firstName);

        /// <summary>
        /// Gets authors by last name.
        /// </summary>
        /// <param name="lastName">name.</param>
        /// <returns>authors.</returns>
        IEnumerable<Author> GetAuthorsByLastName(string lastName);

        /// <summary>
        /// Gets books by author ID.
        /// </summary>
        /// <param name="authorId">authorId.</param>
        /// <returns>books.</returns>
        IEnumerable<Book> GetBooksByAuthor(int authorId);

        /// <summary>
        /// Creates a new author with validation.
        /// </summary>
        /// <param name="author">author.</param>
        void CreateAuthor(Author author);

        /// <summary>
        /// Updates an author.
        /// </summary>
        /// <param name="author">author.</param>
        void UpdateAuthor(Author author);

        /// <summary>
        /// Deletes an author.
        /// </summary>
        /// <param name="authorId">authorId.</param>
        void DeleteAuthor(int authorId);
    }
}
