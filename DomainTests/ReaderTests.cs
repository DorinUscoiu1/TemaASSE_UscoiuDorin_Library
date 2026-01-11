// <copyright file="ReaderTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace DomainTests
{
    using System;
    using System.Linq;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the Reader model class.
    /// </summary>
    [TestClass]
    public class ReaderTests
    {
        private Reader reader;

        /// <summary>
        /// Initializes the test by creating a new Reader instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.reader = new Reader();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_CreateWithBasicInfo_StoresCorrectly()
        {
            this.reader.Id = 1;
            this.reader.FirstName = "John";
            this.reader.LastName = "Doe";
            this.reader.Email = "john@example.com";
            this.reader.Address = "123 Main Street";
            this.reader.PhoneNumber = "555-1234";
            this.reader.IsStaff = false;
            this.reader.RegistrationDate = DateTime.Now;
            Assert.AreEqual(1, this.reader.Id);
            Assert.AreEqual("John", this.reader.FirstName);
            Assert.AreEqual("Doe", this.reader.LastName);
            Assert.AreEqual("john@example.com", this.reader.Email);
            Assert.IsFalse(this.reader.IsStaff);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_StaffVsNonStaff_IsIdentifiedCorrectly()
        {
            var staffReader = new Reader { Id = 1, FirstName = "Alice", IsStaff = true };
            var regularReader = new Reader { Id = 2, FirstName = "Bob", IsStaff = false };
            Assert.IsTrue(staffReader.IsStaff);
            Assert.IsFalse(regularReader.IsStaff);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_BorrowingRecords_AreTrackedCorrectly()
        {
            this.reader.Id = 1;
            this.reader.FirstName = "Jane";
            var borrowing1 = new Borrowing { Id = 1, ReaderId = 1, BookId = 1, IsActive = true, ReturnDate = null };
            var borrowing2 = new Borrowing { Id = 2, ReaderId = 1, BookId = 2, IsActive = true, ReturnDate = null };
            var borrowing3 = new Borrowing { Id = 3, ReaderId = 1, BookId = 3, IsActive = false, ReturnDate = DateTime.Now };
            this.reader.BorrowingRecords.Add(borrowing1);
            this.reader.BorrowingRecords.Add(borrowing2);
            this.reader.BorrowingRecords.Add(borrowing3);
            int activeBorrowings = this.reader.BorrowingRecords.Count(b => b.IsActive);
            Assert.AreEqual(3, this.reader.BorrowingRecords.Count);
            Assert.AreEqual(2, activeBorrowings);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_RegistrationDate_IsTrackedCorrectly()
        {
            var registrationDate = new DateTime(2023, 6, 15);
            this.reader.RegistrationDate = registrationDate;
            Assert.AreEqual(2023, this.reader.RegistrationDate.Year);
            Assert.AreEqual(6, this.reader.RegistrationDate.Month);
            Assert.AreEqual(15, this.reader.RegistrationDate.Day);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_GetFullName_ReturnsCorrectFormat()
        {
            this.reader.FirstName = "John";
            this.reader.LastName = "Doe";
            string fullName = this.reader.GetFullName();
            Assert.AreEqual("John Doe", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_GetFullName_WithEmptyNames_ReturnsEmptyResult()
        {
            this.reader.FirstName = string.Empty;
            this.reader.LastName = string.Empty;
            string fullName = this.reader.GetFullName();
            Assert.AreEqual(" ", fullName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Reader_DefaultInitialization_HasEmptyCollections()
        {
            var newReader = new Reader();
            Assert.IsNotNull(newReader.BorrowingRecords);
            Assert.AreEqual(0, newReader.BorrowingRecords.Count);
            Assert.AreEqual(string.Empty, newReader.FirstName);
            Assert.AreEqual(string.Empty, newReader.LastName);
            Assert.AreEqual(string.Empty, newReader.Address);
            Assert.AreEqual(string.Empty, newReader.PhoneNumber);
            Assert.AreEqual(string.Empty, newReader.Email);
        }
    }
}
