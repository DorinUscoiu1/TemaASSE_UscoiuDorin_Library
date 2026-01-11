// <copyright file="EditionTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace DomainTests
{
    using System;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the Edition model class.
    /// </summary>
    [TestClass]
    public class EditionTests
    {
        private Edition edition;

        /// <summary>
        /// Initializes the test by creating a new Edition instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.edition = new Edition();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Edition_CreateWithCompleteInfo_StoresCorrectly()
        {
            this.edition.Id = 1;
            this.edition.BookId = 5;
            this.edition.Publisher = "O'Reilly";
            this.edition.Year = 2022;
            this.edition.EditionNumber = 3;
            this.edition.PageCount = 456;
            this.edition.BookType = "Hardcover";
            Assert.AreEqual(1, this.edition.Id);
            Assert.AreEqual(5, this.edition.BookId);
            Assert.AreEqual("O'Reilly", this.edition.Publisher);
            Assert.AreEqual(2022, this.edition.Year);
            Assert.AreEqual(3, this.edition.EditionNumber);
            Assert.AreEqual(456, this.edition.PageCount);
            Assert.AreEqual("Hardcover", this.edition.BookType);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Edition_WithBook_MaintainsRelationship()
        {
            var book = new Book { Id = 1, Title = "Programming in C#" };
            this.edition.Book = book;
            this.edition.BookId = book.Id;
            Assert.IsNotNull(this.edition.Book);
            Assert.AreEqual(book.Id, this.edition.BookId);
            Assert.AreEqual("Programming in C#", this.edition.Book.Title);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Edition_DefaultInitialization_HasCorrectDefaults()
        {
            var newEdition = new Edition();
            Assert.AreEqual(0, newEdition.Id);
            Assert.AreEqual(0, newEdition.BookId);
            Assert.IsNull(newEdition.Book);
            Assert.AreEqual(string.Empty, newEdition.Publisher);
            Assert.AreEqual(0, newEdition.Year);
            Assert.AreEqual(0, newEdition.EditionNumber);
            Assert.AreEqual(0, newEdition.PageCount);
            Assert.AreEqual(string.Empty, newEdition.BookType);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Edition_MultipleEditions_StoreDistinctValues()
        {
            var edition1 = new Edition { Id = 1, Publisher = "Packt", Year = 2020, EditionNumber = 1, PageCount = 400 };
            var edition2 = new Edition { Id = 2, Publisher = "Microsoft Press", Year = 2023, EditionNumber = 2, PageCount = 550 };
            Assert.AreNotEqual(edition1.Publisher, edition2.Publisher);
            Assert.AreNotEqual(edition1.Year, edition2.Year);
            Assert.AreNotEqual(edition1.PageCount, edition2.PageCount);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Edition_BookReference_CanBeSet()
        {
            var book = new Book { Id = 1 };
            var edition = new Edition { BookId = 1, Book = book };

            Assert.AreEqual(edition.Book, book);
        }
    }
}
