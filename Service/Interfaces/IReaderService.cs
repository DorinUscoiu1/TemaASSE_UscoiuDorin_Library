// <copyright file="IReaderService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Service interface for reader operations with validation.
    /// </summary>
    public interface IReaderService
    {
        /// <summary>
        /// Gets all readers.
        /// </summary>
        /// <returns>readers.</returns>
        IEnumerable<Reader> GetAllReaders();

        /// <summary>
        /// Gets a reader by ID.
        /// </summary>
        /// <param name="readerId">id.</param>
        /// <returns>reader.</returns>
        Reader GetReaderById(int readerId);

        /// <summary>
        /// Gets staff members.
        /// </summary>
        /// <returns>staff members.</returns>
        IEnumerable<Reader> GetStaffMembers();

        /// <summary>
        /// Gets regular readers.
        /// </summary>
        /// <returns>regular readers.</returns>
        IEnumerable<Reader> GetRegularReaders();

        /// <summary>
        /// Creates a new reader with validation.
        /// Validates: name consistency, at least one contact method.
        /// </summary>
        /// <param name="reader">reader.</param>
        void CreateReader(Reader reader);

        /// <summary>
        /// Updates a reader.
        /// </summary>
        /// <param name="reader">reader.</param>
        void UpdateReader(Reader reader);

        /// <summary>
        /// Deletes a reader.
        /// </summary>
        /// <param name="readerId">readerId.</param>
        void DeleteReader(int readerId);

        /// <summary>
        /// Validates reader data consistency.
        /// </summary>
        /// <param name="reader">reader.</param>
        /// <returns>is valid.</returns>
        bool ValidateReader(Reader reader);
    }
}
