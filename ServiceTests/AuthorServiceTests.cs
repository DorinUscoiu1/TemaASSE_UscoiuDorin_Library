// <copyright file="AuthorServiceTests.cs" company="Transilvania University of Brasov">
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
    /// Unit tests for AuthorService with comprehensive coverage.
    /// </summary>
    [TestClass]
    public class AuthorServiceTests
    {
        private IAuthor mockAuthorRepository;
        private AuthorService authorService;
        private LibraryConfiguration config;
        private LibraryDbContext dbContext;

        /// <summary>
        /// TestInitialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockAuthorRepository = MockRepository.GenerateStub<IAuthor>();
            this.config = new LibraryConfiguration();
            this.authorService = new AuthorService(this.mockAuthorRepository, this.config);
            this.dbContext = new LibraryDbContext();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAllAuthors_WhenCalled_ReturnsAllAuthors()
        {
            var authors = new List<Author>
            {
                new Author { Id = 1, FirstName = "Isaac", LastName = "Newton" },
                new Author { Id = 2, FirstName = "Albert", LastName = "Einstein" },
                new Author { Id = 3, FirstName = "Marie", LastName = "Curie" },
            };

            this.mockAuthorRepository.Stub(x => x.GetAll()).Return(authors);
            var result = this.authorService.GetAllAuthors();
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Any(a => a.FirstName == "Isaac"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAuthorById_WithValidId_ReturnsCorrectAuthor()
        {
            var author = new Author { Id = 1, FirstName = "Stephen", LastName = "Hawking" };
            this.mockAuthorRepository.Stub(x => x.GetById(1)).Return(author);
            var result = this.authorService.GetAuthorById(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Stephen", result.FirstName);
            Assert.AreEqual("Hawking", result.LastName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAuthorById_WithInvalidId()
        {
            this.mockAuthorRepository.Stub(x => x.GetById(999)).Return(null);
            var result = this.authorService.GetAuthorById(999);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAuthor_WithNullAuthor()
        {
            this.authorService.CreateAuthor(null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateAuthor_WithValidData()
        {
            var author = new Author
            {
                FirstName = "Carl",
                LastName = "Sagan",
            };
            this.authorService.CreateAuthor(author);
            this.mockAuthorRepository.AssertWasCalled(x => x.Add(Arg<Author>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateAuthor_WithValidData()
        {
            var author = new Author
            {
                Id = 1,
                FirstName = "Alan",
                LastName = "Turing",
            };
            this.authorService.UpdateAuthor(author);
            this.mockAuthorRepository.AssertWasCalled(x => x.Update(author));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteAuthor_WithValidId()
        {
            this.authorService.DeleteAuthor(1);
            this.mockAuthorRepository.AssertWasCalled(x => x.Delete(1));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksByAuthor_WithValidAuthorId()
        {
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book One" },
                new Book { Id = 2, Title = "Book Two" },
            };

            var author = new Author
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Books = books,
            };

            this.mockAuthorRepository.Stub(x => x.GetById(1)).Return(author);
            var result = this.authorService.GetBooksByAuthor(1);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(b => b.Title == "Book One"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetBooksByAuthor_WithInvalidAuthorId()
        {
            this.mockAuthorRepository.Stub(x => x.GetById(999)).Return(null);
            var result = this.authorService.GetBooksByAuthor(999);
            Assert.AreEqual(0, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAuthorsByFirstName_WithValidName()
        {
            var authors = new List<Author>
            {
                new Author { Id = 1, FirstName = "John", LastName = "Doe" },
                new Author { Id = 2, FirstName = "John", LastName = "Smith" },
            };

            this.mockAuthorRepository.Stub(x => x.GetByFirstName("John")).Return(authors);
            var result = this.authorService.GetAuthorsByFirstName("John");
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(a => a.FirstName == "John"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAuthorsByFirstName_WithEmptyString()
        {
            var result = this.authorService.GetAuthorsByFirstName(string.Empty);
            Assert.AreEqual(0, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAuthorsByLastName_WithValidName()
        {
            var authors = new List<Author>
            {
                new Author { Id = 1, FirstName = "John", LastName = "Doe" },
                new Author { Id = 2, FirstName = "Jane", LastName = "Doe" },
            };

            this.mockAuthorRepository.Stub(x => x.GetByLastName("Doe")).Return(authors);
            var result = this.authorService.GetAuthorsByLastName("Doe");
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(a => a.LastName == "Doe"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void InsertAuthor_IntoDatabase()
        {
                var realRepository = new AuthorDataService(this.dbContext);
                var realService = new AuthorService(realRepository, this.config);
                var author = new Author
                {
                    FirstName = "Donald",
                    LastName = "Knuth",
                };

                realService.CreateAuthor(author);
                var retrievedAuthor = this.dbContext.Authors.FirstOrDefault(a => a.FirstName == "Donald" && a.LastName == "Knuth");

                Assert.IsNotNull(retrievedAuthor, "Author should exist in database");
                Assert.AreEqual("Donald", retrievedAuthor.FirstName);
                Assert.IsTrue(retrievedAuthor.Id > 0, "Author should have an ID");
                this.dbContext.Authors.Remove(retrievedAuthor);
                this.dbContext.SaveChanges();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateAuthor_InDatabase()
        {
                var realRepository = new AuthorDataService(this.dbContext);
                var realService = new AuthorService(realRepository, this.config);

                var author = new Author
                {
                    FirstName = "Richard",
                    LastName = "Feynman",
                };

                realService.CreateAuthor(author);

                var insertedAuthor = this.dbContext.Authors
                    .FirstOrDefault(a => a.FirstName == "Richard" && a.LastName == "Feynman");
                Assert.IsNotNull(insertedAuthor);
                insertedAuthor.FirstName = "Richard P.";
                realService.UpdateAuthor(insertedAuthor);
                var updatedAuthor = this.dbContext.Authors.Find(insertedAuthor.Id);
                Assert.AreEqual("Richard P.", updatedAuthor.FirstName);
                this.dbContext.Authors.Remove(updatedAuthor);
                this.dbContext.SaveChanges();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteAuthor_FromDatabase()
        {
            var realRepository = new AuthorDataService(this.dbContext);
            var realService = new AuthorService(realRepository, this.config);
            var author = new Author
            {
                    FirstName = "Blaise",
                    LastName = "Pascal",
            };

            realService.CreateAuthor(author);
            var insertedAuthor = this.dbContext.Authors.FirstOrDefault(a => a.FirstName == "Blaise" && a.LastName == "Pascal");
            Assert.IsNotNull(insertedAuthor);
            int authorId = insertedAuthor.Id;
            realService.DeleteAuthor(authorId);
            var deletedAuthor = this.dbContext.Authors.Find(authorId);
            Assert.IsNull(deletedAuthor, "Author should be deleted from database");
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateAuthor_WithNullAuthor()
        {
            this.authorService.UpdateAuthor(null);
        }

        /// <summary>
        /// Test: UpdateAuthor throws ValidationException when author is invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void UpdateAuthor_WithInvalidData()
        {
            var author = new Author
            {
                Id = 1,
                FirstName = string.Empty,
                LastName = "ValidLastName",
            };
            this.authorService.UpdateAuthor(author);
        }
    }
}
