// <copyright file="ReaderDataService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;

    /// <summary>
    /// Repository implementation for Reader-specific operations.
    /// </summary>
    public class ReaderDataService : IReader
    {
        private readonly LibraryDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderDataService"/> class.
        /// </summary>
        /// <param name="context">The library database context.</param>
        public ReaderDataService(LibraryDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all readers.
        /// </summary>
        /// <returns>readers.</returns>
        public IEnumerable<Reader> GetAll()
        {
            return this.context.Readers.ToList();
        }

        /// <summary>
        /// Gets a reader by ID.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>reader.</returns>
        public Reader GetById(int id)
        {
            return this.context.Readers.Find(id);
        }

        /// <summary>
        /// Gets readers by staff status.
        /// </summary>
        /// <returns>staff members.</returns>
        public IEnumerable<Reader> GetStaffMembers()
        {
            return this.context.Readers
                .Where(r => r.IsStaff)
                .ToList();
        }

        /// <summary>
        /// Gets all non-staff readers.
        /// </summary>
        /// <returns>regular readers.</returns>
        public IEnumerable<Reader> GetRegularReaders()
        {
            return this.context.Readers
                .Where(r => !r.IsStaff)
                .ToList();
        }

        /// <summary>
        /// Finds a reader by email.
        /// </summary>
        /// <param name="email">email.</param>
        /// <returns>reader.</returns>
        public Reader GetByEmail(string email)
        {
            return this.context.Readers
                .FirstOrDefault(r => r.Email == email);
        }

        /// <summary>
        /// Gets readers with active borrowings.
        /// </summary>
        /// <returns>readers.</returns>
        public IEnumerable<Reader> GetReadersWithActiveBorrowings()
        {
            return this.context.Readers
                .Where(r => r.BorrowingRecords.Any(b => b.IsActive))
                .ToList();
        }

        /// <summary>
        /// Checks if a reader id belongs to a staff member.
        /// </summary>
        /// <param name="id">Reader id.</param>
        /// <returns><c>true</c> if staff; otherwise <c>false</c>.</returns>
        public bool IsStaff(int id)
        {
            return this.context.Readers.Any(r => r.Id == id && r.IsStaff);
        }

        /// <summary>
        /// Adds a new reader.
        /// </summary>
        /// <param name="reader">reader.</param>
        public void Add(Reader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.context.Readers.Add(reader);
            this.context.SaveChanges();
        }

        /// <summary>
        /// Updates a reader.
        /// </summary>
        /// <param name="reader">reader.</param>
        public void Update(Reader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var existingReader = this.context.Readers.Find(reader.Id);
            if (existingReader != null)
            {
                existingReader.FirstName = reader.FirstName;
                existingReader.LastName = reader.LastName;
                existingReader.Address = reader.Address;
                existingReader.PhoneNumber = reader.PhoneNumber;
                existingReader.Email = reader.Email;
                existingReader.IsStaff = reader.IsStaff;
                this.context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes a reader.
        /// </summary>
        /// <param name="id">id.</param>
        public void Delete(int id)
        {
            var reader = this.context.Readers.Find(id);
            if (reader != null)
            {
                this.context.Readers.Remove(reader);
                this.context.SaveChanges();
            }
        }
    }
}
