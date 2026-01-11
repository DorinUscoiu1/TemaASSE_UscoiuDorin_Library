// <copyright file="Borrowing.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a borrowing transaction of a book copy by a reader.
    /// Tracks the borrowing and return dates, extensions, and current status.
    /// </summary>
    public class Borrowing
    {
        /// <summary>
        /// Gets or sets the unique identifier for the borrowing record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the reader who borrowed the book.
        /// </summary>
        public int ReaderId { get; set; }

        /// <summary>
        /// Gets or sets the reader who borrowed the book.
        /// </summary>
        public Reader Reader { get; set; } = null;

        /// <summary>
        /// Gets or sets the ID of the reader who borrowed the book.
        /// </summary>
        public int? StaffId { get; set; }

        /// <summary>
        /// Gets or sets the reader who borrowed the book.
        /// </summary>
        public Reader Staff { get; set; } = null;

        /// <summary>
        /// Gets or sets the ID of the book (for quick reference and filtering).
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the book (for quick reference and filtering).
        /// </summary>
        public Book Book { get; set; } = null;

        /// <summary>
        /// Gets or sets the date and time when the book was borrowed.
        /// </summary>
        public DateTime BorrowingDate { get; set; }

        /// <summary>
        /// Gets or sets the due date for returning the book.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the actual return date (null if not yet returned).
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Gets or sets the extensions granted for this loan.
        /// </summary>
        public ICollection<LoanExtension> Extensions { get; set; } = new List<LoanExtension>();

        /// <summary>
        /// Gets or sets a value indicating whether this borrowing record is currently active.
        /// Use as property to support assignment and boolean checks across the codebase/tests.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets total extension days (tracked explicitly by services/tests).
        /// </summary>
        public int TotalExtensionDays { get; set; }

        /// <summary>
        /// Gets or sets date of the last extension, if any.
        /// </summary>
        public DateTime? LastExtensionDate { get; set; }

        /// <summary>
        /// Gets or sets the initial number of borrowing days when the record was created.
        /// </summary>
        public int InitialBorrowingDays { get; set; }

        /// <summary>
        /// Gets the total extension days granted.
        /// If TotalExtensionDays was set explicitly, return it; otherwise compute from Extensions.
        /// </summary>
        /// <returns>Total extension days.</returns>
        public int GetTotalExtensionDays()
        {
            if (this.TotalExtensionDays > 0)
            {
                return this.TotalExtensionDays;
            }

            return this.Extensions?.Sum(e => e.ExtensionDays) ?? 0;
        }
    }
}
