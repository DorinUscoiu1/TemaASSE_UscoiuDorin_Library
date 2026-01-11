// <copyright file="Reader.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Domain.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a library reader/patron.
    /// Readers can borrow books according to specific constraints and rules.
    /// </summary>
    public class Reader
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reader.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the first name of the reader.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the reader.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street address of the reader.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the reader.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the reader.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the registration date of the reader.
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reader is also a library staff member.
        /// </summary>
        public bool IsStaff { get; set; }

        /// <summary>
        /// Gets or sets the collection of borrowing records for this reader.
        /// </summary>
        public virtual ICollection<Borrowing> BorrowingRecords { get; set; } = new List<Borrowing>();

        /// <summary>
        /// Gets or sets the collection of borrowing records given by this reader (if staff).
        /// </summary>
        public virtual ICollection<Borrowing> BorrowingGiven { get; set; } = new List<Borrowing>();

        /// <summary>FullName.</summary>
        /// <returns>Full name.</returns>
        public string GetFullName()
        {
            return $"{this.FirstName} {this.LastName}";
        }
    }
}
