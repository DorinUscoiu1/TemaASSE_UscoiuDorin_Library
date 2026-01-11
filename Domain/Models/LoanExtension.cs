// <copyright file="LoanExtension.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     LOAN.EXTENSION table model class.
    /// </summary>
    public class LoanExtension
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the loan identifier.
        /// </summary>
        public int BorrowingId { get; set; }

        /// <summary>
        /// Gets or sets the loan.
        /// </summary>
        public Borrowing Borrowing { get; set; } = null;

        /// <summary>
        /// Gets or sets the extension date.
        /// </summary>
        public DateTime ExtensionDate { get; set; }

        /// <summary>
        /// Gets or sets the number of extension days granted.
        /// </summary>
        public int ExtensionDays { get; set; }
    }
}
