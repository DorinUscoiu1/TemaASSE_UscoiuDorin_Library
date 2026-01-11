// <copyright file="LoanExtensionTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace DomainTests
{
    using System;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the LoanExtension model class.
    /// </summary>
    [TestClass]
    public class LoanExtensionTests
    {
        private LoanExtension loanExtension;

        /// <summary>
        /// Initializes the test by creating a new LoanExtension instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.loanExtension = new LoanExtension();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_CreateWithBasicInfo_StoresCorrectly()
        {
            this.loanExtension.Id = 1;
            this.loanExtension.BorrowingId = 1;
            this.loanExtension.ExtensionDays = 7;
            this.loanExtension.ExtensionDate = DateTime.Now;
            Assert.AreEqual(1, this.loanExtension.Id);
            Assert.AreEqual(1, this.loanExtension.BorrowingId);
            Assert.AreEqual(7, this.loanExtension.ExtensionDays);
            Assert.IsNotNull(this.loanExtension.ExtensionDate);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_WithBorrowingReference_MaintainsCorrectRelationship()
        {
            var borrowing = new Borrowing { Id = 1, BorrowingDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14) };
            this.loanExtension.Borrowing = borrowing;
            this.loanExtension.BorrowingId = 1;
            Assert.IsNotNull(this.loanExtension.Borrowing);
            Assert.AreEqual(1, this.loanExtension.BorrowingId);
            Assert.AreEqual(borrowing.Id, this.loanExtension.Borrowing.Id);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_MultipleExtensionsForSameBorrowing_WorkCorrectly()
        {
            var borrowing = new Borrowing { Id = 1 };
            var extension1 = new LoanExtension { Id = 1, BorrowingId = 1, Borrowing = borrowing, ExtensionDays = 7 };
            var extension2 = new LoanExtension { Id = 2, BorrowingId = 1, Borrowing = borrowing, ExtensionDays = 7 };
            borrowing.Extensions.Add(extension1);
            borrowing.Extensions.Add(extension2);
            Assert.AreEqual(2, borrowing.Extensions.Count);
            Assert.AreEqual(7, extension1.ExtensionDays);
            Assert.AreEqual(7, extension2.ExtensionDays);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_WithVaryingDays_StoresCorrectly()
        {
            var ext1 = new LoanExtension { Id = 1, ExtensionDays = 3 };
            var ext2 = new LoanExtension { Id = 2, ExtensionDays = 7 };
            var ext3 = new LoanExtension { Id = 3, ExtensionDays = 14 };
            var ext4 = new LoanExtension { Id = 4, ExtensionDays = 30 };
            Assert.AreEqual(3, ext1.ExtensionDays);
            Assert.AreEqual(7, ext2.ExtensionDays);
            Assert.AreEqual(14, ext3.ExtensionDays);
            Assert.AreEqual(30, ext4.ExtensionDays);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_ExtensionDate_TracksGrantDate()
        {
            var now = DateTime.Now;
            this.loanExtension.ExtensionDate = now;
            Assert.AreEqual(now.Date, this.loanExtension.ExtensionDate.Date);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_WithPastDate_StoresCorrectly()
        {
            var pastDate = DateTime.Now.AddDays(-7);
            this.loanExtension.ExtensionDate = pastDate;
            Assert.IsTrue(this.loanExtension.ExtensionDate < DateTime.Now);
            Assert.AreEqual(pastDate.Date, this.loanExtension.ExtensionDate.Date);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_DefaultInitialization_HasCorrectDefaults()
        {
            var newExtension = new LoanExtension();
            Assert.AreEqual(0, newExtension.Id);
            Assert.AreEqual(0, newExtension.BorrowingId);
            Assert.AreEqual(0, newExtension.ExtensionDays);
            Assert.IsNull(newExtension.Borrowing);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_MultipleExtensionsDaysAccumulate_CalculatesCorrectly()
        {
            var extension1 = new LoanExtension { Id = 1, ExtensionDays = 7 };
            var extension2 = new LoanExtension { Id = 2, ExtensionDays = 7 };
            var extension3 = new LoanExtension { Id = 3, ExtensionDays = 7 };
            int totalDays = extension1.ExtensionDays + extension2.ExtensionDays + extension3.ExtensionDays;
            Assert.AreEqual(21, totalDays);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_WithZeroDays_StoresCorrectly()
        {
            this.loanExtension.ExtensionDays = 0;
            Assert.AreEqual(0, this.loanExtension.ExtensionDays);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void LoanExtension_WithMaximumDays_StoresCorrectly()
        {
            this.loanExtension.ExtensionDays = 365;
            Assert.AreEqual(365, this.loanExtension.ExtensionDays);
        }
    }
}
