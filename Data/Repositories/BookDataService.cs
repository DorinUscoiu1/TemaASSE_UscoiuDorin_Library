// <copyright file="BookDataService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;

    /// <summary>
    /// Repository implementation for Book-specific operations.
    /// </summary>
    public class BookDataService : IBook
    {
        private readonly LibraryDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookDataService"/> class.
        /// </summary>
        /// <param name="context">The library database context.</param>
        public BookDataService(LibraryDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>books.</returns>
        public IEnumerable<Book> GetAll()
        {
            return this.context.Books.ToList();
        }

        /// <summary>
        /// Gets a book by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>book.</returns>
        public Book GetById(int id)
        {
            return this.context.Books.Find(id);
        }

        /// <summary>
        /// Gets books by author ID.
        /// </summary>
        /// <param name="authorId">authorId.</param>
        /// <returns>books.</returns>
        public IEnumerable<Book> GetBooksByAuthor(int authorId)
        {
            return this.context.Books
                .Where(b => b.Authors.Any(a => a.Id == authorId))
                .ToList();
        }

        /// <summary>
        /// Gets books by domain ID.
        /// </summary>
        /// <param name="domainId">domainId.</param>
        /// <returns>books.</returns>
        public IEnumerable<Book> GetBooksByDomain(int domainId)
        {
            return this.context.Books
                .Where(b => b.Domains.Any(d => d.Id == domainId))
                .ToList();
        }

        /// <summary>
        /// Gets books with available copies.
        /// </summary>
        /// <returns>books.</returns>
        public IEnumerable<Book> GetAvailableBooks()
        {
            return this.context.Books
                .Where(b => b.GetAvailableCopies() > 0)
                .ToList();
        }

        /// <summary>
        /// Gets books by ISBN.
        /// </summary>
        /// <param name="isbn">isbn.</param>
        /// <returns>book.</returns>
        public Book GetByISBN(string isbn)
        {
            return this.context.Books
                .FirstOrDefault(b => b.ISBN == isbn);
        }

        /// <summary>
        /// Adds a new book.
        /// </summary>
        /// <param name="book">book.</param>
        public void Add(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            this.context.Books.Add(book);
            this.context.SaveChanges();
        }

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="book">book.</param>
        public void Update(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            var existingBook = this.context.Books.Find(book.Id);
            if (existingBook != null)
            {
                existingBook.Title = book.Title;
                existingBook.ISBN = book.ISBN;
                existingBook.Description = book.Description;
                existingBook.TotalCopies = book.TotalCopies;
                existingBook.ReadingRoomOnlyCopies = book.ReadingRoomOnlyCopies;
                this.context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">id.</param>
        public void Delete(int id)
        {
            var book = this.context.Books.Find(id);
            if (book != null)
            {
                this.context.Books.Remove(book);
                this.context.SaveChanges();
            }
        }
    }
}
