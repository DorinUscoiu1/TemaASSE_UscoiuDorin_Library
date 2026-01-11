// <copyright file="IBorrowingService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System;
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Service interface for borrowing operations with business rules validation.
    /// </summary>
    public interface IBorrowingService
    {
        /// <summary>
        /// Attempts to borrow a book for a reader with all business rule validations.
        /// Also records the staff member that processed the borrowing.
        /// </summary>
        /// <param name="readerId">Reader id.</param>
        /// <param name="bookId">Book id.</param>
        /// <param name="borrowingDays">Borrowing days.</param>
        /// <param name="staffId">Staff member id.</param>
        void BorrowBook(int readerId, int bookId, int borrowingDays, int staffId);

        /// <summary>
        /// Creates borrowing records for a reader with the specified books and borrowing period.
        /// </summary>
        /// <param name="readerId">The unique identifier of the reader.</param>
        /// <param name="bookIds">A list of book identifiers to be borrowed.</param>
        /// <param name="borrowingDate">The date when the borrowing starts.</param>
        /// <param name="daysToBorrow">The number of days the books are borrowed for.</param>
        void CreateBorrowings(int readerId, List<int> bookIds, DateTime borrowingDate, int daysToBorrow);

        /// <summary>
        /// Creates borrowing records for a reader with the specified books and borrowing period.
        /// Also records the staff member that processed the borrowings.
        /// </summary>
        /// <param name="readerId">Reader id.</param>
        /// <param name="bookIds">Book ids.</param>
        /// <param name="borrowingDate">Borrowing date.</param>
        /// <param name="daysToBorrow">Days to borrow.</param>
        /// <param name="staffId">Staff member id.</param>
        void CreateBorrowings(int readerId, List<int> bookIds, DateTime borrowingDate, int daysToBorrow, int staffId);

        /// <summary>
        /// Returns a borrowed book.
        /// </summary>
        /// <param name="borrowingId">id.</param>
        /// <param name="returnDate">date.</param>
        void ReturnBorrowing(int borrowingId, DateTime returnDate);

        /// <summary>
        /// Extends a borrowing period if allowed.
        /// </summary>
        /// <param name="borrowingId">id.</param>
        /// <param name="extensionDays">days.</param>
        /// <param name="extensionDate">date.</param>
        void ExtendBorrowing(int borrowingId, int extensionDays, DateTime extensionDate);

        /// <summary>
        /// Gets all active borrowings for a reader.
        /// </summary>
        /// <param name="readerId">id.</param>
        /// <returns>list.</returns>
        IEnumerable<Borrowing> GetActiveBorrowings(int readerId);

        /// <summary>
        /// Gets overdue borrowings.
        /// </summary>
        /// <returns>list.</returns>
        ///

        IEnumerable<Borrowing> GetOverdueBorrowings();

        /// <summary>
        /// Validates if a reader can borrow a book.
        /// </summary>
        /// <param name="readerId">id.</param>
        /// <param name="bookId">id1.</param>
        /// <returns>bool.</returns>
        bool CanBorrowBook(int readerId, int bookId);

        /// <summary>
        /// Gets the count of active borrowings for a reader.
        /// </summary>
        /// <param name="readerId">id.</param>
        /// <returns>int.</returns>
        int GetActiveBorrowingCount(int readerId);
    }
}
