// <copyright file="Program.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace TemaASSE_UscoiuDorin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Data;
    using Data.Repositories;
    using Domain.Models;
    using log4net;
    using log4net.Config;
    using Service;

    /// <summary>
    /// Console entry point.
    /// </summary>
    internal class Program
    {
        private const string DemoDomainScience = "DEMO_SCIENCE";
        private const string DemoDomainCs = "DEMO_CS";
        private const string DemoIsbn = "23454320001";
        private const string DemoStaffEmail = "demo.staff@local";
        private const string DemoUserEmail = "demo.user@local";
        private const string DemoEditionPublisher = "DEMO_PUBLISHER";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            Logger.Info("Application started.");

            try
            {
                RunInteractive();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Unhandled exception.", ex);
                throw;
            }
            finally
            {
                Logger.Info("Application stopped.");
                LogManager.Shutdown();
            }
        }

        private static void RunInteractive()
        {
            Console.WriteLine("==== Library demo (EF6 + log4net) ====");
            Console.WriteLine("1) Populate demo data (safe re-run)");
            Console.WriteLine("2) Depopulate demo data (delete demo rows)");
            Console.WriteLine("3) Borrow demo book (requires populate)");
            Console.WriteLine("4) Return demo borrowing (requires existing active borrowing)");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            if (string.Equals(input, "0", StringComparison.Ordinal))
            {
                return;
            }

            using (var context = new LibraryDbContext())
            {
                // Helpful when debugging.

                var config = new LibraryConfiguration
                {
                    MinAvailablePercentage = 0,
                    MaxBooksStaffPerDay = 100,
                    MaxExtensionDays = 10,
                    MaxBooksPerPeriod = 5,
                    MaxBooksPerDay = 3,
                    MaxBooksPerDomain = 2,
                    DomainLimitMonths = 3,
                    MinDaysBetweenBorrows = 7,
                    MaxBooksPerRequest = 3,
                    BorrowingPeriodDays = 30,
                };

                var domainRepository = new BookDomainDataService(context);
                var bookRepository = new BookDataService(context);
                var readerRepository = new ReaderDataService(context);
                var borrowingRepository = new BorrowingDataService(context);
                var editionRepository = new EditionDataService(context);

                var bookService = new BookService(bookRepository, domainRepository, config);
                var readerService = new ReaderService(readerRepository);
                var editionService = new EditionService(editionRepository, bookRepository);
                var borrowingService = new BorrowingService(borrowingRepository, bookRepository, readerRepository, config);

                if (string.Equals(input, "1", StringComparison.Ordinal))
                {
                    Populate(context, bookService, readerService, editionService);
                    Console.WriteLine("Populate completed.");
                    return;
                }

                if (string.Equals(input, "2", StringComparison.Ordinal))
                {
                    Depopulate(context);
                    Console.WriteLine("Depopulate completed.");
                    return;
                }

                if (string.Equals(input, "3", StringComparison.Ordinal))
                {
                    BorrowDemoBook(context, borrowingService);
                    Console.WriteLine("Borrow completed.");
                    return;
                }

                if (string.Equals(input, "4", StringComparison.Ordinal))
                {
                    ReturnDemoBorrowing(context, borrowingService);
                    Console.WriteLine("Return completed.");
                    return;
                }

                Console.WriteLine("Unknown option.");
            }
        }

        private static void Populate(
            LibraryDbContext context,
            BookService bookService,
            ReaderService readerService,
            EditionService editionService)
        {
            Logger.Info("Populate started.");

            // Domain hierarchy
            var science = EnsureDomain(context, DemoDomainScience, parentDomainId: null);
            var cs = EnsureDomain(context, DemoDomainCs, parentDomainId: science.Id);
            EnsureReader(context, readerService, DemoStaffEmail, "Demo", "Staff", isStaff: true);
            EnsureReader(context, readerService, DemoUserEmail, "Demo", "User", isStaff: false);

            var book = EnsureBook(context, bookService, DemoIsbn, "Demo Book", new List<int> { cs.Id });
            EnsureEdition(context, editionService, book.Id);

            Logger.Info("Populate finished.");
        }

        private static void Depopulate(LibraryDbContext context)
        {
            Logger.Info("Depopulate started.");

            // 1) Borrowings (FK to Reader/Book)
            var borrowings = context.Borrowings
                .Where(b =>
                    (b.Book != null && b.Book.ISBN == DemoIsbn) ||
                    (b.Reader != null && (b.Reader.Email == DemoUserEmail || b.Reader.Email == DemoStaffEmail)) ||
                    (b.Staff != null && (b.Staff.Email == DemoUserEmail || b.Staff.Email == DemoStaffEmail)))
                .ToList();

            if (borrowings.Any())
            {
                context.Borrowings.RemoveRange(borrowings);
                context.SaveChanges();
                Logger.InfoFormat("Deleted borrowings: {0}", borrowings.Count);
            }

            // 2) Editions for demo book
            var editions = context.Editions
                .Where(e => e.Book != null && e.Book.ISBN == DemoIsbn)
                .ToList();

            if (editions.Any())
            {
                context.Editions.RemoveRange(editions);
                context.SaveChanges();
                Logger.InfoFormat("Deleted editions: {0}", editions.Count);
            }

            // 3) Demo book
            var book = context.Books.FirstOrDefault(b => b.ISBN == DemoIsbn);
            if (book != null)
            {
                context.Books.Remove(book);
                context.SaveChanges();
                Logger.Info("Deleted demo book.");
            }

            // 4) Demo readers
            var readers = context.Readers
                .Where(r => r.Email == DemoUserEmail || r.Email == DemoStaffEmail)
                .ToList();

            if (readers.Any())
            {
                context.Readers.RemoveRange(readers);
                context.SaveChanges();
                Logger.InfoFormat("Deleted readers: {0}", readers.Count);
            }

            // 5) Demo domains (delete child first)
            var domains = context.Domains
                .Where(d => d.Name == DemoDomainCs || d.Name == DemoDomainScience)
                .ToList()
                .OrderByDescending(d => d.ParentDomainId.HasValue)
                .ToList();

            if (domains.Any())
            {
                context.Domains.RemoveRange(domains);
                context.SaveChanges();
                Logger.InfoFormat("Deleted domains: {0}", domains.Count);
            }

            Logger.Info("Depopulate finished.");
        }

        private static void BorrowDemoBook(LibraryDbContext context, BorrowingService borrowingService)
        {
            var staff = context.Readers.FirstOrDefault(r => r.Email == DemoStaffEmail);
            var user = context.Readers.FirstOrDefault(r => r.Email == DemoUserEmail);
            var book = context.Books.FirstOrDefault(b => b.ISBN == DemoIsbn);

            if (staff == null || user == null || book == null)
            {
                throw new InvalidOperationException("Demo data missing. Run Populate first.");
            }

            var alreadyActive = context.Borrowings.Any(b => b.ReaderId == user.Id && b.BookId == book.Id && b.IsActive);
            if (alreadyActive)
            {
                Logger.Info("Borrow skipped: already has an active borrowing for demo book.");
                return;
            }

            borrowingService.BorrowBook(user.Id, book.Id, borrowingDays: 14, staffId: staff.Id);
            Logger.Info("Borrowed demo book.");
        }

        private static void ReturnDemoBorrowing(LibraryDbContext context, BorrowingService borrowingService)
        {
            var user = context.Readers.FirstOrDefault(r => r.Email == DemoUserEmail);
            var book = context.Books.FirstOrDefault(b => b.ISBN == DemoIsbn);

            if (user == null || book == null)
            {
                throw new InvalidOperationException("Demo data missing. Run Populate first.");
            }

            var borrowing = context.Borrowings
                .Where(b => b.ReaderId == user.Id && b.BookId == book.Id && b.IsActive)
                .OrderByDescending(b => b.BorrowingDate)
                .FirstOrDefault();

            if (borrowing == null)
            {
                Logger.Info("No active borrowing found to return.");
                return;
            }

            borrowingService.ReturnBorrowing(borrowing.Id, DateTime.Now);
            Logger.InfoFormat("Returned borrowingId={0}", borrowing.Id);
        }

        private static BookDomain EnsureDomain(LibraryDbContext context, string name, int? parentDomainId)
        {
            var existing = context.Domains.FirstOrDefault(d => d.Name == name);
            if (existing != null)
            {
                return existing;
            }

            var domain = new BookDomain
            {
                Name = name,
                ParentDomainId = parentDomainId,
            };

            context.Domains.Add(domain);
            context.SaveChanges();

            Logger.InfoFormat("Created domain: Id={0}, Name={1}, ParentId={2}", domain.Id, domain.Name, domain.ParentDomainId);
            return domain;
        }

        private static Reader EnsureReader(
            LibraryDbContext context,
            ReaderService readerService,
            string email,
            string firstName,
            string lastName,
            bool isStaff)
        {
            var existing = context.Readers.FirstOrDefault(r => r.Email == email);
            if (existing != null)
            {
                return existing;
            }

            var reader = new Reader
            {
                FirstName = firstName,
                LastName = lastName,
                Address = "Demo Address",
                Email = email,
                PhoneNumber = "000",
                IsStaff = isStaff,
                RegistrationDate = DateTime.Now,
            };

            readerService.CreateReader(reader);

            var created = context.Readers.FirstOrDefault(r => r.Email == email);
            if (created == null)
            {
                throw new InvalidOperationException("Reader creation failed unexpectedly.");
            }

            Logger.InfoFormat("Created reader: Id={0}, Email={1}, IsStaff={2}", created.Id, created.Email, created.IsStaff);
            return created;
        }

        private static Book EnsureBook(
            LibraryDbContext context,
            BookService bookService,
            string isbn,
            string title,
            List<int> domainIds)
        {
            var existing = context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if (existing != null)
            {
                return existing;
            }

            var book = new Book
            {
                Title = title,
                ISBN = isbn,
                Description = "Inserted by demo runner",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author>
                {
                    new Author { FirstName = "Demo", LastName = "Author" },
                },
            };

            bookService.CreateBook(book, domainIds);

            var created = context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if (created == null)
            {
                throw new InvalidOperationException("Book creation failed unexpectedly.");
            }

            Logger.InfoFormat("Created book: Id={0}, ISBN={1}, Title={2}", created.Id, created.ISBN, created.Title);
            return created;
        }

        private static Edition EnsureEdition(LibraryDbContext context, EditionService editionService, int bookId)
        {
            var existing = context.Editions.FirstOrDefault(e => e.BookId == bookId && e.Publisher == DemoEditionPublisher);
            if (existing != null)
            {
                return existing;
            }

            var edition = new Edition
            {
                BookId = bookId,
                Publisher = DemoEditionPublisher,
                Year = DateTime.Now.Year,
                EditionNumber = 1,
                PageCount = 250,
                BookType = "Paperback",
            };

            editionService.CreateEdition(edition);

            var created = context.Editions.FirstOrDefault(e => e.BookId == bookId && e.Publisher == DemoEditionPublisher);
            if (created == null)
            {
                throw new InvalidOperationException("Edition creation failed unexpectedly.");
            }

            Logger.InfoFormat("Created edition: Id={0}, BookId={1}, Publisher={2}", created.Id, created.BookId, created.Publisher);
            return created;
        }
    }
}
