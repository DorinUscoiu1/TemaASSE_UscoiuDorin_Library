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
            services.BorrowingService = new BorrowingService(services.BorrowingRepo, services.BookRepo, services.ReaderRepo, services.Config, services.DomainRepo);

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
        public void BookAvailability_AfterLoans()
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

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-100), 14, createdStaff.Id);

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
        public void CreateBorrowings_With3BooksFrom1Domain()
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
                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Member",
                        Email = "staff@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderService.CreateReader(reader);
                    services.ReaderService.CreateReader(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds, DateTime.Now.AddDays(-110), 14, createdStaff.Id);
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

                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Daily",
                        Email = "staff.daily@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderService.CreateReader(reader);
                    services.ReaderService.CreateReader(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var testDate = DateTime.Now.AddDays(-120);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds.Take(services.Config.MaxBooksPerDay).ToList(), testDate, 14, createdStaff.Id);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds.Skip(services.Config.MaxBooksPerDay).Take(1).ToList(), testDate, 14, createdStaff.Id);
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
                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Period",
                        Email = "staff.period@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test Address",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderService.CreateReader(reader);
                    services.ReaderService.CreateReader(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    for (int i = 0; i < services.Config.MaxBooksPerPeriod; i++)
                    {
                        services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[i] }, DateTime.Now.AddDays(-130 - i), 14, createdStaff.Id);
                    }

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[services.Config.MaxBooksPerPeriod] }, DateTime.Now.AddDays(-100), 14, createdStaff.Id);
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
                    var staff = new Reader
                    {
                        FirstName = "Staff",
                        LastName = "Member",
                        Email = "staff.unavail@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderService.CreateReader(reader1);
                    services.ReaderService.CreateReader(staff);
                    var createdReader1 = context.Readers.FirstOrDefault(r => r.Email == reader1.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);
                    Assert.IsNotNull(createdStaff);
                    readerIds.Add(createdReader1.Id);
                    readerIds.Add(createdStaff.Id);

                    services.BorrowingService.CreateBorrowings(createdReader1.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-170), 14, createdStaff.Id);
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_ExceedingLIM_IsRefused()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    services.Config.MaxExtensionDays = 10;

                    var domain = new BookDomain { Name = "LimExceed_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "LIM Test Book",
                        ISBN = "232222222111111",
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "A", LastName = "One" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    context.SaveChanges();

                    var createdBook = context.Books.First(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Lim",
                        LastName = "Reader",
                        Email = "lim.reader." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Lim",
                        LastName = "Staff",
                        Email = "lim.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.First(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var borrowDate = DateTime.Now.AddDays(-20);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowDate, 14, createdStaff.Id);

                    var borrowing = services.BorrowingRepo.GetBorrowingsByBook(createdBook.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .FirstOrDefault();

                    Assert.IsNotNull(borrowing);

                    borrowing.TotalExtensionDays = 8;
                    borrowing.LastExtensionDate = DateTime.Now.AddDays(-10);
                    services.BorrowingRepo.Update(borrowing);

                    services.BorrowingService.ExtendBorrowing(borrowing.Id, 3, DateTime.Now);
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
        public void ExtendBorrowing_CumulativeRecentExtensions_Refused()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    services.Config.MaxExtensionDays = 10;

                    var domain = new BookDomain { Name = "CumulLim_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    for (int i = 0; i < 3; i++)
                    {
                        var b = new Book
                        {
                            Title = $"Cumul Book {i}",
                            ISBN = "34323432323232" + i.ToString(),
                            TotalCopies = 5,
                            ReadingRoomOnlyCopies = 0,
                            Authors = new List<Author> { new Author { FirstName = "X", LastName = "Y" } },
                        };
                        services.BookService.CreateBook(b, new List<int> { domain.Id });
                        context.SaveChanges();
                        var created = context.Books.First(bb => bb.ISBN == b.ISBN);
                        bookIds.Add(created.Id);
                    }

                    var reader = new Reader
                    {
                        FirstName = "Cumul",
                        LastName = "Reader",
                        Email = "cumul.reader." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Cumul",
                        LastName = "Staff",
                        Email = "cumul.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.First(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var recentDate = DateTime.Now.AddDays(-30);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[0] }, recentDate, 14, createdStaff.Id);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[1] }, recentDate, 14, createdStaff.Id);

                    var b0 = services.BorrowingRepo.GetBorrowingsByBook(bookIds[0])
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();
                    var b1 = services.BorrowingRepo.GetBorrowingsByBook(bookIds[1])
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    b0.TotalExtensionDays = 6;
                    b0.LastExtensionDate = DateTime.Now.AddDays(-20);
                    services.BorrowingRepo.Update(b0);

                    b1.TotalExtensionDays = 5;
                    b1.LastExtensionDate = DateTime.Now.AddDays(-10);
                    services.BorrowingRepo.Update(b1);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { bookIds[2] }, recentDate, 14, createdStaff.Id);
                    var b2 = services.BorrowingRepo.GetBorrowingsByBook(bookIds[2])
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    services.BorrowingService.ExtendBorrowing(b2.Id, 1, DateTime.Now);
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
        public void ExtendBorrowing_OldExtensionsIgnored_AllowsExtension()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    services.Config.MaxExtensionDays = 10;

                    var domain = new BookDomain { Name = "OldExtIgnored_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var bookOld = new Book
                    {
                        Title = "Old Ext Book",
                        ISBN = "1234567734654345",
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "O", LastName = "L" } },
                    };
                    var bookNow = new Book
                    {
                        Title = "New Ext Book",
                        ISBN = "3432903432343234",
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "N", LastName = "E" } },
                    };
                    services.BookService.CreateBook(bookOld, new List<int> { domain.Id });
                    services.BookService.CreateBook(bookNow, new List<int> { domain.Id });
                    context.SaveChanges();

                    var createdOld = context.Books.First(b => b.ISBN == bookOld.ISBN);
                    var createdNow = context.Books.First(b => b.ISBN == bookNow.ISBN);
                    bookIds.Add(createdOld.Id);
                    bookIds.Add(createdNow.Id);

                    var reader = new Reader
                    {
                        FirstName = "Old",
                        LastName = "Reader",
                        Email = "old.reader." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Old",
                        LastName = "Staff",
                        Email = "old.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.First(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var oldBorrowDate = DateTime.Now.AddMonths(-4);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdOld.Id }, oldBorrowDate, 14, createdStaff.Id);

                    var oldBorrowing = services.BorrowingRepo.GetBorrowingsByBook(createdOld.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    oldBorrowing.TotalExtensionDays = 10;
                    oldBorrowing.LastExtensionDate = DateTime.Now.AddMonths(-4).AddDays(10);
                    services.BorrowingRepo.Update(oldBorrowing);

                    var recentBorrowDate = DateTime.Now.AddDays(-20);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdNow.Id }, recentBorrowDate, 14, createdStaff.Id);

                    var recentBorrowing = services.BorrowingRepo.GetBorrowingsByBook(createdNow.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    services.BorrowingService.ExtendBorrowing(recentBorrowing.Id, services.Config.MaxExtensionDays, DateTime.Now);

                    var updated = services.BorrowingRepo.GetById(recentBorrowing.Id);
                    Assert.AreEqual(services.Config.MaxExtensionDays, updated.TotalExtensionDays);
                    Assert.IsNotNull(updated.LastExtensionDate);
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
        public void CanBorrowBook_DomainLimitAppliesAcrossHierarchy()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);

                    services.Config.MaxBooksPerDomain = 2;
                    services.Config.DomainLimitMonths = 12;

                    var parent = new BookDomain { Name = "Parent_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(parent);
                    context.SaveChanges();

                    var sub1 = new BookDomain { Name = "SubA_" + Guid.NewGuid().ToString().Substring(0, 5), ParentDomainId = parent.Id };
                    var sub2 = new BookDomain { Name = "SubB_" + Guid.NewGuid().ToString().Substring(0, 5), ParentDomainId = parent.Id };
                    context.Domains.Add(sub1);
                    context.Domains.Add(sub2);
                    context.SaveChanges();

                    var createdParent = context.Domains.FirstOrDefault(d => d.Name == parent.Name);
                    var createdSub1 = context.Domains.FirstOrDefault(d => d.Name == sub1.Name);
                    var createdSub2 = context.Domains.FirstOrDefault(d => d.Name == sub2.Name);
                    Assert.IsNotNull(createdParent);
                    Assert.IsNotNull(createdSub1);
                    Assert.IsNotNull(createdSub2);
                    domainIds.Add(createdParent.Id);
                    domainIds.Add(createdSub1.Id);
                    domainIds.Add(createdSub2.Id);

                    var bookAIsbn = "87654323456432";
                    var bookA = new Book
                    {
                        Title = "Hierarchy Book A",
                        ISBN = bookAIsbn,
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "A", LastName = "One" } },
                    };
                    services.BookService.CreateBook(bookA, new List<int> { createdSub1.Id });

                    var bookBIsbn = "2345676543456";
                    var bookB = new Book
                    {
                        Title = "Hierarchy Book B",
                        ISBN = bookBIsbn,
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "B", LastName = "Two" } },
                    };
                    services.BookService.CreateBook(bookB, new List<int> { createdSub1.Id });

                    var bookCIsbn = "345411345434";
                    var bookC = new Book
                    {
                        Title = "Hierarchy Book C",
                        ISBN = bookCIsbn,
                        TotalCopies = 5,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "C", LastName = "Three" } },
                    };
                    services.BookService.CreateBook(bookC, new List<int> { createdSub2.Id });

                    context.SaveChanges();

                    var createdBookA = context.Books.FirstOrDefault(b => b.ISBN == bookAIsbn);
                    var createdBookB = context.Books.FirstOrDefault(b => b.ISBN == bookBIsbn);
                    var createdBookC = context.Books.FirstOrDefault(b => b.ISBN == bookCIsbn);
                    Assert.IsNotNull(createdBookA);
                    Assert.IsNotNull(createdBookB);
                    Assert.IsNotNull(createdBookC);
                    bookIds.Add(createdBookA.Id);
                    bookIds.Add(createdBookB.Id);
                    bookIds.Add(createdBookC.Id);

                    var readerEmail = "hier.reader." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com";
                    var staffEmail = "hier.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com";

                    var reader = new Reader
                    {
                        FirstName = "Hier",
                        LastName = "Reader",
                        Email = readerEmail,
                        Address = "Addr",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Hier",
                        LastName = "Staff",
                        Email = staffEmail,
                        Address = "Addr",
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

                    var borrowDate = DateTime.Now.AddDays(-10);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBookA.Id }, borrowDate, 14, createdStaff.Id);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBookB.Id }, borrowDate, 14, createdStaff.Id);

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, createdBookC.Id);

                    Assert.IsFalse(canBorrow, "Domain limit should be applied cumulatively across the domain hierarchy.");
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

                    var staff = new Reader
                    {
                        FirstName = "Inactive",
                        LastName = "stfaf",
                        Email = "staff@" + Guid.NewGuid().ToString() + ".com",
                        Address = "Test",
                        IsStaff = true,
                    };

                    services.ReaderService.CreateReader(staff);
                    services.ReaderService.CreateReader(reader);
                    var createdReader = context.Readers.FirstOrDefault(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.FirstOrDefault(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, DateTime.Now.AddDays(-220), 14, createdStaff.Id);
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
        public void CanBorrowBook_NonStaffBlockedDuringDelta()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);
                    var delta = services.Config.MinDaysBetweenBorrows;

                    var domain = new BookDomain { Name = "DeltaNonStaff_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Delta Block Book",
                        ISBN = "94949291395",
                        TotalCopies = 3,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    context.SaveChanges();

                    var createdBook = context.Books.First(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Delta",
                        LastName = "Reader",
                        Email = "delta.ns." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Delta",
                        LastName = "Staff",
                        Email = "delta.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.First(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var borrowDate = DateTime.Now.AddDays(-30);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowDate, 7, createdStaff.Id);

                    var borrowing = services.BorrowingRepo.GetBorrowingsByBook(createdBook.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    var returnDate = DateTime.Now.AddDays(-(delta - 1));
                    services.BorrowingService.ReturnBorrowing(borrowing.Id, returnDate);

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, createdBook.Id);
                    Assert.IsFalse(canBorrow, "Non-staff reader should be blocked from re-borrowing inside DELTA.");
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
        public void CanBorrowBook_AtExactDelta_Allowed()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);
                    var delta = services.Config.MinDaysBetweenBorrows;

                    var domain = new BookDomain { Name = "DeltaExact_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Delta Exact Book",
                        ISBN = "43432121222222",
                        TotalCopies = 3,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "E", LastName = "X" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    context.SaveChanges();

                    var createdBook = context.Books.First(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Delta",
                        LastName = "Exact",
                        Email = "delta.exact." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Delta",
                        LastName = "Staff",
                        Email = "delta.exact.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.First(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var borrowDate = DateTime.Now.AddDays(-30);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowDate, 7, createdStaff.Id);

                    var borrowing = services.BorrowingRepo.GetBorrowingsByBook(createdBook.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    var returnDate = DateTime.Now.AddDays(-delta);
                    services.BorrowingService.ReturnBorrowing(borrowing.Id, returnDate);

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, createdBook.Id);
                    Assert.IsTrue(canBorrow, "Reader should be allowed to re-borrow when days since return == DELTA.");
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
        public void CanBorrowBook_WithMultipleHistoricalBorrowings_LastReturnControls()
        {
            var bookIds = new List<int>();
            var readerIds = new List<int>();
            var domainIds = new List<int>();

            using (var context = new LibraryDbContext())
            {
                try
                {
                    var services = this.CreateIntegrationServices(context);
                    var delta = services.Config.MinDaysBetweenBorrows;

                    var domain = new BookDomain { Name = "DeltaMulti_" + Guid.NewGuid().ToString().Substring(0, 5) };
                    context.Domains.Add(domain);
                    context.SaveChanges();
                    domainIds.Add(domain.Id);

                    var book = new Book
                    {
                        Title = "Delta Multi Book",
                        ISBN = "12328486835566",
                        TotalCopies = 3,
                        ReadingRoomOnlyCopies = 0,
                        Authors = new List<Author> { new Author { FirstName = "M", LastName = "U" } },
                    };
                    services.BookService.CreateBook(book, new List<int> { domain.Id });
                    context.SaveChanges();

                    var createdBook = context.Books.First(b => b.ISBN == book.ISBN);
                    bookIds.Add(createdBook.Id);

                    var reader = new Reader
                    {
                        FirstName = "Multi",
                        LastName = "Reader",
                        Email = "multi.reader." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = false,
                        RegistrationDate = DateTime.Now,
                    };

                    var staff = new Reader
                    {
                        FirstName = "Multi",
                        LastName = "Staff",
                        Email = "multi.staff." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@test.com",
                        IsStaff = true,
                        RegistrationDate = DateTime.Now,
                    };

                    services.ReaderRepo.Add(reader);
                    services.ReaderRepo.Add(staff);
                    context.SaveChanges();

                    var createdReader = context.Readers.First(r => r.Email == reader.Email);
                    var createdStaff = context.Readers.First(r => r.Email == staff.Email);
                    readerIds.Add(createdReader.Id);
                    readerIds.Add(createdStaff.Id);

                    var borrowDate1 = DateTime.Now.AddDays(-60);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowDate1, 7, createdStaff.Id);

                    var b1 = services.BorrowingRepo.GetBorrowingsByBook(createdBook.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderBy(b => b.BorrowingDate)
                        .First();

                    services.BorrowingService.ReturnBorrowing(b1.Id, DateTime.Now.AddDays(-50));

                    var borrowDate2 = DateTime.Now.AddDays(-10);
                    services.BorrowingService.CreateBorrowings(createdReader.Id, new List<int> { createdBook.Id }, borrowDate2, 7, createdStaff.Id);

                    var b2 = services.BorrowingRepo.GetBorrowingsByBook(createdBook.Id)
                        .Where(b => b.ReaderId == createdReader.Id)
                        .OrderByDescending(b => b.BorrowingDate)
                        .First();

                    services.BorrowingService.ReturnBorrowing(b2.Id, DateTime.Now.AddDays(-(delta - 2)));

                    var canBorrow = services.BorrowingService.CanBorrowBook(createdReader.Id, createdBook.Id);
                    Assert.IsFalse(canBorrow, "Most recent historical borrowing return should control DELTA logic.");
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

                    services.Config.MaxBooksStaffPerDay = 3;
                    services.Config.MinAvailablePercentage = 0;

                    var domainName = "PERSIMP_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    var domain = new BookDomain { Name = domainName };
                    context.Domains.Add(domain);
                    context.SaveChanges();

                    var createdDomain = context.Domains.First(d => d.Name == domainName);
                    domainIds.Add(createdDomain.Id);

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

                    var day = DateTime.Now.Date.AddDays(-200).AddHours(10);

                    services.BorrowingService.CreateBorrowings(createdReader.Id, bookIds.Take(services.Config.MaxBooksStaffPerDay).ToList(), day, 14, createdStaff.Id);

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
