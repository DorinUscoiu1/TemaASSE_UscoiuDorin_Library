// <copyright file="BookServiceTests.cs" company="Transilvania University of Brasov">
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
    using Rhino.Mocks;
    using Rhino.Mocks.Constraints;
    using Service;

    /// <summary>
    /// Unit tests for BookService with Rhino Mocks.
    /// </summary>
    [TestClass]
    public class BookServiceTests
    {
        private IBook mockBookRepository;
        private IBookDomain mockBookDomainRepository;
        private LibraryConfiguration mockConfigRepository;
        private BookService bookService;

        /// <summary>
        /// Initializes test fixtures before each test with mocked dependencies.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockBookRepository = MockRepository.GenerateStub<IBook>();
            this.mockBookDomainRepository = MockRepository.GenerateStub<IBookDomain>();
            this.mockConfigRepository = new LibraryConfiguration();
            this.bookService = new BookService(this.mockBookRepository, this.mockBookDomainRepository, this.mockConfigRepository);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAllBooks_WhenCalled_ReturnsAllBooks()
        {
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book One" },
                new Book { Id = 2, Title = "Book Two" },
                new Book { Id = 3, Title = "Book Three" },
            };

            this.mockBookRepository.Stub(x => x.GetAll()).Return(books);
            var result = this.bookService.GetAllBooks();
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Any(b => b.Title == "Book One"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBookById_WithValidId()
        {
            var book = new Book { Id = 1, Title = "Test Book", ISBN = "123-456" };
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            var result = this.bookService.GetBookById(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Test Book", result.Title);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksByDomain_WithValidDomainId_ReturnsBooks()
        {
            var parentDomain = new BookDomain { Id = 1, Name = "Science" };
            var childDomain = new BookDomain { Id = 2, Name = "Physics", ParentDomainId = 1 };
            var booksInParent = new List<Book>
            {
                new Book { Id = 1, Title = "General Science" },
            };
            var booksInChild = new List<Book>
            {
                new Book { Id = 2, Title = "Physics Basics" },
            };

            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(parentDomain);
            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(1)).Return(new List<BookDomain> { childDomain });
            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(2)).Return(new List<BookDomain>());
            this.mockBookRepository.Stub(x => x.GetBooksByDomain(1)).Return(booksInParent);
            this.mockBookRepository.Stub(x => x.GetBooksByDomain(2)).Return(booksInChild);

            var result = this.bookService.GetBooksByDomain(1);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(b => b.Title == "General Science"));
            Assert.IsTrue(result.Any(b => b.Title == "Physics Basics"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FluentValidation.ValidationException))]
        public void CreateBook_WithNullTitle()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var book = new Book { Id = 1, Title = null, Domains = new List<BookDomain> { domain } };
            this.bookService.CreateBook(book, new List<int> { 1 });
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FluentValidation.ValidationException))]
        public void CreateBook_WithExceededDomains()
        {
            var domainIds = new List<int> { 1, 2, 3, 4 };
            this.bookService.CreateBook(new Book { Title = "Test Book" }, domainIds);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ValidateBookDomains_WithExceededDomains()
        {
            // Arrange
            var domains = new List<BookDomain>
            {
                new BookDomain { Id = 1, Name = "Domain1" },
                new BookDomain { Id = 2, Name = "Domain2" },
                new BookDomain { Id = 3, Name = "Domain3" },
                new BookDomain { Id = 4, Name = "Domain4" },
            };

            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                Domains = domains,
            };
            var result = this.bookService.ValidateBookDomains(book);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ValidateBookDomains_WithAncestorDescendantDomains()
        {
            this.mockConfigRepository.MaxDomainsPerBook = 10;

            var parentDomain = new BookDomain { Id = 1, Name = "Parent" };
            var childDomain = new BookDomain { Id = 2, Name = "Child", ParentDomainId = 1 };
            this.mockBookDomainRepository.Stub(x => x.GetById(2)).Return(childDomain);
            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(parentDomain);
            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                Domains = new List<BookDomain> { parentDomain, childDomain },
            };
            var result = this.bookService.ValidateBookDomains(book);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAvailableCopies_WithValidBook()
        {
            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 2,
                BorrowingRecords = new List<Borrowing>
                {
                    new Borrowing { Id = 1, ReturnDate = null },
                    new Borrowing { Id = 2, ReturnDate = null },
                },
            };
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            var result = this.bookService.GetAvailableCopies(1);
            Assert.AreEqual(6, result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksByDomain_WhenBookIsInLeafDomain()
        {
            var stiinta = new BookDomain { Id = 1, Name = "Stiinta" };
            var informatica = new BookDomain { Id = 2, Name = "Informatica", ParentDomainId = 1 };
            var bazeDeDate = new BookDomain { Id = 3, Name = "Baze de date", ParentDomainId = 2 };

            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(stiinta);
            this.mockBookDomainRepository.Stub(x => x.GetById(2)).Return(informatica);
            this.mockBookDomainRepository.Stub(x => x.GetById(3)).Return(bazeDeDate);

            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(1)).Return(new List<BookDomain> { informatica });
            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(2)).Return(new List<BookDomain> { bazeDeDate });
            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(3)).Return(new List<BookDomain>());

            var leafBook = new Book { Id = 100, Title = "SQL Fundamentals" };

            this.mockBookRepository.Stub(x => x.GetBooksByDomain(1)).Return(new List<Book>());
            this.mockBookRepository.Stub(x => x.GetBooksByDomain(2)).Return(new List<Book>());
            this.mockBookRepository.Stub(x => x.GetBooksByDomain(3)).Return(new List<Book> { leafBook });

            var resultFromStiinta = this.bookService.GetBooksByDomain(1).ToList();
            var resultFromInformatica = this.bookService.GetBooksByDomain(2).ToList();
            Assert.AreEqual(1, resultFromStiinta.Count);
            Assert.AreEqual(1, resultFromInformatica.Count);
            Assert.AreEqual(100, resultFromStiinta[0].Id);
            Assert.AreEqual(100, resultFromInformatica[0].Id);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook()
        {
            var book = new Book
            {
                Id = 1,
                Title = "10 Percent Boundary Book",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
            };

            for (int i = 0; i < 9; i++)
            {
                book.BorrowingRecords.Add(new Borrowing { IsActive = true, ReturnDate = null });
            }

            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            var result = this.bookService.CanBorrowBook(1);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAvailableBooks()
        {
            var availableBooks = new List<Book>
            {
                new Book { Id = 1, Title = "Available Book 1" },
                new Book { Id = 2, Title = "Available Book 2" },
            };

            this.mockBookRepository.Stub(x => x.GetAvailableBooks()).Return(availableBooks);
            var result = this.bookService.GetAvailableBooks();
            Assert.AreEqual(2, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateBook_WithNullDomainIds()
        {
            var book = new Book
            {
                Title = "Valid Book",
                ISBN = "12345432234",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
            };
            this.mockBookRepository.Stub(x => x.GetByISBN(book.ISBN)).Return(null);
            this.bookService.CreateBook(book, null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBook_WithTooManyDomains()
        {
            this.mockConfigRepository.MaxDomainsPerBook = 2;
            var book = new Book
            {
                Title = "Valid Book",
                ISBN = "1234323432",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
            };

            this.mockBookRepository.Stub(x => x.GetByISBN(book.ISBN)).Return(null);
            var domainIds = new List<int> { 1, 2, 3 };
            this.bookService.CreateBook(book, domainIds);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void CreateBook_WhenDomainNotFound()
        {
            var book = new Book
            {
                Title = "Valid Book",
                ISBN = "3456543",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
            };

            this.mockBookRepository.Stub(x => x.GetByISBN(book.ISBN)).Return(null);
            const int missingDomainId = 10;
            try
            {
                this.bookService.CreateBook(book, new List<int> { missingDomainId });
            }
            catch (KeyNotFoundException ex)
            {
                Assert.IsTrue(ex.Message.Contains(missingDomainId.ToString()));
                throw;
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FluentValidation.ValidationException))]
        public void CreateBook_ZeroCopies()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var book = new Book
            {
                Title = "Test Book",
                ISBN = "1234567890",
                TotalCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
                Domains = new List<BookDomain> { domain },
            };
            this.bookService.CreateBook(book, new List<int> { 1 });
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookService_GetAvailableBooks()
        {
            var mockBookRepo = MockRepository.GenerateStub<IBook>();
            var mockDomainRepo = MockRepository.GenerateStub<IBookDomain>();
            var config = new LibraryConfiguration();

            var books = new List<Book>
            {
                new Book { Id = 1, TotalCopies = 10, ReadingRoomOnlyCopies = 0, BorrowingRecords = new List<Borrowing>() },
                new Book { Id = 2, TotalCopies = 5, ReadingRoomOnlyCopies = 5, BorrowingRecords = new List<Borrowing>() },
            };

            mockBookRepo.Stub(x => x.GetAvailableBooks()).Return(books.Where(b => b.GetAvailableCopies() > 0));
            var service = new BookService(mockBookRepo, mockDomainRepo, config);
            var result = service.GetAvailableBooks();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateBook_WithEmptyDomains()
        {
            var book = new Book
            {
                Title = "Test Book",
                ISBN = "1234567890",
                TotalCopies = 5,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
            };
            this.bookService.CreateBook(book, new List<int>());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBook_WithValidData()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var book = new Book
            {
                Title = "Valid Book",
                ISBN = "1234567890",
                TotalCopies = 5,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
                Domains = new List<BookDomain> { domain },
            };

            this.mockBookRepository.Stub(x => x.GetByISBN("1234567890")).Return(null);
            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(domain);
            this.bookService.CreateBook(book, new List<int> { 1 });
            this.mockBookRepository.AssertWasCalled(x => x.Add(Arg<Book>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FluentValidation.ValidationException))]
        public void CreateBook_WithNegativeReadingRoomCopies()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var book = new Book
            {
                Title = "Test Book",
                ISBN = "1234567890",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = -1,
                Domains = new List<BookDomain> { domain },
            };
            this.bookService.CreateBook(book, new List<int> { 1 });
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FluentValidation.ValidationException))]
        public void CreateBook_WithReadingRoomCopiesExceedingTotal()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var book = new Book
            {
                Title = "Test Book",
                ISBN = "1234567890",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 10,
                Domains = new List<BookDomain> { domain },
            };
            this.bookService.CreateBook(book, new List<int> { 1 });
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBook_IntoDatabase_SuccessfullyStored()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var domain = new BookDomain { Name = "Science" };
                context.Domains.Add(domain);
                context.SaveChanges();

                var createdDomain = context.Domains.FirstOrDefault(d => d.Name == "Science");
                Assert.IsNotNull(createdDomain);

                var author = new Author { FirstName = "John", LastName = "Smith" };
                var book = new Book
                {
                    Title = "Integration Test Book",
                    ISBN = "0001001111",
                    Description = "Test book for database integration",
                    TotalCopies = 5,
                    ReadingRoomOnlyCopies = 1,
                    Authors = new List<Author> { author },
                };
                realService.CreateBook(book, new List<int> { createdDomain.Id });
                var retrievedBook = context.Books
                    .FirstOrDefault(b => b.ISBN == "0001001111");

                Assert.IsNotNull(retrievedBook, "Book should exist in database");
                Assert.AreEqual("Integration Test Book", retrievedBook.Title);
                Assert.AreEqual("0001001111", retrievedBook.ISBN);
                Assert.AreEqual(5, retrievedBook.TotalCopies);
                Assert.AreEqual(1, retrievedBook.ReadingRoomOnlyCopies);
                Assert.IsTrue(retrievedBook.Id > 0, "Book should have been assigned an ID");

                context.Books.Remove(retrievedBook);
                context.SaveChanges();
                context.Domains.Remove(createdDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBook_WithMultipleDomains()
        {
            // Arrange
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var domain1 = new BookDomain { Name = "Science" };
                var domain2 = new BookDomain { Name = "Technology" };
                context.Domains.Add(domain1);
                context.Domains.Add(domain2);
                context.SaveChanges();

                var createdDomain1 = context.Domains.FirstOrDefault(d => d.Name == "Science");
                var createdDomain2 = context.Domains.FirstOrDefault(d => d.Name == "Technology");

                var author = new Author { FirstName = "John", LastName = "Smith" };
                var book = new Book
                {
                    Title = "Multi-Domain Test Book",
                    ISBN = "123464",
                    TotalCopies = 3,
                    Authors = new List<Author> { author },
                };

                realService.CreateBook(book, new List<int> { createdDomain1.Id, createdDomain2.Id });

                var retrievedBook = context.Books
                    .FirstOrDefault(b => b.ISBN == "123464");

                Assert.IsNotNull(retrievedBook);
                Assert.AreEqual(2, retrievedBook.Domains.Count, "Book should have 2 domains");
                Assert.IsTrue(retrievedBook.Domains.Any(d => d.Name == "Science"));
                Assert.IsTrue(retrievedBook.Domains.Any(d => d.Name == "Technology"));

                context.Books.Remove(retrievedBook);
                context.SaveChanges();
                context.Domains.Remove(createdDomain1);
                context.Domains.Remove(createdDomain2);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBook_WithAuthors()
        {
            // Arrange
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var domain = new BookDomain { Name = "Science" };
                context.Domains.Add(domain);
                context.SaveChanges();

                var createdDomain = context.Domains.FirstOrDefault(d => d.Name == "Science");

                var author1 = new Author { FirstName = "John", LastName = "Smith" };
                var author2 = new Author { FirstName = "Jane", LastName = "Doe" };

                var book = new Book
                {
                    Title = "Multi-Author Test Book",
                    ISBN = "00011001",
                    TotalCopies = 4,
                    Authors = new List<Author> { author1, author2 },
                };

                // Act
                realService.CreateBook(book, new List<int> { createdDomain.Id });

                // Assert
                var retrievedBook = context.Books
                    .FirstOrDefault(b => b.ISBN == "00011001");

                Assert.IsNotNull(retrievedBook);
                Assert.AreEqual(2, retrievedBook.Authors.Count, "Book should have 2 authors");
                Assert.IsTrue(retrievedBook.Authors.Any(a => a.FirstName == "John"));
                Assert.IsTrue(retrievedBook.Authors.Any(a => a.FirstName == "Jane"));

                context.Books.Remove(retrievedBook);
                context.SaveChanges();
                context.Domains.Remove(createdDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBookById_FromDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var domain = new BookDomain { Name = "Science" };
                context.Domains.Add(domain);
                context.SaveChanges();

                var createdDomain = context.Domains.FirstOrDefault(d => d.Name == "Science");

                var author = new Author { FirstName = "John", LastName = "Smith" };
                var book = new Book
                {
                    Title = "GetById Test Book",
                    ISBN = "123001",
                    TotalCopies = 2,
                    Authors = new List<Author> { author },
                };

                realService.CreateBook(book, new List<int> { createdDomain.Id });

                var insertedBook = context.Books
                    .FirstOrDefault(b => b.ISBN == "123001");
                Assert.IsNotNull(insertedBook);
                int bookId = insertedBook.Id;
                var retrievedBook = realService.GetBookById(bookId);

                Assert.IsNotNull(retrievedBook);
                Assert.AreEqual(bookId, retrievedBook.Id);
                Assert.AreEqual("GetById Test Book", retrievedBook.Title);
                Assert.AreEqual("123001", retrievedBook.ISBN);

                context.Books.Remove(insertedBook);
                context.SaveChanges();
                context.Domains.Remove(createdDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>>
        [TestMethod]
        public void UpdateBook_InDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var domain = new BookDomain { Name = "Science" };
                context.Domains.Add(domain);
                context.SaveChanges();

                var createdDomain = context.Domains.FirstOrDefault(d => d.Name == "Science");

                var author = new Author { FirstName = "John", LastName = "Smith" };
                var book = new Book
                {
                    Title = "Original Title",
                    ISBN = "0987001",
                    Description = "Original description",
                    TotalCopies = 5,
                    Authors = new List<Author> { author },
                };

                realService.CreateBook(book, new List<int> { createdDomain.Id });

                var insertedBook = context.Books
                    .FirstOrDefault(b => b.ISBN == "0987001");
                Assert.IsNotNull(insertedBook);

                insertedBook.Title = "Updated Title";
                insertedBook.Description = "Updated description";
                insertedBook.TotalCopies = 10;
                realService.UpdateBook(insertedBook);
                var updatedBook = context.Books.Find(insertedBook.Id);
                Assert.AreEqual("Updated Title", updatedBook.Title);
                Assert.AreEqual("Updated description", updatedBook.Description);
                Assert.AreEqual(10, updatedBook.TotalCopies);

                context.Books.Remove(updatedBook);
                context.SaveChanges();
                context.Domains.Remove(createdDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteBook_FromDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var domain = new BookDomain { Name = "Science" };
                context.Domains.Add(domain);
                context.SaveChanges();

                var createdDomain = context.Domains.FirstOrDefault(d => d.Name == "Science");

                var author = new Author { FirstName = "John", LastName = "Smith" };
                var book = new Book
                {
                    Title = "Delete Test Book",
                    ISBN = "9876001",
                    TotalCopies = 3,
                    Authors = new List<Author> { author },
                };

                realService.CreateBook(book, new List<int> { createdDomain.Id });

                var insertedBook = context.Books
                    .FirstOrDefault(b => b.ISBN == "9876001");
                Assert.IsNotNull(insertedBook);
                int bookId = insertedBook.Id;
                realService.DeleteBook(bookId);
                var deletedBook = context.Books.Find(bookId);
                Assert.IsNull(deletedBook, "Book should be deleted from database");
                context.Domains.Remove(createdDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksByAuthor_WithValidAuthorId()
        {
            var authorBooks = new List<Book>
            {
                new Book { Id = 1, Title = "Book by Author 1" },
                new Book { Id = 2, Title = "Book by Author 2" },
            };

            this.mockBookRepository.Stub(x => x.GetBooksByAuthor(1)).Return(authorBooks);
            var result = this.bookService.GetBooksByAuthor(1);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(b => b.Title == "Book by Author 1"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksOrderedByAvailability()
        {
            var book1 = new Book { Id = 1, Title = "Book 1", TotalCopies = 10, ReadingRoomOnlyCopies = 0, BorrowingRecords = new List<Borrowing>() };
            var book2 = new Book { Id = 2, Title = "Book 2", TotalCopies = 10, ReadingRoomOnlyCopies = 0, BorrowingRecords = new List<Borrowing>() };

            for (int i = 0; i < 5; i++)
            {
                book2.BorrowingRecords.Add(new Borrowing { ReturnDate = null });
            }

            var availableBooks = new List<Book> { book1, book2 };
            this.mockBookRepository.Stub(x => x.GetAvailableBooks()).Return(availableBooks);
            var result = this.bookService.GetBooksOrderedByAvailability();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Book 1", result.First().Title);
            Assert.AreEqual("Book 2", result.Last().Title);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void IsbnExists()
        {
            var book = new Book { Id = 1, Title = "Test Book", ISBN = "1234567890" };
            this.mockBookRepository.Stub(x => x.GetByISBN("1234567890")).Return(book);
            var result = this.bookService.IsbnExists("1234567890");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void IsbnExists_WithNonExistentISBN()
        {
            this.mockBookRepository.Stub(x => x.GetByISBN("9999999999")).Return(null);
            var result = this.bookService.IsbnExists("9999999999");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void IsbnExists_WithNullOrEmptyISBN()
        {
            Assert.IsFalse(this.bookService.IsbnExists(null));
            Assert.IsFalse(this.bookService.IsbnExists(string.Empty));
            Assert.IsFalse(this.bookService.IsbnExists("   "));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetTotalBooksCount()
        {
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book 1" },
                new Book { Id = 2, Title = "Book 2" },
                new Book { Id = 3, Title = "Book 3" },
                new Book { Id = 4, Title = "Book 4" },
                new Book { Id = 5, Title = "Book 5" },
            };

            this.mockBookRepository.Stub(x => x.GetAll()).Return(books);
            var result = this.bookService.GetTotalBooksCount();
            Assert.AreEqual(5, result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksWithNoCopiesAvailable()
        {
            var book1 = new Book { Id = 1, Title = "Available", TotalCopies = 5, ReadingRoomOnlyCopies = 0, BorrowingRecords = new List<Borrowing>() };
            var book2 = new Book { Id = 2, Title = "Unavailable", TotalCopies = 5, ReadingRoomOnlyCopies = 5, BorrowingRecords = new List<Borrowing>() };
            var books = new List<Book> { book1, book2 };
            this.mockBookRepository.Stub(x => x.GetAll()).Return(books);
            var result = this.bookService.GetBooksWithNoCopiesAvailable();
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.Any(b => b.Title == "Unavailable"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetReadingRoomOnlyBooks()
        {
            var book1 = new Book { Id = 1, Title = "Regular Book", TotalCopies = 5, ReadingRoomOnlyCopies = 2 };
            var book2 = new Book { Id = 2, Title = "Reading Room Only", TotalCopies = 3, ReadingRoomOnlyCopies = 3 };

            var books = new List<Book> { book1, book2 };
            this.mockBookRepository.Stub(x => x.GetAll()).Return(books);
            var result = this.bookService.GetReadingRoomOnlyBooks();
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.Any(b => b.Title == "Reading Room Only"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WithAllReadingRoomOnly()
        {
            var book = new Book
            {
                Id = 1,
                Title = "Reading Room Book",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 10,
                BorrowingRecords = new List<Borrowing>(),
            };

            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            var result = this.bookService.CanBorrowBook(1);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBook_WithDuplicateISBN()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var existingBook = new Book { Id = 1, ISBN = "1234567890" };
            var newBook = new Book
            {
                Title = "New Book",
                ISBN = "1234567890",
                TotalCopies = 5,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
            };

            this.mockBookRepository.Stub(x => x.GetByISBN("1234567890")).Return(existingBook);
            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(domain);
            this.bookService.CreateBook(newBook, new List<int> { 1 });
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBook_WithAncestorDescendantDomains()
        {
            var parentDomain = new BookDomain { Id = 1, Name = "Science" };
            var childDomain = new BookDomain { Id = 2, Name = "Physics", ParentDomainId = 1 };

            var book = new Book
            {
                Title = "Test Book",
                ISBN = "1234567890",
                TotalCopies = 5,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
            };

            this.mockBookRepository.Stub(x => x.GetByISBN("1234567890")).Return(null);
            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(parentDomain);
            this.mockBookDomainRepository.Stub(x => x.GetById(2)).Return(childDomain);
            this.bookService.CreateBook(book, new List<int> { 1, 2 });
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksInDomain_WithSubdomains_IncludesAllBooks()
        {
            var parentDomain = new BookDomain { Id = 1, Name = "Science" };
            var childDomain = new BookDomain { Id = 2, Name = "Physics", ParentDomainId = 1 };

            var booksInParent = new List<Book> { new Book { Id = 1, Title = "Science Book" } };
            var booksInChild = new List<Book> { new Book { Id = 2, Title = "Physics Book" } };

            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(parentDomain);
            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(1)).Return(new List<BookDomain> { childDomain });
            this.mockBookDomainRepository.Stub(x => x.GetSubdomains(2)).Return(new List<BookDomain>());

            this.mockBookRepository.Stub(x => x.GetBooksByDomain(1)).Return(booksInParent);
            this.mockBookRepository.Stub(x => x.GetBooksByDomain(2)).Return(booksInChild);

            var result = this.bookService.GetBooksInDomain(1);

            Assert.AreEqual(2, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksDirectlyInDomain()
        {
            var domain = new BookDomain { Id = 1, Name = "Science" };
            var books = new List<Book> { new Book { Id = 1, Title = "Science Book" } };
            this.mockBookDomainRepository.Stub(x => x.GetById(1)).Return(domain);
            this.mockBookRepository.Stub(x => x.GetBooksByDomain(1)).Return(books);
            var result = this.bookService.GetBooksDirectlyInDomain(1);
            Assert.AreEqual(1, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksInDomain_Database_IncludesSubdomainBooks()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDataService(context);
                var realDomainRepository = new BookDomainDataService(context);
                var realService = new BookService(realRepository, realDomainRepository, this.mockConfigRepository);

                var parentDomain = new BookDomain { Name = "Science_Parent_" + Guid.NewGuid().ToString().Substring(0, 5) };
                var childDomain = new BookDomain { Name = "Physics_Child_" + Guid.NewGuid().ToString().Substring(0, 5), ParentDomainId = 0 };
                context.Domains.Add(parentDomain);
                context.SaveChanges();

                var createdParent = context.Domains.FirstOrDefault(d => d.Name == parentDomain.Name);
                Assert.IsNotNull(createdParent);

                childDomain.ParentDomainId = createdParent.Id;
                context.Domains.Add(childDomain);
                context.SaveChanges();

                var createdChild = context.Domains.FirstOrDefault(d => d.Name == childDomain.Name);
                Assert.IsNotNull(createdChild);

                var book1 = new Book
                {
                    Title = "Parent Domain Book",
                    ISBN = "555555",
                    TotalCopies = 5,
                    Authors = new List<Author> { new Author { FirstName = "Author", LastName = "One" } },
                };
                realService.CreateBook(book1, new List<int> { createdParent.Id });

                var book2 = new Book
                {
                    Title = "Child Domain Book",
                    ISBN = "666666",
                    TotalCopies = 5,
                    Authors = new List<Author> { new Author { FirstName = "Author", LastName = "Two" } },
                };
                realService.CreateBook(book2, new List<int> { createdChild.Id });
                var result = realService.GetBooksInDomain(createdParent.Id);

                Assert.AreEqual(2, result.Count(), "Should include books from both parent and child domains");
                var createdBooks = context.Books.Where(b => b.Title == "Parent Domain Book" || b.Title == "Child Domain Book").ToList();
                foreach (var b in createdBooks)
                {
                    context.Books.Remove(b);
                }

                context.SaveChanges();

                context.Domains.Remove(createdChild);
                context.Domains.Remove(createdParent);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateBook_WithNullBook()
        {
            this.bookService.UpdateBook(null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FluentValidation.ValidationException))]
        public void UpdateBook_WithInvalidBook()
        {
            var invalidBook = new Book
            {
                Title = null,
                ISBN = null,
            };
            this.bookService.UpdateBook(invalidBook);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateBook_WithInvalidDomains()
        {
            var validButDomainInvalidBook = new Book
            {
                Id = 1,
                Title = "Valid Title",
                ISBN = "134323432",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
                Domains = null,
            };
            this.bookService.UpdateBook(validButDomainInvalidBook);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateBook_WithValidBook()
        {
            var domain = new BookDomain { Id = 10, Name = "TestDomain" };

            var validBook = new Book
            {
                Id = 1,
                Title = "Valid Title",
                ISBN = "12343234",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "John", LastName = "Doe" } },
                Domains = new List<BookDomain> { domain },
            };
            this.mockBookDomainRepository.Stub(x => x.GetById(Arg<int>.Is.Anything)).Return(domain);
            this.bookService.UpdateBook(validBook);
            this.mockBookRepository.AssertWasCalled(x => x.Update(Arg<Book>.Is.Same(validBook)));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WhenBookNotFound()
        {
            this.mockBookRepository.Stub(x => x.GetById(123)).Return(null);
            var result = this.bookService.CanBorrowBook(123);
            Assert.IsFalse(result);
        }
    }
}
