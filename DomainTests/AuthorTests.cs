// <copyright file="AuthorTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace DomainTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the Author model class.
    /// </summary>
    [TestClass]
    public class AuthorTests
    {
        private Author author;

        /// <summary>
        /// Initializes the test by creating a new Author instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.author = new Author();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_CreateWithBasicInfo_StoresCorrectly()
        {
            this.author.Id = 1;
            this.author.FirstName = "Stephen";
            this.author.LastName = "King";
            Assert.AreEqual(1, this.author.Id);
            Assert.AreEqual("Stephen", this.author.FirstName);
            Assert.AreEqual("King", this.author.LastName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_GetFullName_ReturnsCorrectFormat()
        {
            this.author.FirstName = "George";
            this.author.LastName = "Martin";
            string fullName = this.author.GetFullName();
            Assert.AreEqual("George Martin", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_GetFullName_WithEmptyFirstName_ReturnsLastNameOnly()
        {
            this.author.FirstName = string.Empty;
            this.author.LastName = "Tolkien";
            string fullName = this.author.GetFullName();
            Assert.AreEqual(" Tolkien", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_GetFullName_WithEmptyLastName_ReturnsFirstNameOnly()
        {
            this.author.FirstName = "Isaac";
            this.author.LastName = string.Empty;
            string fullName = this.author.GetFullName();
            Assert.AreEqual("Isaac ", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_WithMultipleBooks_StoresCorrectly()
        {
            this.author.Id = 1;
            this.author.FirstName = "J.K.";
            this.author.LastName = "Rowling";
            var book1 = new Book { Id = 1, Title = "Harry Potter 1" };
            var book2 = new Book { Id = 2, Title = "Harry Potter 2" };
            var book3 = new Book { Id = 3, Title = "Harry Potter 3" };
            this.author.Books.Add(book1);
            this.author.Books.Add(book2);
            this.author.Books.Add(book3);
            Assert.AreEqual(3, this.author.Books.Count);
            Assert.IsTrue(this.author.Books.Contains(book1));
            Assert.IsTrue(this.author.Books.Contains(book2));
            Assert.IsTrue(this.author.Books.Contains(book3));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_DefaultInitialization_HasEmptyCollections()
        {
            var newAuthor = new Author();
            Assert.AreEqual(0, newAuthor.Id);
            Assert.AreEqual(string.Empty, newAuthor.FirstName);
            Assert.AreEqual(string.Empty, newAuthor.LastName);
            Assert.IsNotNull(newAuthor.Books);
            Assert.AreEqual(0, newAuthor.Books.Count);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_WithWhitespaceNames_StoresAsIs()
        {
            this.author.FirstName = "   John   ";
            this.author.LastName = "   Doe   ";
            string fullName = this.author.GetFullName();
            Assert.AreEqual("   John       Doe   ", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_WithSpecialCharacters_StoresCorrectly()
        {
            this.author.FirstName = "Jose!";
            this.author.LastName = "García";
            string fullName = this.author.GetFullName();
            Assert.AreEqual("Jose! García", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_WithLongNames_StoresCorrectly()
        {
            this.author.FirstName = "Christopher";
            this.author.LastName = "SFertgfdedrf23456765432gh";
            string fullName = this.author.GetFullName();
            Assert.AreEqual("Christopher SFertgfdedrf23456765432gh", fullName);
            Assert.IsTrue(fullName.Length > 30);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Author_MultipleAuthorsWithSharedBook_WorksCorrectly()
        {
            var author1 = new Author { Id = 1, FirstName = "Neil", LastName = "Gaiman" };
            var author2 = new Author { Id = 2, FirstName = "Terry", LastName = "Pratchett" };
            var sharedBook = new Book { Id = 1, Title = "Good Omens" };
            author1.Books.Add(sharedBook);
            author2.Books.Add(sharedBook);
            Assert.AreEqual(1, author1.Books.Count);
            Assert.AreEqual(1, author2.Books.Count);
            Assert.AreEqual(sharedBook.Title, author1.Books.First().Title);
            Assert.AreEqual(sharedBook.Title, author2.Books.First().Title);
        }
    }
}
