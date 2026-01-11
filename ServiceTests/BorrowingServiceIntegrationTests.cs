// <copyright file="BorrowingServiceIntegrationTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace ServiceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Data.Repositories;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Service;

    /// <summary>
    /// Integration tests for <see cref="BorrowingService"/> using a real database context.
    /// </summary>
    [TestClass]
    public class BorrowingServiceIntegrationTests
    {
        /// <summary>
        /// Helper class to hold integration test services.
        /// </summary>
        private class IntegrationTestServices
        {
            public LibraryDbContext Context { get; set; }

            public BookDataService BookRepo { get; set; }

            public BookDomainDataService DomainRepo { get; set; }

            public ReaderDataService ReaderRepo { get; set; }

            public BorrowingDataService BorrowingRepo { get; set; }

            public LibraryConfiguration Config { get; set; }

            public ReaderService ReaderService { get; set; }

            public BookService BookService { get; set; }

            public BorrowingService BorrowingService { get; set; }

            public BookDomainService DomainService { get; set; }
        }

        /// <summary>
        /// Creates and initializes all services needed for integration tests.
        /// </summary>
        /// <param name="context">Database context to use.</param>
        /// <returns>Integration services container.</returns>
        private IntegrationTestServices CreateIntegrationServices(LibraryDbContext context)
        {
            var services = new IntegrationTestServices
            {
                Context = context,
                BookRepo = new BookDataService(context),
                DomainRepo = new BookDomainDataService(context),
                ReaderRepo = new ReaderDataService(context),
                BorrowingRepo = new BorrowingDataService(context),
                Config = new LibraryConfiguration(),
            };

            services.ReaderService = new ReaderService(services.ReaderRepo);
            services.DomainService = new BookDomainService(services.DomainRepo);
            services.BookService = new BookService(services.BookRepo, services.DomainRepo, services.Config);
            services.BorrowingService = new BorrowingService(services.BorrowingRepo, services.BookRepo, services.ReaderRepo, services.Config);

            return services;
        }

        /// <summary>
        /// Helper method to completely clean up all test data.
        /// </summary>
        private void CompleteCleanup(LibraryDbContext context, List<int> bookIds = null, List<int> readerIds = null, List<int> domainIds = null)
        {
            if (bookIds != null && bookIds.Any())
            {
                var borrowings = context.Borrowings.Where(b => bookIds.Contains(b.BookId)).ToList();
                foreach (var b in borrowings)
                {
                    context.Borrowings.Remove(b);
                }

                context.SaveChanges();
            }

            if (readerIds != null && readerIds.Any())
            {
                var borrowings = context.Borrowings.Where(b => readerIds.Contains(b.ReaderId)).ToList();
                foreach (var b in borrowings)
                {
                    context.Borrowings.Remove(b);
                }

                context.SaveChanges();
            }

            if (readerIds != null && readerIds.Any())
            {
                var readers = context.Readers.Where(r => readerIds.Contains(r.Id)).ToList();
                foreach (var reader in readers)
                {
                    context.Readers.Remove(reader);
                }

                context.SaveChanges();
            }

            if (bookIds != null && bookIds.Any())
            {
                var books = context.Books.Where(b => bookIds.Contains(b.Id)).ToList();
                foreach (var book in books)
                {
                    context.Books.Remove(book);
                }

                context.SaveChanges();
            }

            if (domainIds != null && domainIds.Any())
            {
                var domains = context.Domains.Where(d => domainIds.Contains(d.Id)).ToList();
                foreach (var domain in domains)
                {
                    context.Domains.Remove(domain);
                }

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CompleteWorkflow_CreateBookAndLoan_Succeeds()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domainName = "Science_" + Guid.NewGuid().ToString().Substring(0, 8);
                    var domain = new BookDomain { Name = domainName };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domainName);
                    Assert.IsNotNull(createdDomain);
                    domainIds.Add(createdDomain.Id);

                    var isbn = "123456789153";
                    var book = new Book
                    {
                        Title = "Test Book ",
                        ISBN = isbn,
                        TotalCopies = 10,
                        Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { createdDomain.Id });

                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
                    Assert.IsNotNull(createdBook);
                    bookIds.Add(createdBook.Id);

                    var readerEmail = "jane." + Guid.NewGuid().ToString().Substring(0, 8) + "@example.com";
                    var staffEmail = "staff." + Guid.NewGuid().ToString().Substring(0, 8) + "@example.com";

                    var reader = new Reader
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Address = "123 Main St",
                        PhoneNumber = "555-1234",
                        Email = readerEmail,
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Smith",
                        Address = "123 Main Staff",
                        PhoneNumber = "555-123234",
                        Email = staffEmail,
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == readerEmail);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staffEmail);
                    Assert.IsNotNull(createdReader);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var borrowingDate = DateTime.Now.AddDays(-50);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowingDate, 14, createdStaff.Id);

                    var activeBorrowings = services.BorrowingService.GetActiveBorrowings(createdReader.Id);
                    Assert.AreEqual(1, activeBorrowings.Count());
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_ValidRequest_Succeeds()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domainName = "ScienceExt_" + Guid.NewGuid().ToString().Substring(0, 8);
                    var domain = new BookDomain { Name = domainName };
                    services.DomainService.CreateDomain(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domainName);
                    Assert.IsNotNull(createdDomain);
                    domainIds.Add(createdDomain.Id);

                    var isbn = "1234567890123";
                    var book = new Book
                    {
                        Title = "Test Book Ext ",
                        ISBN = isbn,
                        TotalCopies = 10,
                        Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { createdDomain.Id });

                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
                    Assert.IsNotNull(createdBook);
                    bookIds.Add(createdBook.Id);

                    var readerEmail = "janeext." + Guid.NewGuid().ToString().Substring(0, 8) + "@example.com";
                    var staffEmail = "staffext." + Guid.NewGuid().ToString().Substring(0, 8) + "@example.com";

                    var reader = new Reader
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Address = "123 Main St",
                        Email = readerEmail,
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Jane",
                        LastName = "Popa",
                        Address = "123 Main Street",
                        Email = staffEmail,
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == readerEmail);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staffEmail);
                    Assert.IsNotNull(createdReader);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    DateTime borrowingDate = DateTime.Now.AddDays(-60);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowingDate, 14, createdStaff.Id);

                    var borrowing = services.BorrowingRepo.GetActiveBorrowingsByReader(createdReader.Id).FirstOrDefault();
                    Assert.IsNotNull(borrowing, "Împrumutul ar trebui să existe.");

                    int extensionDays = 7;
                    DateTime extensionDate = borrowingDate.AddDays(10);
                    services.BorrowingService.ExtendBorrowing(borrowing.Id, extensionDays, extensionDate);

                    var updatedBorrowing = services.BorrowingRepo.GetById(borrowing.Id);

                    DateTime expectedDueDate = borrowingDate.AddDays(14).AddDays(extensionDays);
                    Assert.AreEqual(expectedDueDate.Date, updatedBorrowing.DueDate.Date);
                    Assert.AreEqual(extensionDays, updatedBorrowing.TotalExtensionDays);
                    Assert.AreEqual(extensionDate.Date, updatedBorrowing.LastExtensionDate.Value.Date);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void StaffReader_HasDoublePrivileges_Succeeds()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain1 = new BookDomain { Name = "Science_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    var domain2 = new BookDomain { Name = "Arts_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain1);
                    context.Domains.Add(domain2);
                    context.SaveChanges();

                    var createdDomain1 = context.Domains.FirstOrDefault(d => d.Name == domain1.Name);
                    var createdDomain2 = context.Domains.FirstOrDefault(d => d.Name == domain2.Name);
                    Assert.IsNotNull(createdDomain1);
                    Assert.IsNotNull(createdDomain2);
                    domainIds.Add(createdDomain1.Id);
                    domainIds.Add(createdDomain2.Id);

                    for (int i = 0; i < 8; i++)
                    {
                        var book = new Book
                        {
                            Title = $"Staff Privilege Book {i}_{Guid.NewGuid().ToString().Substring(0, 5)}",
                            ISBN = "99023" + i.ToString(),
                            TotalCopies = 10,
                            Authors = new List<Author> { new Author { FirstName = "Author", LastName = "Test" } },
                        };

                        var domainId = (i < 4) ? createdDomain1.Id : createdDomain2.Id;
                        services.BookService.CreateBook(book, new List<int> { domainId });

                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                        Assert.IsNotNull(createdBook);
                        bookIds.Add(createdBook.Id);
                    }

                    Assert.AreEqual(8, bookIds.Count, "All 8 books should be created");

                    var staffReaderEmail = "staffreader." + Guid.NewGuid().ToString("N").Substring(0, 10) + "@gmail.ro";
                    var staffStaffEmail = "staffstaff." + Guid.NewGuid().ToString("N").Substring(0, 10) + "@gmail.ro";

                    var staffReader = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Reader",
                        Address = "Mircea cel Batran",
                        Email = staffReaderEmail,
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    var staffStaff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Staff",
                        Address = "Mihai Viteazul",
                        Email = staffStaffEmail,
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(staffReader);
                    services.ReaderRepo.Add(staffStaff);
                    context.SaveChanges();

                    var createdStaffReader = context.Readers.FirstOrDefault(r => r.Email == staffReaderEmail);
                    var createdStaffStaff = context.Readers.FirstOrDefault(r => r.Email == staffStaffEmail);
                    Assert.IsNotNull(createdStaffReader);
                    Assert.IsNotNull(createdStaffStaff);

                    readerIds.Add(createdStaffReader.Id);
                    readerIds.Add(createdStaffStaff.Id);

                    var bookIdsToBorrow = bookIds.Take(6).ToList();
                    DateTime borrowingDate = DateTime.Now.AddDays(-70);

                    services.BorrowingService.CreateBorrowings(createdStaffReader.Id, bookIdsToBorrow, borrowingDate, 14, createdStaffStaff.Id);

                    var activeBorrowings = services.BorrowingService.GetActiveBorrowings(createdStaffReader.Id).ToList();
                    Assert.AreEqual(6, activeBorrowings.Count, "Staff-ul ar fi trebuit să poată împrumuta 6 cărți.");
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookAvailability_AfterLoans_UpdatesCorrectly()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domainName = "AvailTest_";
                    var domain = new BookDomain { Name = domainName };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domainName);
                    Assert.IsNotNull(createdDomain);
                    domainIds.Add(createdDomain.Id);

                    var isbn = "3454345444";
                    var book = new Book
                    {
                        Title = "Availability Test Book " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = isbn,
                        TotalCopies = 10,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { createdDomain.Id });
                    context.SaveChanges();

                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
                    Assert.IsNotNull(createdBook, "Book should be created");
                    bookIds.Add(createdBook.Id);

                    int initialAvailable = createdBook.GetAvailableCopies();
                    Assert.AreEqual(10, initialAvailable);

                    var readerEmail = "jane.avail@" + Guid.NewGuid().ToString().Substring(0, 8) + ".com";
                    var reader = new Reader
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = readerEmail,
                        Address = "123 Main St",
                        PhoneNumber = "555-1234",
                    };

                    var staff = new Reader
                    {
                        FirstName = "Will",
                        LastName = "Smith",
                        Email = readerEmail,
                        Address = "123 Principala",
                        PhoneNumber = "555-12123",
                        IsStaff = true,
                    };
                    services.ReaderService.CreateReader(reader);
                    context.SaveChanges();
                    services.ReaderService.CreateReader(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == readerEmail);
                    Assert.IsNotNull(createdReader);
                    readerIds.Add(createdReader.Id);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-80), 14, staff.Id);
                    context.SaveChanges();

                    var bookAfterLoan = context.Books.Find(createdBook.Id);
                    Assert.IsNotNull(bookAfterLoan);

                    int actualAvailable = bookAfterLoan.GetAvailableCopies();
                    Assert.AreEqual(9, actualAvailable);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void MultipleReaders_BorrowSameBook_AvailabilityDecreases()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();
            var staffIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domainName = "MultiReader_" + Guid.NewGuid().ToString().Substring(0, 5);
                    var domain = new BookDomain { Name = domainName };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domainName);
                    Assert.IsNotNull(createdDomain, "Domain should be created");
                    domainIds.Add(createdDomain.Id);

                    var isbn = "2343231223";
                    var book = new Book
                    {
                        Title = "Multi-Reader Book ",
                        ISBN = isbn,
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { createdDomain.Id });
                    context.SaveChanges();

                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
                    Assert.IsNotNull(createdBook, "Book should be created");
                    bookIds.Add(createdBook.Id);

                    for (int i = 0; i < 3; i++)
                    {
                        var emailReader = $"reader{i}_{Guid.NewGuid().ToString("N").Substring(0, 10)}@test.com";
                        var emailStaff = $"staff{i}_{Guid.NewGuid().ToString("N").Substring(0, 10)}@test.com";

                        var reader = new Reader
                        {
                            FirstName = $"Reader{i}",
                            LastName = "Test",
                            Email = emailReader,
                            Address = $"Address {i}",
                            PhoneNumber = $"555-000{i}",
                            IsStaff = false,
                            RegistrationDate = DateTime.Now,
                        };

                        var staff = new Reader
                        {
                            FirstName = $"Staff{i}",
                            LastName = "Test_staff",
                            Email = emailStaff,
                            Address = $"Address Staff{i}",
                            PhoneNumber = $"555-00001{i}",
                            IsStaff = true,
                            RegistrationDate = DateTime.Now,
                        };

                        services.ReaderRepo.Add(reader);
                        services.ReaderRepo.Add(staff);
                        context.SaveChanges();

                        var createdReaderInDb = context.Readers.FirstOrDefault(r => r.Email == emailReader);
                        var createdStaffInDb = context.Readers.FirstOrDefault(r => r.Email == emailStaff);
                        Assert.IsNotNull(createdReaderInDb);
                        Assert.IsNotNull(createdStaffInDb);

                        readerIds.Add(createdReaderInDb.Id);
                        readerIds.Add(createdStaffInDb.Id);

                        staffIds.Add(createdStaffInDb.Id);
                    }

                    var readers = context.Readers.Where(r => readerIds.Contains(r.Id) && !r.IsStaff).ToList();
                    Assert.AreEqual(3, readers.Count, "All 3 readers should be created");

                    var staffs = context.Readers.Where(r => staffIds.Contains(r.Id)).ToList();
                    Assert.AreEqual(3, staffs.Count, "All 3 staff should be created");

                    services.BorrowingService.CreateBorrowings(readers[0].Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-90), 14, staffs[0].Id);
                    context.SaveChanges();

                    var book1 = context.Books.Find(createdBook.Id);
                    Assert.AreEqual(4, book1.GetAvailableCopies());

                    services.BorrowingService.CreateBorrowings(readers[1].Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-89), 14, staffs[1].Id);
                    context.SaveChanges();

                    var book2 = context.Books.Find(createdBook.Id);
                    Assert.AreEqual(3, book2.GetAvailableCopies());

                    services.BorrowingService.CreateBorrowings(readers[2].Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-88), 14, staffs[2].Id);
                    context.SaveChanges();

                    var book3 = context.Books.Find(createdBook.Id);
                    Assert.AreEqual(2, book3.GetAvailableCopies());
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReturnBook_IncreasesAvailability()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "Return_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domain.Name);
                    Assert.IsNotNull(createdDomain);
                    domainIds.Add(createdDomain.Id);

                    var isbn = "777666551233";
                    var book = new Book
                    {
                        Title = "Return Test Book ",
                        ISBN = isbn,
                        TotalCopies = 3,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "Author", LastName = "Name" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { createdDomain.Id });

                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
                    Assert.IsNotNull(createdBook);
                    bookIds.Add(createdBook.Id);

                    var emailReader = "return@" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".com";
                    var emailStaff = "staff@" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".com";

                    var reader = new Reader
                    {
                        FirstName = "Return",
                        LastName = "Tester",
                        Email = emailReader,
                        Address = "Test Address",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Staff",
                        Email = emailStaff,
                        Address = "Test Address",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == emailReader);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == emailStaff);
                    Assert.IsNotNull(createdReader);
                    Assert.IsNotNull(createdStaff);

                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    services.BorrowingService.CreateBorrowings(
                        createdReader.Id,
                        new List<int> { createdBook.Id },
                        DateTime.Now.AddDays(-100),
                        14,
                        createdStaff.Id);

                    var bookAfterBorrow = context.Books.Find(createdBook.Id);
                    Assert.IsNotNull(bookAfterBorrow);
                    Assert.AreEqual(2, bookAfterBorrow.GetAvailableCopies(), "After borrow, should have 2 copies");

                    var borrowing = context.Borrowings.FirstOrDefault(b => b.BookId == createdBook.Id && b.ReaderId == createdReader.Id);
                    Assert.IsNotNull(borrowing);

                    services.BorrowingService.ReturnBorrowing(borrowing.Id, DateTime.Now.AddDays(-95));

                    var bookAfterReturn = context.Books.Find(createdBook.Id);
                    Assert.IsNotNull(bookAfterReturn);
                    Assert.AreEqual(3, bookAfterReturn.GetAvailableCopies(), "After return, should have 3 copies again");
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_With3BooksFrom1Domain_ThrowsException()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "OneDomain_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domain.Name);
                    domainIds.Add(createdDomain.Id);

                    for (int i = 0; i < 3; i++)
                    {
                        var book = new Book
                        {
                            Title = $"Book {i}",
                            ISBN = "12345432" + i.ToString(),
                            TotalCopies = 10,
                            Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                        };
                        services.BookService.CreateBook(book, new List<int> { createdDomain.Id });
                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                        bookIds.Add(createdBook.Id);
                    }

                    var reader = new Reader
                    {
                        FirstName = "Test",
                        LastName = "Reader",
                        Email = "test@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                    };
                    services.ReaderService.CreateReader(reader);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    readerIds.Add(createdReader.Id);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds, DateTime.Now.AddDays(-110), 14);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_ExceedingDailyLimit()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "Daily_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    for (int i = 0; i < services.Config.MaxBooksPerDay + 2; i++)
                    {
                        var book = new Book
                        {
                            Title = $"DailyBook {i}_{Guid.NewGuid().ToString().Substring(0, 5)}",
                            ISBN = "2345789" + i.ToString(),
                            TotalCopies = 10,
                            Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                        };
                        services.BookService.CreateBook(book, new List<int> { domain.Id });
                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                        bookIds.Add(createdBook.Id);
                    }

                    var reader = new Reader
                    {
                        FirstName = "Daily",
                        LastName = "Reader",
                        Email = "daily@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                        IsStaff = false,
                    };
                    services.ReaderService.CreateReader(reader);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    readerIds.Add(createdReader.Id);

                    var testDate = DateTime.Now.AddDays(-120);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds.Take(services.Config.MaxBooksPerDay).ToList(), testDate, 14);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds.Skip(services.Config.MaxBooksPerDay).Take(1).ToList(), testDate, 14);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_ExceedingPeriodLimit()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "Period_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    for (int i = 0; i < services.Config.MaxBooksPerPeriod + 1; i++)
                    {
                        var book = new Book
                        {
                            Title = $"PeriodBook {i}_{Guid.NewGuid().ToString().Substring(0, 5)}",
                            ISBN = "234543234" + i.ToString(),
                            TotalCopies = 10,
                            Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                        };
                        services.BookService.CreateBook(book, new List<int> { domain.Id });
                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                        bookIds.Add(createdBook.Id);
                    }

                    var reader = new Reader
                    {
                        FirstName = "Period",
                        LastName = "Reader",
                        Email = "period@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                        IsStaff = false,
                    };
                    services.ReaderService.CreateReader(reader);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    readerIds.Add(createdReader.Id);

                    for (int i = 0; i < services.Config.MaxBooksPerPeriod; i++)
                    {
                        services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[i] }, DateTime.Now.AddDays(-130 - i), 14);
                    }

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[services.Config.MaxBooksPerPeriod] }, DateTime.Now.AddDays(-100), 14);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_StaffWithHalvedDelta_CanReBorrowSooner()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "StaffDelta_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Staff Delta Book " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = "56532123",
                        TotalCopies = 10,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Delta",
                        Email = "staffdelta@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                        IsStaff = true,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Dorin",
                        LastName = "Uscoiu",
                        Email = "dorinuscoiu@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test",
                        IsStaff = true,
                    };
                    services.ReaderService.CreateReader(reader);
                    context.SaveChanges();

                    services.ReaderService.CreateReader(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    readerIds.Add(createdReader.Id);

                    var halfDelta = services.Config.MinDaysBetweenBorrows / 2;
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-(halfDelta + 150)), 7, staff.Id);
                    var borrowing = context.Borrowings.FirstOrDefault(b => b.BookId == createdBook.Id);
                    services.BorrowingService.ReturnBorrowing(borrowing.Id, DateTime.Now.AddDays(-(halfDelta + 140)));

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, createdBook.Id);

                    Assert.IsTrue(canBorrow, "Staff should be able to reborrow after halved DELTA period");
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_AllCopiesReadingRoomOnly()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "ReadOnly_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Reading Room Book " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = "14222789",
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 5,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Test",
                        LastName = "Reader",
                        Email = "readonly@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                    };
                    services.ReaderService.CreateReader(reader);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    readerIds.Add(createdReader.Id);

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, createdBook.Id);

                    Assert.IsFalse(canBorrow, "Cannot borrow books with all copies marked as reading room only");
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ExtendBorrowingAdvanced_CallsExtendBorrowingWithCurrentDate()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "AdvExt_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Advanced Extension Book " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = "999888777236",
                        TotalCopies = 10,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                    Assert.IsNotNull(createdBook);
                    bookIds.Add(createdBook.Id);

                    var readerEmail = "advext@" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".com";
                    var staffEmail = "staff.advext@" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".com";

                    var reader = new Reader
                    {
                        FirstName = "Test",
                        LastName = "User",
                        Email = readerEmail,
                        Address = "Test Address",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "User",
                        Email = staffEmail,
                        Address = "Test Add",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == readerEmail);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staffEmail);
                    Assert.IsNotNull(createdReader);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    services.BorrowingService.CreateBorrowings(
                        createdReader.Id,
                        new List<int> { createdBook.Id },
                        DateTime.Now.AddDays(-160),
                        14,
                        createdStaff.Id);

                    var borrowing = context.Borrowings.FirstOrDefault(b => b.BookId == createdBook.Id && b.ReaderId == createdReader.Id);
                    Assert.IsNotNull(borrowing);

                    services.BorrowingService.ExtendBorrowingAdvanced(borrowing.Id, 5);

                    var updatedBorrowing = services.BorrowingRepo.GetById(borrowing.Id);
                    Assert.AreEqual(5, updatedBorrowing.TotalExtensionDays);
                    Assert.IsNotNull(updatedBorrowing.LastExtensionDate);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenBookUnavailable()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "UnavailExt_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Single Copy Book " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = "111222312333",
                        TotalCopies = 1,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader1 = new Reader
                    {
                        FirstName = "Reader1",
                        LastName = "Test",
                        Email = "reader1@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test",
                    };
                    services.ReaderService.CreateReader(reader1);
                    var createdReader1 = context.Readers.FirstOrDefault(r => r.Email == reader1.Email);
                    readerIds.Add(createdReader1.Id);

                    services.BorrowingService.CreateBorrowings(createdReader1.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-170), 14);
                    var borrowing1 = context.Borrowings.FirstOrDefault(b => b.BookId == createdBook.Id);

                    services.BorrowingService.ExtendBorrowing(borrowing1.Id, 5, DateTime.Now);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WhenCanBorrowBookFails()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "CannotBorrow_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Reading Room Only " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = "7778889234399",
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 5,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Test",
                        LastName = "Reader",
                        Email = "cannotborrow@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test",
                    };

                    var staff = new Reader
                    {
                        FirstName = "Test",
                        LastName = "Staff",
                        Email = "staff@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Staff",
                    };

                    services.ReaderService.CreateReader(reader);
                    services.ReaderService.CreateReader(staff);
                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);

                    readerIds.Add(createdStaff.Id);
                    readerIds.Add(createdReader.Id);

                    services.BorrowingService.BorrowBook(createdReader.Id, createdBook.Id, 14, createdStaff.Id);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WhenReaderHasMaxActiveBorrowings()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain1 = new BookDomain { Name = "MaxActive1_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    var domain2 = new BookDomain { Name = "MaxActive2_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain1);
                    context.Domains.Add(domain2);
                    context.SaveChanges();
                    domainIds.Add(domain1.Id);
                    domainIds.Add(domain2.Id);

                    for (int i = 0; i < services.Config.MaxBooksPerPeriod + 1; i++)
                    {
                        var book = new Book
                        {
                            Title = $"MaxActiveBook_{i}",
                            ISBN = "5556667767" + i.ToString(),
                            TotalCopies = 10,
                            Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                        };

                        var domainId = (i % 2 == 0) ? domain1.Id : domain2.Id;
                        services.BookService.CreateBook(book, new List<int> { domainId });

                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                        Assert.IsNotNull(createdBook);
                        bookIds.Add(createdBook.Id);
                    }

                    var reader = new Reader
                    {
                        FirstName = "MaxActive",
                        LastName = "Reader",
                        Email = "maxactive@" + Guid.NewGuid().ToString("N").Substring(0, 12) + ".com",
                        Address = "Test",
                        IsStaff = false,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Member",
                        Email = "staff@" + Guid.NewGuid().ToString("N").Substring(0, 12) + ".com",
                        Address = "Test",
                        IsStaff = true,
                    };

                    services.ReaderService.CreateReader(reader);
                    services.ReaderService.CreateReader(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);
                    Assert.IsNotNull(createdReader);
                    Assert.IsNotNull(createdStaff);

                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    // Use a "safe" borrowing date range (well inside SQL datetime range).
                    // Also keep each borrowing on a different day to avoid daily limit interference.
                    var borrowingDateBase = DateTime.Now.Date.AddDays(-300);

                    for (int i = 0; i < services.Config.MaxBooksPerPeriod; i++)
                    {
                        var borrowingDate = borrowingDateBase.AddDays(i);

                        services.BorrowingService.CreateBorrowings(
                            createdReader.Id,
                            new List<int> { bookIds[i] },
                            borrowingDate,
                            14,
                            createdStaff.Id);
                    }

                    var activeBorrowings = services.BorrowingService.GetActiveBorrowings(createdReader.Id);
                    Assert.AreEqual(services.Config.MaxBooksPerPeriod, activeBorrowings.Count());

                    var canBorrow = services.BorrowingService.CanBorrowBook(
                        createdReader.Id,
                        bookIds[services.Config.MaxBooksPerPeriod]);

                    Assert.IsFalse(canBorrow, "Should not be able to borrow when max active borrowings reached");
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WhenExceedingDomainLimit()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "DomainMax_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    var createdDomain = context.Domains.FirstOrDefault(d => d.Name == domain.Name);
                    Assert.IsNotNull(createdDomain);
                    domainIds.Add(createdDomain.Id);

                    for (int i = 0; i <= services.Config.MaxBooksPerDomain; i++)
                    {
                        var book = new Book
                        {
                            Title = $"DomainMaxBook {i}_{Guid.NewGuid().ToString().Substring(0, 5)}",
                            ISBN = "888999000" + i.ToString().PadLeft(3, '0'),
                            TotalCopies = 10,
                            Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                        };
                        services.BookService.CreateBook(book, new List<int> { createdDomain.Id });
                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                        Assert.IsNotNull(createdBook);
                        bookIds.Add(createdBook.Id);
                    }

                    var readerEmail = "domainmax@" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".com";
                    var staffEmail = "staff@" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".com";

                    var reader = new Reader
                    {
                        FirstName = "DomainMax",
                        LastName = "Reader",
                        Email = readerEmail,
                        Address = "Test",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Staff",
                        Email = staffEmail,
                        Address = "Test",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == readerEmail);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staffEmail);
                    Assert.IsNotNull(createdReader);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var domainLimitDays = services.Config.DomainLimitMonths * 30;
                    for (int i = 0; i < services.Config.MaxBooksPerDomain; i++)
                    {
                        var borrowDate = DateTime.Now.Date.AddDays(-(domainLimitDays / 2) + i);

                        services.BorrowingService.CreateBorrowings(
                            createdReader.Id,
                            new List<int> { bookIds[i] },
                            borrowDate,
                            14,
                            createdStaff.Id);
                    }

                    var activeBorrowings = services.BorrowingService.GetActiveBorrowings(createdReader.Id);
                    Assert.AreEqual(
                        services.Config.MaxBooksPerDomain,
                        activeBorrowings.Count(),
                        $"Should have exactly {services.Config.MaxBooksPerDomain} active borrowings");

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, bookIds[services.Config.MaxBooksPerDomain]);

                    Assert.IsFalse(canBorrow, "Should not be able to borrow when domain limit exceeded");
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WithInvalidBorrowingIdn()
        {
            using (var context = new LibraryDbContext())
            {
                var services = this.CreateIntegrationServices(context);

                services.BorrowingService.ExtendBorrowing(99999, 5, DateTime.Now);
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WithInactiveBorrowing()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    var domain = new BookDomain { Name = "InactiveExt_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Inactive Extension Test " + Guid.NewGuid().ToString().Substring(0, 8),
                        ISBN = "222333232331444",
                        TotalCopies = 10,
                        Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    var createdBook = context.Books.FirstOrDefault(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Inactive",
                        LastName = "Test",
                        Email = "inactive@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test",
                    };
                    services.ReaderService.CreateReader(reader);
                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    readerIds.Add(createdReader.Id);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-220), 14);
                    var borrowing = context.Borrowings.FirstOrDefault(b => b.BookId == createdBook.Id);
                    services.BorrowingService.ReturnBorrowing(borrowing.Id, DateTime.Now.AddDays(-210));

                    services.BorrowingService.ExtendBorrowing(borrowing.Id, 5, DateTime.Now);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StaffCannotGiveMoreThanPERSIMPBooksInOneDay()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    // Arrange config
                    services.Config.MaxBooksStaffPerDay = 3;
                    services.Config.MinAvailablePercentage = 0;

                    // Domain
                    var domainName = "PERSIMP_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    var domain = new BookDomain { Name = domainName };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.First(d => d.Name == domainName);
                    domainIds.Add(createdDomain.Id);

                    // Books: need MaxBooksStaffPerDay + 1
                    for (int i = 0; i < services.Config.MaxBooksStaffPerDay + 1; i++)
                    {
                        var isbn = "1234234323" + i.ToString();
                        var book = new Book
                        {
                            Title = "PERSIMP Book " + i.ToString(),
                            ISBN = isbn,
                            TotalCopies = 20,
                            ReadingRoomOnlyCopies = 0,
                            Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
                        };

                        services.BookService.CreateBook(book, new List<int> { createdDomain.Id });

                        var createdBook = context.Books.FirstOrDefault(b => b.ISBN == isbn);
                        Assert.IsNotNull(createdBook);
                        bookIds.Add(createdBook.Id);
                    }

                    var readerEmail = "reader.persimp." + Guid.NewGuid().ToString("N").Substring(0, 10) + "@test.com";
                    var staffEmail = "staff.persimp." + Guid.NewGuid().ToString("N").Substring(0, 10) + "@test.com";

                    var reader = new Reader
                    {
                        FirstName = "Test",
                        LastName = "Reader",
                        Address = "Addr",
                        Email = readerEmail,
                        PhoneNumber = "0700000000",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Test",
                        LastName = "Staff",
                        Address = "Addr",
                        Email = staffEmail,
                        PhoneNumber = "0700000001",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == readerEmail);
                    var createdStaff = context.Readers.First(r => r.Email == staffEmail);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    // Same day for all operations
                    var day = DateTime.Now.Date.AddDays(-200).AddHours(10);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds.Take(services.Config.MaxBooksStaffPerDay).ToList(), day, 14, createdStaff.Id);

                    // One more on the same day by the same staff => must throw due to PERSIMP
                    services.BorrowingService.CreateBorrowings(
                        createdReader.Id,
                        new List<int> { bookIds[services.Config.MaxBooksStaffPerDay] },
                        day,
                        14,
                        createdStaff.Id);
                }
                finally
                {
                    this.CompleteCleanup(context, bookIds, readerIds, domainIds);
                }
            }
        }
    }
}
