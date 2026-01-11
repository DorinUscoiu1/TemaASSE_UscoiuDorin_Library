// <copyright file="BookTests.cs" company="Transilvania University of Brasov">
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
    /// Unit tests for the Book model class.
    /// </summary>
    [TestClass]
    public class BookTests
    {
        private Book book;

        /// <summary>
        /// Initializes the test by creating a new Book instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.book = new Book();
        }

        /// <summary>
        /// Test.
        /// </summary>s
        [TestMethod]
        public void Book_Creation_ShouldSetProperties()
        {
            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                ISBN = "1234567890",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 2,
            };
            Assert.AreEqual(book.Id, 1);
            Assert.AreEqual(book.Title, "Test Book");
            Assert.AreEqual(book.ISBN, "1234567890");
            Assert.AreEqual(book.TotalCopies, 10);
            Assert.AreEqual(book.ReadingRoomOnlyCopies, 2);
        }

        /// <summary>
        /// Test .
        /// </summary>
        [TestMethod]
        public void Book_GetAvailableCopies_CalculatesCorrectly()
        {
            this.book.Id = 1;
            this.book.Title = "Test Book";
            this.book.TotalCopies = 10;
            this.book.ReadingRoomOnlyCopies = 2;
            var activeBorrowing1 = new Borrowing { Id = 1, ReturnDate = null };
            var activeBorrowing2 = new Borrowing { Id = 2, ReturnDate = null };
            var returnedBorrowing = new Borrowing { Id = 3, ReturnDate = DateTime.Now };
            this.book.BorrowingRecords.Add(activeBorrowing1);
            this.book.BorrowingRecords.Add(activeBorrowing2);
            this.book.BorrowingRecords.Add(returnedBorrowing);
            int availableCopies = this.book.GetAvailableCopies();
            Assert.AreEqual(6, availableCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_WithMultipleAuthorsAndDomains_StoresCorrectly()
        {
            this.book.Id = 1;
            this.book.Title = "Advanced Physics";
            this.book.ISBN = "ISBN-12345";

            var author1 = new Author { Id = 1, FirstName = "Albert", LastName = "Einstein" };
            var author2 = new Author { Id = 2, FirstName = "Niels", LastName = "Bohr" };
            var domain1 = new BookDomain { Id = 1, Name = "Physics" };
            var domain2 = new BookDomain { Id = 2, Name = "Science" };
            this.book.Authors.Add(author1);
            this.book.Authors.Add(author2);
            this.book.Domains.Add(domain1);
            this.book.Domains.Add(domain2);
            Assert.AreEqual(2, this.book.Authors.Count);
            Assert.AreEqual(2, this.book.Domains.Count);
            Assert.IsTrue(this.book.Authors.Any(a => a.FirstName == "Albert"));
            Assert.IsTrue(this.book.Domains.Any(d => d.Name == "Physics"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_GetAvailableCopies_ReturnsZeroWhenNoAvailableCopies()
        {
            this.book.TotalCopies = 5;
            this.book.ReadingRoomOnlyCopies = 2;
            for (int i = 1; i <= 3; i++)
            {
                this.book.BorrowingRecords.Add(new Borrowing { Id = i, ReturnDate = null });
            }

            int availableCopies = this.book.GetAvailableCopies();
            Assert.AreEqual(0, availableCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_WithEditions_StoresEditionsCorrectly()
        {
            this.book.Id = 1;
            this.book.Title = "Programming Languages";
            var edition1 = new Edition { Id = 1, BookId = 1, Publisher = "O'Reilly", Year = 2020 };
            var edition2 = new Edition { Id = 2, BookId = 1, Publisher = "O'Reilly", Year = 2022 };
            this.book.Editions.Add(edition1);
            this.book.Editions.Add(edition2);
            Assert.AreEqual(2, this.book.Editions.Count);
            Assert.IsTrue(this.book.Editions.All(e => e.BookId == this.book.Id));
            Assert.IsTrue(this.book.Editions.Any(e => e.Year == 2020));
            Assert.IsTrue(this.book.Editions.Any(e => e.Year == 2022));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_CanBeLoanable_ReturnsFalseWhenAllReadingRoomOnly()
        {
            this.book.TotalCopies = 5;
            this.book.ReadingRoomOnlyCopies = 5;
            bool canBeLoanable = this.book.CanBeLoanable();
            Assert.IsFalse(canBeLoanable);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_CanBeLoanable_ReturnsTrueWhenLoanableCopiesAvailable()
        {
            this.book.TotalCopies = 10;
            this.book.ReadingRoomOnlyCopies = 2;
            bool canBeLoanable = this.book.CanBeLoanable();
            Assert.IsTrue(canBeLoanable);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_CanBeLoanable_ReturnsFalseWhenBelowThreshold()
        {
            this.book.TotalCopies = 10;
            this.book.ReadingRoomOnlyCopies = 1;
            for (int i = 1; i <= 9; i++)
            {
                this.book.BorrowingRecords.Add(new Borrowing { Id = i, ReturnDate = null });
            }

            bool canBeLoanable = this.book.CanBeLoanable();
            Assert.IsFalse(canBeLoanable);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_GetAvailableCopies_WithNoBorrowings_ReturnsCorrectValue()
        {
            this.book.TotalCopies = 10;
            this.book.ReadingRoomOnlyCopies = 3;
            int availableCopies = this.book.GetAvailableCopies();
            Assert.AreEqual(7, availableCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_DefaultInitialization_HasCorrectDefaults()
        {
            var newBook = new Book();
            Assert.AreEqual(0, newBook.Id);
            Assert.AreEqual(string.Empty, newBook.Title);
            Assert.AreEqual(string.Empty, newBook.Description);
            Assert.AreEqual(string.Empty, newBook.ISBN);
            Assert.AreEqual(0, newBook.TotalCopies);
            Assert.AreEqual(0, newBook.ReadingRoomOnlyCopies);
            Assert.IsNotNull(newBook.Authors);
            Assert.IsNotNull(newBook.Domains);
            Assert.IsNotNull(newBook.Editions);
            Assert.IsNotNull(newBook.BorrowingRecords);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBeLoanable_SingleCopy_ReturnsTrue()
        {
            var book = new Book
            {
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
            };
            Assert.AreEqual(book.CanBeLoanable(), true);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAvailableCopies_ReturnedLoans_NotCounted()
        {
            var book = new Book
            {
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>
                {
                    new Borrowing { ReturnDate = DateTime.Now.AddDays(-1) },
                    new Borrowing { ReturnDate = DateTime.Now.AddDays(-2) },
                },
            };

            Assert.AreEqual(book.GetAvailableCopies(), 10);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Book_ISBN10_IsValid()
        {
            var book = new Book { ISBN = "1234567890" };
            Assert.IsTrue(book.ISBN.Length >= 6 && book.ISBN.Length <= 17);
        }
    }
}
