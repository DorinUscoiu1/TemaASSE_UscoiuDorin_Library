// <copyright file="IBorrowing.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Repository interface for Borrowing-specific operations.
    /// </summary>
    public interface IBorrowing
    {
        /// <summary>
        /// Gets active borrowings for a specific reader.
        /// </summary>
        /// <param name="readerId">The ID of the reader.</param>
        /// <returns>Active borrowings.</returns>
        IEnumerable<Borrowing> GetActiveBorrowingsByReader(int readerId);

        /// <summary>
        /// Gets overdue borrowings.
        /// </summary>
        /// <returns>Overdue borrowings.</returns>
        IEnumerable<Borrowing> GetOverdueBorrowings();

        /// <summary>
        /// Gets borrowings by book ID.
        /// </summary>
        /// <param name="bookId">The ID of the book.</param>
        /// <returns>Borrowings for the specified book.</returns>
        IEnumerable<Borrowing> GetBorrowingsByBook(int bookId);

        /// <summary>
        /// Gets borrowings within a date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>Borrowings within the specified date range.</returns>
        IEnumerable<Borrowing> GetBorrowingsByDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Adds a new borrowing.
        /// </summary>
        /// <param name="borrowing">borrowing.</param>
        void Add(Borrowing borrowing);

        /// <summary>
        /// Updates a borrowing.
        /// </summary>
        /// <param name="borrowing">borrowing.</param>
        void Update(Borrowing borrowing);

        /// <summary>
        /// Gets a borrowing by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>borrowing.</returns>
        Borrowing GetById(int id);
    }
}
