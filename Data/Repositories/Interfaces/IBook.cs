// <copyright file="IBook.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Repository interface for Book-specific operations.
    /// </summary>
    public interface IBook
    {
        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>books.</returns>
        IEnumerable<Book> GetAll();

        /// <summary>
        /// Gets a book by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>book.</returns>
        Book GetById(int id);

        /// <summary>
        /// Gets books by author ID.
        /// </summary>
        /// <param name="authorId">id.</param>
        /// <returns>books.</returns>
        IEnumerable<Book> GetBooksByAuthor(int authorId);

        /// <summary>
        /// Gets books by domain ID.
        /// </summary>
        /// <param name="domainId">id.</param>
        /// <returns>books.</returns>
        IEnumerable<Book> GetBooksByDomain(int domainId);

        /// <summary>
        /// Gets books with available copies.
        /// </summary>
        /// <returns>books.</returns>
        IEnumerable<Book> GetAvailableBooks();

        /// <summary>
        /// Gets books by ISBN.
        /// </summary>
        /// <param name="isbn">isbn.</param>
        /// <returns>books.</returns>
        Book GetByISBN(string isbn);

        /// <summary>
        /// Adds a new book.
        /// </summary>
        /// <param name="book">book.</param>
        void Add(Book book);

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="book">book.</param>
        void Update(Book book);

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">id.</param>
        void Delete(int id);
    }
}
