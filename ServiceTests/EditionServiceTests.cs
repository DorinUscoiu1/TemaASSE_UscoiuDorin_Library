// <copyright file="EditionServiceTests.cs" company="Transilvania University of Brasov">
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
    using FluentValidation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhino.Mocks;
    using Service;

    /// <summary>
    /// Unit tests for EditionService with comprehensive coverage.
    /// </summary>
    [TestClass]
    public class EditionServiceTests
    {
        private IEdition mockEditionRepository;
        private IBook mockBookRepository;
        private EditionService editionService;

        /// <summary>
        /// Initializes test fixtures before each test with mocked dependencies.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockEditionRepository = MockRepository.GenerateStub<IEdition>();
            this.mockBookRepository = MockRepository.GenerateStub<IBook>();

            this.editionService = new EditionService(
                this.mockEditionRepository,
                this.mockBookRepository);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAllEditions()
        {
            var editions = new List<Edition>
            {
                new Edition { Id = 1, BookId = 1, Publisher = "O'Reilly", Year = 2020 },
                new Edition { Id = 2, BookId = 2, Publisher = "Microsoft Press", Year = 2021 },
                new Edition { Id = 3, BookId = 3, Publisher = "Packt", Year = 2022 },
            };

            this.mockEditionRepository.Stub(x => x.GetAll()).Return(editions);
            var result = this.editionService.GetAllEditions();
            Assert.AreEqual(3, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetEditionById_WithValidId()
        {
            var edition = new Edition
            {
                Id = 1,
                BookId = 1,
                Publisher = "Addison-Wesley",
                Year = 2019,
                EditionNumber = 1,
                PageCount = 500,
            };

            this.mockEditionRepository.Stub(x => x.GetById(1)).Return(edition);
            var result = this.editionService.GetEditionById(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Addison-Wesley", result.Publisher);
            Assert.AreEqual(2019, result.Year);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetEditionById_WithInvalidId()
        {
            this.mockEditionRepository.Stub(x => x.GetById(999)).Return(null);
            var result = this.editionService.GetEditionById(999);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateEdition_WithNullEdition()
        {
            this.editionService.CreateEdition(null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateEdition_WithInvalidBookId()
        {
            var edition = new Edition
            {
                BookId = 999,
                Publisher = "Test Publisher",
                Year = 2023,
                EditionNumber = 1,
                PageCount = 300,
                BookType = "Hardcover",
            };

            this.mockBookRepository.Stub(x => x.GetById(999)).Return(null);
            this.editionService.CreateEdition(edition);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateEdition_WithValidData()
        {
            var book = new Book { Id = 1, Title = "Test Book" };
            var edition = new Edition
            {
                BookId = 1,
                Publisher = "Test Publisher",
                Year = 2023,
                EditionNumber = 1,
                PageCount = 300,
                BookType = "Hardcover",
            };

            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            this.editionService.CreateEdition(edition);
            this.mockEditionRepository.AssertWasCalled(x => x.Add(Arg<Edition>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteEdition_WithValidId()
        {
            this.mockEditionRepository.Stub(x => x.GetById(1)).Return(new Edition { Id = 1, BookId = 1, Publisher = "Test" });
            this.editionService.DeleteEdition(1);
            this.mockEditionRepository.AssertWasCalled(x => x.Delete(1));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetEditionsByBook_WithValidBookId()
        {
            var editions = new List<Edition>
            {
                new Edition { Id = 1, BookId = 1, Publisher = "Publisher A", Year = 2020 },
                new Edition { Id = 2, BookId = 1, Publisher = "Publisher B", Year = 2022 },
            };

            this.mockEditionRepository.Stub(x => x.GetByBookId(1)).Return(editions);
            var result = this.editionService.GetEditionsByBook(1);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(e => e.BookId == 1));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetEditionsByPublisher_WithValidPublisher()
        {
            var editions = new List<Edition>
            {
                new Edition { Id = 1, BookId = 1, Publisher = "O'Reilly", Year = 2020 },
                new Edition { Id = 2, BookId = 2, Publisher = "O'Reilly", Year = 2021 },
            };

            this.mockEditionRepository.Stub(x => x.GetByPublisher("O'Reilly")).Return(editions);
            var result = this.editionService.GetEditionsByPublisher("O'Reilly");
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(e => e.Publisher == "O'Reilly"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void InsertEdition_IntoDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realEditionRepository = new EditionDataService(context);
                var realBookRepository = new BookDataService(context);
                var realService = new EditionService(realEditionRepository, realBookRepository);
                var author = new Author { FirstName = "Test", LastName = "Author" };
                var domain = new BookDomain { Id = 1, Name = "Science" };
                var book = new Book
                {
                    Title = "Test Book",
                    ISBN = "1111111111",
                    TotalCopies = 5,
                    Authors = new List<Author> { author },
                    Domains = new List<BookDomain> { domain },
                };

                var existingBook = context.Books.FirstOrDefault(b => b.ISBN == "1111111111");
                if (existingBook == null)
                {
                    realBookRepository.Add(book);

                    // context.SaveChanges();
                }

                var createdBook = context.Books.FirstOrDefault(b => b.ISBN == "1111111111");

                var edition = new Edition
                {
                    BookId = createdBook.Id,
                    Publisher = "Test Publisher",
                    Year = 2023,
                    EditionNumber = 1,
                    PageCount = 350,
                    BookType = "Paperback",
                };
                realService.CreateEdition(edition);
                var retrievedEdition = context.Editions.FirstOrDefault(e => e.Publisher == "Test Publisher");
                Assert.IsNotNull(retrievedEdition, "Edition should exist in database");
                Assert.AreEqual(createdBook.Id, retrievedEdition.BookId);
                Assert.IsTrue(retrievedEdition.Id > 0, "Edition should have an ID");
                realEditionRepository.Delete(retrievedEdition.Id);
                realBookRepository.Delete(book.Id);
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void EditionService_GetEditionsByBook()
        {
            using (var context = new LibraryDbContext())
            {
                var editionRepo = new EditionDataService(context);
                var bookRepo = new BookDataService(context);
                var service = new EditionService(editionRepo, bookRepo);

                var domain = new BookDomain { Name = "Test_" + Guid.NewGuid().ToString().Substring(0, 5) };
                context.Domains.Add(domain);
                context.SaveChanges();

                var isbn = "ED" + Guid.NewGuid().ToString().Substring(0, 10);
                var book = new Book
                {
                    Title = "Test Book",
                    ISBN = isbn,
                    TotalCopies = 10,
                    Authors = new List<Author> { new Author { FirstName = "Test", LastName = "Author" } },
                };
                context.Books.Add(book);
                book.Domains = new List<BookDomain> { domain };
                context.SaveChanges();

                var edition = new Edition
                {
                    BookId = book.Id,
                    Publisher = "Test Publisher",
                    Year = 2020,
                    EditionNumber = 1,
                };
                editionRepo.Add(edition);

                try
                {
                    var result = service.GetEditionsByBook(book.Id);
                    Assert.AreEqual(1, result.Count());
                }
                finally
                {
                    context.Editions.Remove(edition);
                    context.Books.Remove(book);
                    context.Domains.Remove(domain);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateEdition_InDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realEditionRepository = new EditionDataService(context);
                var realBookRepository = new BookDataService(context);
                var realService = new EditionService(realEditionRepository, realBookRepository);
                var author = new Author { FirstName = "Test", LastName = "Author" };
                var domain = new BookDomain { Id = 1, Name = "Science" };
                var book = new Book
                {
                    Title = "Test Book 2",
                    ISBN = "2222222222",
                    TotalCopies = 5,
                    Authors = new List<Author> { author },
                    Domains = new List<BookDomain> { domain },
                };

                var existingBook = context.Books.FirstOrDefault(b => b.ISBN == "2222222222");
                if (existingBook == null)
                {
                   realBookRepository.Add(book);
                }

                var createdBook = context.Books.FirstOrDefault(b => b.ISBN == "2222222222");

                var edition = new Edition
                {
                    BookId = createdBook.Id,
                    Publisher = "Original Publisher",
                    Year = 2020,
                    EditionNumber = 1,
                    PageCount = 300,
                    BookType = "Hardcover",
                };

                realService.CreateEdition(edition);

                var insertedEdition = context.Editions.FirstOrDefault(e => e.Publisher == "Original Publisher");
                Assert.IsNotNull(insertedEdition);
                insertedEdition.Publisher = "Updated Publisher";
                insertedEdition.Year = 2023;
                realService.UpdateEdition(insertedEdition);

                var updatedEdition = context.Editions.Find(insertedEdition.Id);
                Assert.AreEqual("Updated Publisher", updatedEdition.Publisher);
                Assert.AreEqual(2023, updatedEdition.Year);

                context.Editions.Remove(updatedEdition);
                context.SaveChanges();

                context.Books.Remove(createdBook);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteEdition_FromDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realEditionRepository = new EditionDataService(context);
                var realBookRepository = new BookDataService(context);
                var realService = new EditionService(realEditionRepository, realBookRepository);
                var author = new Author { FirstName = "Test", LastName = "Author" };
                var domain = new BookDomain { Id = 1, Name = "Science" };
                var book = new Book
                {
                    Title = "Test Book 3",
                    ISBN = "3333333333",
                    TotalCopies = 5,
                    Authors = new List<Author> { author },
                    Domains = new List<BookDomain> { domain },
                };

                var existingBook = context.Books.FirstOrDefault(b => b.ISBN == "3333333333");
                if (existingBook == null)
                {
                    context.Books.Add(book);
                    context.SaveChanges();
                }

                var createdBook = context.Books.FirstOrDefault(b => b.ISBN == "3333333333");

                var edition = new Edition
                {
                    BookId = createdBook.Id,
                    Publisher = "Delete Test Publisher",
                    Year = 2020,
                    EditionNumber = 1,
                    PageCount = 300,
                    BookType = "Hardcover",
                };

                realService.CreateEdition(edition);

                var insertedEdition = context.Editions.FirstOrDefault(e => e.Publisher == "Delete Test Publisher");
                Assert.IsNotNull(insertedEdition);
                int editionId = insertedEdition.Id;
                realService.DeleteEdition(editionId);
                var deletedEdition = context.Editions.Find(editionId);
                Assert.IsNull(deletedEdition, "Edition should be deleted from database");
                context.Books.Remove(createdBook);
                context.SaveChanges();
            }
        }
    }
}
