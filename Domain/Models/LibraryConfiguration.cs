// <copyright file="LibraryConfiguration.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;

    /// <summary>
    /// Represents the library configuration settings.
    /// Stores all configurable thresholds and limits for the library system.
    /// </summary>
    public class LibraryConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum number of domains a book can belong to (DOMENII).
        /// </summary>
        public int MaxDomainsPerBook { get; set; } = 3;

        /// <summary>
        /// Gets or sets the maximum number of books a reader can borrow in a period (NMC).
        /// </summary>
        public int MaxBooksPerPeriod { get; set; } = 10;

        /// <summary>
        /// Gets or sets the period in days for maximum books limit (PER).
        /// </summary>
        public int BorrowingPeriodDays { get; set; } = 28;

        /// <summary>
        /// Gets or sets the maximum number of books per borrow request (C).
        /// </summary>
        public int MaxBooksPerRequest { get; set; } = 6;

        /// <summary>
        /// Gets or sets the maximum number of books from the same domain in specified months (D).
        /// </summary>
        public int MaxBooksPerDomain { get; set; } = 3;

        /// <summary>
        /// Gets or sets the number of months to check for domain limit (L).
        /// </summary>
        public int DomainLimitMonths { get; set; } = 6;

        /// <summary>
        /// Gets or sets the maximum extension limit in days for a book (LIM).
        /// </summary>
        public int MaxExtensionDays { get; set; } = 28;

        /// <summary>
        /// Gets or sets the interval in days between consecutive borrows of the same book (DELTA).
        /// </summary>
        public int MinDaysBetweenBorrows { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum number of books a reader can borrow in one day (NCZ).
        /// </summary>
        public int MaxBooksPerDay { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum number of books staff can distribute in one day (PERSIMP).
        /// </summary>
        public int MaxBooksStaffPerDay { get; set; } = 3;

        /// <summary>
        /// Gets or sets the minimum percentage of available copies required to allow borrowing.
        /// Default is 0.10 (10%).
        /// </summary>
        public double MinAvailablePercentage { get; set; } = 0.1;
    }
}
