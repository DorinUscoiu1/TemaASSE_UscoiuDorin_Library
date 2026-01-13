// <copyright file="BookService.cs" company="Transilvania University of Brasov">
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
    /// Service implementation for book operations with business rule validation.
    /// Enforces: domain hierarchy, maximum domains per book, ancestor-descendant validation.
    /// </summary>
    public class BookService : IBookService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BookService));

        private readonly IBook bookRepository;
        private readonly IBookDomain bookDomainRepository;
        private readonly LibraryConfiguration configRepository;
        private readonly IValidator<Book> bookValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookService"/> class.
        /// </summary>
        /// <param name="bookRepository">The book repository.</param>
        /// <param name="bookDomainRepository">The book domain repository.</param>
        /// <param name="configRepository">The library configuration repository.</param>
        public BookService(
            IBook bookRepository,
            IBookDomain bookDomainRepository,
            LibraryConfiguration configRepository)
        {
            this.bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            this.bookDomainRepository = bookDomainRepository ?? throw new ArgumentNullException(nameof(bookDomainRepository));
            this.configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
            this.bookValidator = new BookValidator();
        }

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>A collection of all books.</returns>
        public IEnumerable<Book> GetAllBooks()
        {
            return this.bookRepository.GetAll();
        }

        /// <summary>
        /// Gets a book by ID.
        /// </summary>
        /// <param name="bookId">The book ID.</param>
        /// <returns>The book with the specified ID, or <c>null</c> if not found.</returns>
        public Book GetBookById(int bookId)
        {
            return this.bookRepository.GetById(bookId);
        }

        /// <summary>
        /// Gets books by author ID.
        /// </summary>
        /// <param name="authorId">The author ID.</param>
        /// <returns>A collection of books written by the specified author.</returns>
        public IEnumerable<Book> GetBooksByAuthor(int authorId)
        {
            return this.bookRepository.GetBooksByAuthor(authorId);
        }

        /// <summary>
        /// Gets books by domain, including inherited domains from hierarchy.
        /// If a book is in "Algoritmi", it's also retrieved from "Informatica" and "Stiinta".
        /// </summary>
        /// <param name="domainId">The domain ID.</param>
        /// <returns>A collection of books that belong to the specified domain and its descendant domains.</returns>
        public IEnumerable<Book> GetBooksByDomain(int domainId)
        {
            var domain = this.bookDomainRepository.GetById(domainId);
            if (domain == null)
            {
                return Enumerable.Empty<Book>();
            }

            var domainIds = new List<int> { domainId };
            this.GetDescendantDomainIds(domainId, domainIds);

            var booksInDomain = this.bookRepository.GetBooksByDomain(domainId);
            var booksInDescendants = domainIds
                .Skip(1)
                .SelectMany(did => this.bookRepository.GetBooksByDomain(did))
                .Distinct();

            return booksInDomain.Concat(booksInDescendants).Distinct();
        }

        /// <summary>
        /// Gets books available for borrowing.
        /// </summary>
        /// <returns>A collection of books that can currently be borrowed.</returns>
        public IEnumerable<Book> GetAvailableBooks()
        {
            return this.bookRepository.GetAvailableBooks();
        }

        /// <summary>
        /// Gets books in a domain including all subdomain books.
        /// If a book is in "Algoritmi", it's also retrieved from "Informatica" and "Stiinta".
        /// </summary>
        /// <param name="domainId">The domain ID.</param>
        /// <returns>A collection of books in the specified domain and its subdomains.</returns>
        public IEnumerable<Book> GetBooksInDomain(int domainId)
        {
            var domain = this.bookDomainRepository.GetById(domainId);
            if (domain == null)
            {
                return Enumerable.Empty<Book>();
            }

            var descendantIds = new List<int>();
            this.GetDescendantDomainIds(domainId, descendantIds);
            descendantIds.Add(domainId);

            var allBooks = new List<Book>();
            foreach (var id in descendantIds)
            {
                var booksInDomain = this.bookRepository.GetBooksByDomain(id);
                allBooks.AddRange(booksInDomain);
            }

            return allBooks.GroupBy(b => b.Id).Select(g => g.First());
        }

        /// <summary>
        /// Validates if book can be loaned based on availability.
        /// Rule: At least 10% of loanable copies must remain available.
        /// </summary>
        /// <param name="bookId">The book ID.</param>
        /// <returns><c>true</c> if the book can be borrowed; otherwise, <c>false</c>.</returns>
        public bool CanBorrowBook(int bookId)
        {
            var book = this.bookRepository.GetById(bookId);
            if (book == null)
            {
                return false;
            }

            return book.CanBeLoanable();
        }

        /// <summary>
        /// Gets books sorted by availability (most available first).
        /// </summary>
        /// <returns>A collection of books ordered by available copies in descending order.</returns>
        public IEnumerable<Book> GetBooksOrderedByAvailability()
        {
            var availableBooks = this.bookRepository.GetAvailableBooks();
            return availableBooks.OrderByDescending(b => b.GetAvailableCopies());
        }

        /// <summary>
        /// Gets books from specific domain without subdomains.
        /// </summary>
        /// <param name="domainId">The domain ID.</param>
        /// <returns>A collection of books directly associated with the specified domain.</returns>
        public IEnumerable<Book> GetBooksDirectlyInDomain(int domainId)
        {
            var domain = this.bookDomainRepository.GetById(domainId);
            if (domain == null)
            {
                return Enumerable.Empty<Book>();
            }

            return this.bookRepository.GetBooksByDomain(domainId);
        }

        /// <summary>
        /// Checks if book with ISBN exists.
        /// </summary>
        /// <param name="isbn">The ISBN to check.</param>
        /// <returns><c>true</c> if a book with the specified ISBN exists; otherwise, <c>false</c>.</returns>
        public bool IsbnExists(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return false;
            }

            var book = this.bookRepository.GetByISBN(isbn);
            return book != null;
        }

        /// <summary>
        /// Gets total books count.
        /// </summary>
        /// <returns>The total number of books.</returns>
        public int GetTotalBooksCount()
        {
            return this.bookRepository.GetAll().Count();
        }

        /// <summary>
        /// Gets books with no available copies (all borrowed).
        /// </summary>
        /// <returns>A collection of books that currently have no available copies.</returns>
        public IEnumerable<Book> GetBooksWithNoCopiesAvailable()
        {
            var allBooks = this.bookRepository.GetAll();
            return allBooks.Where(b => b.GetAvailableCopies() <= 0);
        }

        /// <summary>
        /// Gets books that are only available in reading room.
        /// </summary>
        /// <returns>A collection of books that can only be used in the reading room.</returns>
        public IEnumerable<Book> GetReadingRoomOnlyBooks()
        {
            var allBooks = this.bookRepository.GetAll();
            return allBooks.Where(b => b.TotalCopies == b.ReadingRoomOnlyCopies);
        }

        /// <summary>
        /// Creates a new book with comprehensive domain validation.
        /// Rule 1: Max DOMENII domains.
        /// Rule 2: No explicit ancestor-descendant relationships.
        /// Rule 3: Total copies must be positive.
        /// Rule 4: Book must belong to at least one domain.
        /// </summary>
        /// <param name="book">The book to create.</param>
        /// <param name="domainIds">The domain IDs that the book belongs to.</param>
        public void CreateBook(Book book, List<int> domainIds)
        {
            Logger.InfoFormat("Creating book. Title={0}, ISBN={1}", book?.Title, book?.ISBN);

            try
            {
                var validationResult = this.bookValidator.Validate(book);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    Logger.WarnFormat("Book validation failed. Title={0}, Errors={1}", book?.Title, errors);
                    throw new ValidationException(errors);
                }

                var existingBook = this.bookRepository.GetByISBN(book.ISBN);
                if (existingBook != null)
                {
                    Logger.WarnFormat("Duplicate ISBN detected. ISBN={0}", book.ISBN);
                    throw new InvalidOperationException($"O carte cu ISBN-ul {book.ISBN} exist? deja.");
                }

                if (domainIds == null || !domainIds.Any())
                {
                    Logger.Warn("CreateBook called with empty domainIds.");
                    throw new ArgumentException("Cartea trebuie s? apar?in? de cel pu?in un domeniu.");
                }

                if (domainIds.Count > this.configRepository.MaxDomainsPerBook)
                {
                    Logger.WarnFormat("Exceeded max domains. Count={0}, Max={1}", domainIds.Count, this.configRepository.MaxDomainsPerBook);
                    throw new InvalidOperationException($"S-a dep??it limita maxim? de domenii ({this.configRepository.MaxDomainsPerBook}).");
                }

                var domains = new List<BookDomain>();
                foreach (var id in domainIds)
                {
                    var dom = this.bookDomainRepository.GetById(id);
                    if (dom == null)
                    {
                        Logger.WarnFormat("Domain not found. DomainId={0}", id);
                        throw new KeyNotFoundException($"Domeniul cu ID {id} nu a fost g?sit.");
                    }

                    domains.Add(dom);
                }

                for (int i = 0; i < domains.Count; i++)
                {
                    for (int j = i + 1; j < domains.Count; j++)
                    {
                        if (this.IsAncestor(domains[i].Id, domains[j].Id) ||
                            this.IsAncestor(domains[j].Id, domains[i].Id))
                        {
                            Logger.WarnFormat("Domain hierarchy conflict. BookTitle={0}, DomainA={1}, DomainB={2}", book.Title, domains[i].Id, domains[j].Id);
                            throw new InvalidOperationException("O carte nu poate fi în acela?i timp într-un domeniu p?rinte ?i unul descendent.");
                        }
                    }
                }

                book.Domains = domains;
                this.bookRepository.Add(book);

                Logger.InfoFormat("Book created successfully. Title={0}, ISBN={1}", book.Title, book.ISBN);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error creating book. Title={0}, ISBN={1}", book?.Title, book?.ISBN), ex);
                throw;
            }
        }

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="book">The book to update.</param>
        public void UpdateBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            var validationResult = this.bookValidator.Validate(book);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            if (!this.ValidateBookDomains(book))
            {
                throw new InvalidOperationException("Book domain constraints violated.");
            }

            this.bookRepository.Update(book);
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="bookId">The book ID.</param>
        public void DeleteBook(int bookId)
        {
            this.bookRepository.Delete(bookId);
        }

        /// <summary>
        /// Gets available copies count.
        /// </summary>
        /// <param name="bookId">The book ID.</param>
        /// <returns>The number of currently available copies for the specified book.</returns>
        public int GetAvailableCopies(int bookId)
        {
            var book = this.bookRepository.GetById(bookId);
            return book?.GetAvailableCopies() ?? 0;
        }

        /// <summary>
        /// Validates book domain constraints.
        /// 1. Max DOMENII domains
        /// 2. No ancestor-descendant explicit relationships.
        /// </summary>
        /// <param name="book">The book to validate.</param>
        /// <returns><c>true</c> if the book satisfies all domain constraints; otherwise, <c>false</c>.</returns>
        public bool ValidateBookDomains(Book book)
        {
            if (book?.Domains == null)
            {
                return false;
            }

            var config = this.configRepository;

            if (book.Domains.Count > config.MaxDomainsPerBook)
            {
                return false;
            }

            var domainList = book.Domains.ToList();
            for (int i = 0; i < domainList.Count; i++)
            {
                for (int j = i + 1; j < domainList.Count; j++)
                {
                    if (this.IsAncestor(domainList[i].Id, domainList[j].Id) ||
                        this.IsAncestor(domainList[j].Id, domainList[i].Id))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if ancestorId is an ancestor of descendantId.
        /// </summary>
        private bool IsAncestor(int potentialAncestorId, int potentialDescendantId)
        {
            var currentDomain = this.bookDomainRepository.GetById(potentialDescendantId);

            while (currentDomain?.ParentDomainId.HasValue == true)
            {
                if (currentDomain.ParentDomainId == potentialAncestorId)
                {
                    return true;
                }

                currentDomain = this.bookDomainRepository.GetById(currentDomain.ParentDomainId.Value);
            }

            return false;
        }

        /// <summary>
        /// Gets all descendant domain IDs recursively.
        /// </summary>
        private void GetDescendantDomainIds(int parentDomainId, List<int> result)
        {
            var subdomains = this.bookDomainRepository.GetSubdomains(parentDomainId);
            foreach (var subdomain in subdomains)
            {
                result.Add(subdomain.Id);
                this.GetDescendantDomainIds(subdomain.Id, result);
            }
        }
    }
}
