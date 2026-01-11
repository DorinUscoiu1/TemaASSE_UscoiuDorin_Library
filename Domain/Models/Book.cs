// <copyright file="Book.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a book in the library.
    /// A book can belong to one or more domains and have multiple editions.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets or sets the unique identifier for the book.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the book.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the book.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ISBN of the book.
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of copies.
        /// </summary>
        public int TotalCopies { get; set; }

        /// <summary>
        /// Gets or sets the number of copies available only in reading room.
        /// </summary>
        public int ReadingRoomOnlyCopies { get; set; }

        /// <summary>
        /// Gets or sets the collection of authors of this book.
        /// </summary>
        public ICollection<Author> Authors { get; set; } = new List<Author>();

        /// <summary>
        /// Gets or sets the collection of domains this book belongs to.
        /// </summary>
        public ICollection<BookDomain> Domains { get; set; } = new List<BookDomain>();

        /// <summary>
        /// Gets or sets the collection of editions of this book.
        /// </summary>
        public ICollection<Edition> Editions { get; set; } = new List<Edition>();

        /// <summary>
        /// Gets or sets the collection of borrowing records for this book.
        /// </summary>
        public ICollection<Borrowing> BorrowingRecords { get; set; } = new List<Borrowing>();

        /// <summary>gets the number of available copies for loan.</summary>
        /// <returns>int.</returns>
        public int GetAvailableCopies()
        {
            var loanedCopies = this.BorrowingRecords.Count(l => l.ReturnDate == null);
            return this.TotalCopies - this.ReadingRoomOnlyCopies - loanedCopies;
        }

        /// <summary>
        /// Checks if the book can be loaned.
        /// </summary>
        /// <returns>True if book can be loaned.</returns>
        public bool CanBeLoanable()
        {
            if (this.TotalCopies == this.ReadingRoomOnlyCopies)
            {
                return false;
            }

            var loanableCopies = this.TotalCopies - this.ReadingRoomOnlyCopies;
            var available = this.GetAvailableCopies();
            return available >= (loanableCopies * 0.1);
        }
    }
}
