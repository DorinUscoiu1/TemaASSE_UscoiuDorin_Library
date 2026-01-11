// <copyright file="IBookService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Service interface for book operations with business rule validation.
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>books.</returns>
        IEnumerable<Book> GetAllBooks();

        /// <summary>
        /// Gets a book by ID.
        /// </summary>
        /// <param name="bookId">bookId.</param>
        /// <returns>book.</returns>
        Book GetBookById(int bookId);

        /// <summary>
        /// Gets books by author ID.
        /// </summary>
        /// <param name="authorId">authorId.</param>
        /// <returns>books.</returns>
        IEnumerable<Book> GetBooksByAuthor(int authorId);

        /// <summary>
        /// Gets books by domain (including ancestor domains via hierarchy).
        /// </summary>
        /// <param name="domainId">domainId.</param>
        /// <returns>books.</returns>
        IEnumerable<Book> GetBooksByDomain(int domainId);

        /// <summary>
        /// Gets books available for borrowing.
        /// </summary>
        /// <returns>books.</returns>
        IEnumerable<Book> GetAvailableBooks();

        /// <summary>
        /// Creates a new book with domain validation.
        /// Validates: max DOMENII domains, no ancestor-descendant relationships.
        /// </summary>
        /// <param name="book">book.</param>
        /// <param name="domainIds">domainIds.</param>
        void CreateBook(Book book, List<int> domainIds);

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="book">book.</param>
        void UpdateBook(Book book);

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="bookId">bookId.</param>
        void DeleteBook(int bookId);

        /// <summary>
        /// Gets available copies count for a book.
        /// </summary>
        /// <param name="bookId">bookId.</param>
        /// <returns>available copies.</returns>
        int GetAvailableCopies(int bookId);

        /// <summary>
        /// Validates book domain constraints.
        /// </summary>
        /// <param name="book">book.</param>
        /// <returns>is valid.</returns>
        bool ValidateBookDomains(Book book);
    }
}
