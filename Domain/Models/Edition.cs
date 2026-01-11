// <copyright file="Edition.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a specific edition of a book (publisher, year, page count, type, etc.).
    /// </summary>
    public class Edition
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the book identifier.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the book.
        /// </summary>
        public Book Book { get; set; } = null;

        /// <summary>
        /// Gets or sets the publisher name.
        /// </summary>
        public string Publisher { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the publication year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the edition number.
        /// </summary>
        public int EditionNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of pages.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the book type (e.g., hardcover, paperback).
        /// </summary>
        public string BookType { get; set; } = string.Empty;
    }
}
