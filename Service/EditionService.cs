// <copyright file="EditionService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data.Repositories;
    using Data.Validators;
    using Domain.Models;
    using FluentValidation;
    using log4net;

    /// <summary>
    /// Service implementation for edition operations.
    /// Manages book editions with validation.
    /// </summary>
    public class EditionService : IEditionService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EditionService));

        private readonly IEdition editionRepository;
        private readonly IBook bookRepository;
        private readonly IValidator<Edition> editionValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditionService"/> class.
        /// </summary>
        /// <param name="editionRepository">The edition repository.</param>
        /// <param name="bookRepository">The book repository.</param>
        public EditionService(IEdition editionRepository, IBook bookRepository)
        {
            this.editionRepository = editionRepository;
            this.bookRepository = bookRepository;
            this.editionValidator = new EditionValidator();
        }

        /// <summary>
        /// Gets all editions.
        /// </summary>
        /// <returns>Editions.</returns>
        public IEnumerable<Edition> GetAllEditions()
        {
            Logger.Info("GetAllEditions called.");
            return this.editionRepository.GetAll();
        }

        /// <summary>
        /// Gets an edition by ID.
        /// </summary>
        /// <param name="editionId">id.</param>
        /// <returns>Edition.</returns>
        public Edition GetEditionById(int editionId)
        {
            Logger.Info($"GetEditionById called. editionId={editionId}");
            return this.editionRepository.GetById(editionId);
        }

        /// <summary>
        /// Gets editions by book ID.
        /// </summary>
        /// <param name="bookId">bookId.</param>
        /// <returns>Editions.</returns>
        public IEnumerable<Edition> GetEditionsByBook(int bookId)
        {
            Logger.Info($"GetEditionsByBook called. bookId={bookId}");
            return this.editionRepository.GetByBookId(bookId);
        }

        /// <summary>
        /// Gets editions by publisher.
        /// </summary>
        /// <param name="publisher">publisher.</param>
        /// <returns>editions.</returns>
        public IEnumerable<Edition> GetEditionsByPublisher(string publisher)
        {
            Logger.Info($"GetEditionsByPublisher called. publisher='{publisher}'");
            return this.editionRepository.GetByPublisher(publisher);
        }

        /// <summary>
        /// Creates a new edition with validation.
        /// </summary>
        /// <param name="edition">edition.</param>
        public void CreateEdition(Edition edition)
        {
                Logger.Info("CreateEdition called.");

                var validationResult = this.editionValidator.Validate(edition);
                if (!validationResult.IsValid)
                {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                Logger.Warn($"CreateEdition validation failed: {errors}");
                throw new ValidationException(errors);
                }

            // Verify book exists
                var book = this.bookRepository.GetById(edition.BookId);
                if (book == null)
                {
                    Logger.Warn($"CreateEdition failed: book not found. bookId={edition.BookId}");
                    throw new InvalidOperationException("Book not found.");
                }

                this.editionRepository.Add(edition);
                Logger.Info($"CreateEdition succeeded. editionId={edition.Id}");
            }

        /// <summary>
        /// Updates an edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        public void UpdateEdition(Edition edition)
        {
            Logger.Info($"UpdateEdition called. editionId={edition?.Id}");
            var validationResult = this.editionValidator.Validate(edition);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                Logger.Warn($"UpdateEdition validation failed: {errors}");
                throw new ValidationException(errors);
            }

            this.editionRepository.Update(edition);
            Logger.Info($"UpdateEdition succeeded. editionId={edition.Id}");
        }

        /// <summary>
        /// Deletes an edition.
        /// </summary>
        /// <param name="editionId">editionId.</param>
        public void DeleteEdition(int editionId)
        {
            Logger.Info($"DeleteEdition called. editionId={editionId}");

            var edition = this.editionRepository.GetById(editionId);
            if (edition != null)
            {
                this.editionRepository.Delete(editionId);
                Logger.Info($"DeleteEdition succeeded. editionId={editionId}");
            }
            else
            {
                Logger.Warn($"DeleteEdition: edition not found. editionId={editionId}");
            }
        }
    }
}
