// <copyright file="IReader.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Repository interface for Reader-specific operations.
    /// </summary>
    public interface IReader
    {
        /// <summary>
        /// Gets all readers.
        /// </summary>
        /// <returns>Readers.</returns>
        IEnumerable<Reader> GetAll();

        /// <summary>
        /// Gets a reader by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>Reader.</returns>
        Reader GetById(int id);

        /// <summary>
        /// Gets readers by staff status.
        /// </summary>
        /// <returns>Staff members.</returns>
        IEnumerable<Reader> GetStaffMembers();

        /// <summary>
        /// Gets all non-staff readers.
        /// </summary>
        /// <returns>Regular readers.</returns>
        IEnumerable<Reader> GetRegularReaders();

        /// <summary>
        /// Finds a reader by email.
        /// </summary>
        /// <param name="email">email.</param>
        /// <returns>Reader.</returns>
        Reader GetByEmail(string email);

        /// <summary>
        /// Gets readers with active borrowings.
        /// </summary>
        /// <returns>reader.</returns>
        IEnumerable<Reader> GetReadersWithActiveBorrowings();

        /// <summary>
        /// Checks if a reader id belongs to a staff member.
        /// </summary>
        /// <param name="id">Reader id.</param>
        /// <returns><c>true</c> if staff; otherwise <c>false</c>.</returns>
        bool IsStaff(int id);

        /// <summary>
        /// Adds a new reader.
        /// </summary>
        /// <param name="reader">reader.</param>
        void Add(Reader reader);

        /// <summary>
        /// Updates a reader.
        /// </summary>
        /// <param name="reader">reader.</param>
        void Update(Reader reader);

        /// <summary>
        /// Deletes a reader.
        /// </summary>
        /// <param name="id">id.</param>
        void Delete(int id);
    }
}
