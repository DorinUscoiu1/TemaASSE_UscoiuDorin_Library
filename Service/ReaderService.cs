// <copyright file="ReaderService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System;
    using System.Collections.Generic;
    using Data.Repositories;
    using Data.Validators;
    using Domain.Models;
    using FluentValidation;
    using log4net;

    /// <summary>
    /// Service implementation for reader operations with validation.
    /// Enforces: name consistency, contact information validation.
    /// </summary>
    public class ReaderService : IReaderService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ReaderService));

        private readonly IReader readerRepository;
        private readonly IValidator<Reader> readerValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderService"/> class.
        /// </summary>
        /// <param name="readerRepository">The reader repository.</param>
        public ReaderService(IReader readerRepository)
        {
            this.readerRepository = readerRepository ?? throw new ArgumentNullException(nameof(readerRepository));
            this.readerValidator = new ReaderValidator();
        }

        /// <summary>
        /// Gets all readers.
        /// </summary>
        /// <returns>Readers.</returns>
        public IEnumerable<Reader> GetAllReaders()
        {
            Logger.Info("GetAllReaders called.");
            return this.readerRepository.GetAll();
        }

        /// <summary>
        /// Gets a reader by ID.
        /// </summary>
        /// <param name="readerId">id.</param>
        /// <returns>Reader.</returns>
        public Reader GetReaderById(int readerId)
        {
            Logger.Info($"GetReaderById called. readerId={readerId}");
            return this.readerRepository.GetById(readerId);
        }

        /// <summary>
        /// Gets staff members.
        /// </summary>
        /// <returns>Readers.</returns>
        public IEnumerable<Reader> GetStaffMembers()
        {
            Logger.Info("GetStaffMembers called.");
            return this.readerRepository.GetStaffMembers();
        }

        /// <summary>
        /// Gets regular readers.
        /// </summary>
        /// <returns>Readers.</returns>
        public IEnumerable<Reader> GetRegularReaders()
        {
            Logger.Info("GetRegularReaders called.");
            return this.readerRepository.GetRegularReaders();
        }

        /// <summary>
        /// Creates a new reader with comprehensive validation.
        /// Rule 1: Names must be consistent and non-empty
        /// Rule 2: At least one contact method (phone or email)
        /// Rule 3: Address is required.
        /// </summary>
        /// <param name="reader">reader.</param>
        public void CreateReader(Reader reader)
        {
            Logger.Info("CreateReader called.");

            if (reader == null)
            {
                Logger.Error("CreateReader failed: reader is null.");
                throw new ArgumentNullException(nameof(reader));
            }

            // Validate using FluentValidation
            var validationResult = this.readerValidator.Validate(reader);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                Logger.Warn($"CreateReader validation failed: {errors}");
                throw new ValidationException(errors);
            }

            reader.RegistrationDate = DateTime.Now;
            this.readerRepository.Add(reader);

            Logger.Info($"CreateReader succeeded. readerId={reader.Id}");
        }

        /// <summary>
        /// Updates a reader.
        /// </summary>
        /// <param name="reader">reder.</param>
        public void UpdateReader(Reader reader)
        {
            Logger.Info($"UpdateReader called. readerId={reader?.Id}");

            if (reader == null)
            {
                Logger.Error("UpdateReader failed: reader is null.");
                throw new ArgumentNullException(nameof(reader));
            }

            // Validate using FluentValidation
            var validationResult = this.readerValidator.Validate(reader);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                Logger.Warn($"UpdateReader validation failed: {errors}");
                throw new ValidationException(errors);
            }

            this.readerRepository.Update(reader);
            Logger.Info($"UpdateReader succeeded. readerId={reader.Id}");
        }

        /// <summary>
        /// Deletes a reader.
        /// </summary>
        /// <param name="readerId">readerId.</param>
        public void DeleteReader(int readerId)
        {
            Logger.Info($"DeleteReader called. readerId={readerId}");
            this.readerRepository.Delete(readerId);
        }

        /// <summary>
        /// Validates reader data consistency.
        /// </summary>
        /// <param name="reader">reader.</param>
        /// <returns>bool.</returns>
        public bool ValidateReader(Reader reader)
        {
            Logger.Info($"ValidateReader called. readerId={reader?.Id}");

            if (reader == null)
            {
                return false;
            }

            var validationResult = this.readerValidator.Validate(reader);
            return validationResult.IsValid;
        }

        /// <summary>
        /// Gets staff readers.
        /// </summary>
        /// <returns>Readers.</returns>
        public IEnumerable<Reader> GetStaffReaders()
        {
            Logger.Info("GetStaffReaders called.");
            return this.readerRepository.GetStaffMembers();
        }
    }
}
