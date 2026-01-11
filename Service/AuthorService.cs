// <copyright file="AuthorService.cs" company="Transilvania University of Brasov">
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
    /// Service implementation for author operations with business rule validation.
    /// Enforces: name validation, data consistency.
    /// </summary>
    public class AuthorService : IAuthorService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthorService));

        private readonly IAuthor authorRepository;
        private readonly IValidator<Author> authorValidator;
        private readonly LibraryConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorService"/> class.
        /// </summary>
        /// <param name="authorRepository">The author repository.</param>
        /// <param name="config">The library configuration.</param>
        public AuthorService(IAuthor authorRepository, LibraryConfiguration config)
        {
            this.authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.authorValidator = new AuthorValidator();
        }

        /// <summary>
        /// Gets all authors.
        /// </summary>
        /// <returns>Authors.</returns>
        public IEnumerable<Author> GetAllAuthors()
        {
            Logger.Info("GetAllAuthors called.");
            return this.authorRepository.GetAll();
        }

        /// <summary>
        /// Gets an author by ID.
        /// </summary>
        /// <param name="authorId">Id.</param>
        /// <returns>Authors.</returns>
        public Author GetAuthorById(int authorId)
        {
            Logger.Info($"GetAuthorById called. authorId={authorId}");
            return this.authorRepository.GetById(authorId);
        }

        /// <summary>
        /// Gets authors by first name.
        /// </summary>
        /// <param name="firstName">firstName.</param>
        /// <returns>Authors.</returns>
        public IEnumerable<Author> GetAuthorsByFirstName(string firstName)
        {
            Logger.Info($"GetAuthorsByFirstName called. firstName='{firstName}'");

            if (string.IsNullOrWhiteSpace(firstName))
            {
                return Enumerable.Empty<Author>();
            }

            return this.authorRepository.GetByFirstName(firstName);
        }

        /// <summary>
        /// Gets authors by last name.
        /// </summary>
        /// <param name="lastName">lastName.</param>
        /// <returns>Authors.</returns>
        public IEnumerable<Author> GetAuthorsByLastName(string lastName)
        {
            Logger.Info($"GetAuthorsByLastName called. lastName='{lastName}'");

            if (string.IsNullOrWhiteSpace(lastName))
            {
                return Enumerable.Empty<Author>();
            }

            return this.authorRepository.GetByLastName(lastName);
        }

        /// <summary>
        /// Gets books by author ID.
        /// </summary>
        /// <param name="authorId">authorId.</param>
        /// <returns>Books.</returns>
        public IEnumerable<Book> GetBooksByAuthor(int authorId)
        {
            Logger.Info($"GetBooksByAuthor called. authorId={authorId}");

            var author = this.authorRepository.GetById(authorId);
            if (author == null)
            {
                Logger.Warn($"Author not found. authorId={authorId}");
                return Enumerable.Empty<Book>();
            }

            return author.Books ?? Enumerable.Empty<Book>();
        }

        /// <summary>
        /// Creates a new author with comprehensive validation.
        /// </summary>
        /// <param name="author">Author.</param>
        public void CreateAuthor(Author author)
        {
            Logger.Info("CreateAuthor called.");

            if (author == null)
            {
                Logger.Error("CreateAuthor failed: author is null.");
                throw new ArgumentNullException(nameof(author));
            }

            var validationResult = this.authorValidator.Validate(author);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                Logger.Warn($"CreateAuthor validation failed: {errors}");
                throw new ValidationException(errors);
            }

            this.authorRepository.Add(author);
            Logger.Info($"CreateAuthor succeeded. authorId={author.Id}");
        }

        /// <summary>
        /// Updates an author.
        /// </summary>
        /// <param name="author">Author.</param>
        public void UpdateAuthor(Author author)
        {
            Logger.Info("UpdateAuthor called.");

            if (author == null)
            {
                Logger.Error("UpdateAuthor failed: author is null.");
                throw new ArgumentNullException(nameof(author));
            }

            var validationResult = this.authorValidator.Validate(author);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                Logger.Warn($"UpdateAuthor validation failed: {errors}");
                throw new ValidationException(errors);
            }

            this.authorRepository.Update(author);
            Logger.Info($"UpdateAuthor succeeded. authorId={author.Id}");
        }

        /// <summary>
        /// Deletes an author.
        /// </summary>
        /// <param name="authorId">AuthorId.</param>
        public void DeleteAuthor(int authorId)
        {
            Logger.Info($"DeleteAuthor called. authorId={authorId}");
            this.authorRepository.Delete(authorId);
        }
    }
}
