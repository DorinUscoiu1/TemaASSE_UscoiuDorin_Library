// <copyright file="BorrowingTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace DomainTests
{
    using System;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the Borrowing model class.
    /// </summary>
    [TestClass]
    public class BorrowingTests
    {
        private Borrowing borrowing;

        /// <summary>
        /// Initializes the test by creating a new Borrowing instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.borrowing = new Borrowing();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_ActiveBorrowing_CanBeReturned()
        {
            this.borrowing.Id = 1;
            this.borrowing.ReaderId = 1;
            this.borrowing.BookId = 1;
            this.borrowing.BorrowingDate = DateTime.Now;
            this.borrowing.DueDate = DateTime.Now.AddDays(14);
            this.borrowing.IsActive = true;
            this.borrowing.ReturnDate = null;
            this.borrowing.InitialBorrowingDays = 14;
            this.borrowing.ReturnDate = DateTime.Now;
            this.borrowing.IsActive = false;
            Assert.IsFalse(this.borrowing.IsActive);
            Assert.IsNotNull(this.borrowing.ReturnDate);
            Assert.IsTrue(this.borrowing.ReturnDate <= DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_ExtensionTracking_UpdatesCorrectly()
        {
            // Arrange
            this.borrowing.Id = 1;
            this.borrowing.DueDate = DateTime.Now.AddDays(14);
            this.borrowing.TotalExtensionDays = 0;
            this.borrowing.IsActive = true;
            this.borrowing.DueDate = this.borrowing.DueDate.AddDays(7);
            this.borrowing.TotalExtensionDays += 7;
            this.borrowing.LastExtensionDate = DateTime.Now;
            Assert.AreEqual(7, this.borrowing.TotalExtensionDays);
            Assert.IsNotNull(this.borrowing.LastExtensionDate);
            this.borrowing.DueDate = this.borrowing.DueDate.AddDays(7);
            this.borrowing.TotalExtensionDays += 7;
            this.borrowing.LastExtensionDate = DateTime.Now;
            Assert.AreEqual(14, this.borrowing.TotalExtensionDays);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_Overdue_IsIdentifiedCorrectly()
        {
            // Arrange
            this.borrowing.Id = 1;
            this.borrowing.BorrowingDate = DateTime.Now.AddDays(-20);
            this.borrowing.DueDate = DateTime.Now.AddDays(-5);
            this.borrowing.IsActive = true;
            this.borrowing.ReturnDate = null;
            bool isOverdue = this.borrowing.IsActive && this.borrowing.DueDate < DateTime.Now;
            Assert.IsTrue(isOverdue);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_Returned_IsNotOverdue()
        {
            this.borrowing.Id = 1;
            this.borrowing.BorrowingDate = DateTime.Now.AddDays(-10);
            this.borrowing.DueDate = DateTime.Now.AddDays(5);
            this.borrowing.ReturnDate = DateTime.Now;
            this.borrowing.IsActive = false;
            bool isOverdue = this.borrowing.IsActive && this.borrowing.DueDate < DateTime.Now;
            Assert.IsFalse(isOverdue);
            Assert.IsNotNull(this.borrowing.ReturnDate);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_WithReaderAndBook_MaintainsCorrectRelationships()
        {
            var reader = new Reader { Id = 1, FirstName = "John", LastName = "Doe" };
            var book = new Book { Id = 1, Title = "Test Book" };
            this.borrowing.Id = 1;
            this.borrowing.Reader = reader;
            this.borrowing.ReaderId = reader.Id;
            this.borrowing.Book = book;
            this.borrowing.BookId = book.Id;
            this.borrowing.BorrowingDate = DateTime.Now;
            this.borrowing.DueDate = DateTime.Now.AddDays(14);
            this.borrowing.IsActive = true;
            Assert.AreEqual(reader.Id, this.borrowing.ReaderId);
            Assert.AreEqual(book.Id, this.borrowing.BookId);
            Assert.AreEqual(reader.FirstName, this.borrowing.Reader.FirstName);
            Assert.AreEqual(book.Title, this.borrowing.Book.Title);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_GetTotalExtensionDays_ReturnsTotalExtensionDaysWhenSet()
        {
            this.borrowing.TotalExtensionDays = 14;
            int result = this.borrowing.GetTotalExtensionDays();
            Assert.AreEqual(14, result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_GetTotalExtensionDays_ComputesFromExtensionsWhenTotalIsZero()
        {
            this.borrowing.TotalExtensionDays = 0;
            var extension1 = new LoanExtension { Id = 1, ExtensionDays = 7 };
            var extension2 = new LoanExtension { Id = 2, ExtensionDays = 7 };
            this.borrowing.Extensions.Add(extension1);
            this.borrowing.Extensions.Add(extension2);
            int result = this.borrowing.GetTotalExtensionDays();
            Assert.AreEqual(14, result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Borrowing_GetTotalExtensionDays_ReturnsZeroWhenNoExtensions()
        {
            this.borrowing.TotalExtensionDays = 0;
            int result = this.borrowing.GetTotalExtensionDays();
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Test .
        /// </summary>
        [TestMethod]
        public void Borrowing_DefaultInitialization_HasCorrectDefaults()
        {
            var newBorrowing = new Borrowing();
            Assert.AreEqual(0, newBorrowing.Id);
            Assert.AreEqual(0, newBorrowing.ReaderId);
            Assert.AreEqual(0, newBorrowing.BookId);
            Assert.IsTrue(newBorrowing.IsActive);
            Assert.IsNull(newBorrowing.Reader);
            Assert.IsNull(newBorrowing.Book);
            Assert.IsNull(newBorrowing.ReturnDate);
            Assert.IsNotNull(newBorrowing.Extensions);
            Assert.AreEqual(0, newBorrowing.TotalExtensionDays);
        }
    }
}
