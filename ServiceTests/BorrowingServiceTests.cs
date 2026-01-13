// <copyright file="BorrowingServiceTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace ServiceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data.Repositories;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhino.Mocks;
    using Service;

    /// <summary>
    /// Unit tests for BorrowingService with Rhino Mocks.
    /// </summary>
    [TestClass]
    public class BorrowingServiceTests
    {
        private const int DefaultStaffId = 2;

        private IBorrowing mockBorrowingRepository;
        private IBook mockBookRepository;
        private IReader mockReaderRepository;
        private LibraryConfiguration mockConfigRepository;
        private BorrowingService borrowingService;
        private IBookDomain mockBookDomainRepository;

        /// <summary>
        /// Initializes test fixtures before each test with mocked dependencies.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockBorrowingRepository = MockRepository.GenerateStub<IBorrowing>();
            this.mockBookRepository = MockRepository.GenerateStub<IBook>();
            this.mockReaderRepository = MockRepository.GenerateStub<IReader>();
            this.mockConfigRepository = new LibraryConfiguration();
            this.mockBookDomainRepository = MockRepository.GenerateStub<IBookDomain>();

            this.borrowingService = new BorrowingService(this.mockBorrowingRepository, this.mockBookRepository, this.mockReaderRepository, this.mockConfigRepository, this.mockBookDomainRepository);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WithInvalidReader()
        {
            this.mockReaderRepository.Stub(x => x.GetById(999)).Return(null);
            var staff = new Reader { Id = 2, IsStaff = true };
            this.mockBookRepository.Stub(x => x.GetById(1))
                .Return(new Book { Id = 1, Title = "Test Book" });
            this.borrowingService.BorrowBook(999, 1, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WithInvalidStaff_WhenStaffNotFound()
        {
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var reader = new Reader { Id = 1, IsStaff = false };
            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(null);
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());
            this.borrowingService.BorrowBook(1, 1, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WithInvalidBook()
        {
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(new Reader { Id = 1, FirstName = "John", LastName = "Doe" });
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(new Reader { Id = 1, FirstName = "John", LastName = "Doe", IsStaff = true, });
            this.mockBookRepository.Stub(x => x.GetById(999)).Return(null);
            this.borrowingService.BorrowBook(1, 999, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetActiveBorrowings_WithValidReaderId()
        {
            var activeBorrowings = new List<Borrowing>
            {
                new Borrowing { Id = 1, ReaderId = 1, IsActive = true },
                new Borrowing { Id = 2, ReaderId = 1, IsActive = true },
            };
            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(activeBorrowings);
            var result = this.borrowingService.GetActiveBorrowings(1);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(b => b.IsActive));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetOverdueBorrowings_WhenCalled()
        {
            var overdueBorrowings = new List<Borrowing>
            {
                new Borrowing
                {
                    Id = 1,
                    DueDate = DateTime.Now.AddDays(-5),
                    IsActive = true,
                },
                new Borrowing
                {
                    Id = 2,
                    DueDate = DateTime.Now.AddDays(-3),
                    IsActive = true,
                },
            };
            this.mockBorrowingRepository.Stub(x => x.GetOverdueBorrowings()).Return(overdueBorrowings);
            var result = this.borrowingService.GetOverdueBorrowings();
            Assert.AreEqual(2, result.Count());
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WithExceededExtensionDays()
        {
            var borrowing = new Borrowing
            {
                Id = 1,
                DueDate = DateTime.Now.AddDays(5),
                TotalExtensionDays = 5,
            };
            this.borrowingService.ExtendBorrowing(1, 5, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BorrowBook_WithValidData()
        {
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" }, },
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(10)).Return(new List<Borrowing>());
            this.borrowingService.BorrowBook(1, 10, 14, 2);
            this.mockBorrowingRepository.AssertWasCalled(x => x.Add(Arg<Borrowing>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateBorrowings_WhenBookIdsIsNull_ThrowsArgumentNullException()
        {
            var staff = new Reader { Id = 2, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(new Reader { Id = 1, IsStaff = false });
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.borrowingService.CreateBorrowings(1, null, DateTime.Now, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenReaderNotFound_ThrowsInvalidOperationException()
        {
            this.mockReaderRepository.Stub(x => x.GetById(999)).Return(null);
            this.borrowingService.CreateBorrowings(999, new List<int> { 1 }, DateTime.Now, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenStaffNotFound_ThrowsInvalidOperationException()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(null);
            this.borrowingService.CreateBorrowings(1, new List<int> { 1 }, DateTime.Now, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenStaffIsNotMarkedAsStaff_ThrowsInvalidOperationException()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = false };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.borrowingService.CreateBorrowings(1, new List<int> { 1 }, DateTime.Now, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingStaffDailyLimit_ForNonStaffReader_ThrowsInvalidOperationException()
        {
            this.mockConfigRepository.MaxBooksStaffPerDay = 3;

            var borrowDate = DateTime.Now.AddDays(-5);
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            var staffBorrowingsToday = new List<Borrowing>
            {
                new Borrowing { Id = 1, StaffId = 2, BorrowingDate = borrowDate, IsActive = true },
                new Borrowing { Id = 2, StaffId = 2, BorrowingDate = borrowDate, IsActive = true },
                new Borrowing { Id = 3, StaffId = 2, BorrowingDate = borrowDate, IsActive = true },
            };

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate)).Return(staffBorrowingsToday);
            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingMaxBooksPerRequest_ThrowsInvalidOperationException()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            var bookIds = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerRequest + 1).ToList();

            this.borrowingService.CreateBorrowings(1, bookIds, DateTime.Now.AddDays(-10), 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenAnyBookNotFound_InDomainDiversityBranch_ThrowsInvalidOperationException()
        {
            // Arrange
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(null);

            this.borrowingService.CreateBorrowings(1, new List<int> { 1, 2, 3 }, DateTime.Now.AddDays(-10), 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenDomainDiversityRuleViolated_ThrowsInvalidOperationException()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            var domain = new BookDomain { Id = 1, Name = "D1" };

            this.mockBookRepository.Stub(x => x.GetById(1)).Return(new Book { Id = 1, TotalCopies = 10, BorrowingRecords = new List<Borrowing>(), Domains = new List<BookDomain> { domain } });
            this.mockBookRepository.Stub(x => x.GetById(2)).Return(new Book { Id = 2, TotalCopies = 10, BorrowingRecords = new List<Borrowing>(), Domains = new List<BookDomain> { domain } });
            this.mockBookRepository.Stub(x => x.GetById(3)).Return(new Book { Id = 3, TotalCopies = 10, BorrowingRecords = new List<Borrowing>(), Domains = new List<BookDomain> { domain } });

            this.borrowingService.CreateBorrowings(1, new List<int> { 1, 2, 3 }, DateTime.Now.AddDays(-10), 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingDailyLimit_ForNonStaffReader_ThrowsInvalidOperationException()
        {
            var borrowDate = DateTime.Now.AddDays(-5);

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(new List<Borrowing>());

            var todayBorrowings = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerDay).Select(i => new Borrowing { Id = i, ReaderId = 1, BorrowingDate = borrowDate }).ToList();
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate)).Return(todayBorrowings);

            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingPeriodLimit_ForNonStaffReader_ThrowsInvalidOperationException()
        {
            var borrowDate = DateTime.Now.AddDays(-5);

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate))
                .Return(new List<Borrowing>());
            var periodBorrowings = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerPeriod).Select(i => new Borrowing { Id = i, ReaderId = 1, BorrowingDate = borrowDate.AddDays(-1) }).ToList();

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(periodBorrowings);

            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenCanBorrowBookFails_ThrowsInvalidOperationException()
        {
            var borrowDate = DateTime.Now.AddDays(-5);

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.mockBookRepository.Stub(x => x.GetById(10)).Return(null);
            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenRepositoryAddThrows_ThrowsInvalidOperationException()
        {
            var borrowDate = DateTime.Now.AddDays(-5);

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.mockConfigRepository.MinAvailablePercentage = 0;

            var domain = new BookDomain { Id = 1, Name = "D1" };
            this.mockBookRepository.Stub(x => x.GetById(10))
                .Return(new Book
                {
                    Id = 10,
                    TotalCopies = 10,
                    ReadingRoomOnlyCopies = 0,
                    BorrowingRecords = new List<Borrowing>(),
                    Domains = new List<BookDomain> { domain },
                });

            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(10)).Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.Add(Arg<Borrowing>.Is.Anything)).Throw(new Exception("DB down"));
            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WhenRepositoryAddThrows()
        {
            this.mockConfigRepository.MaxBooksStaffPerDay = 100;

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(10)).Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.Add(Arg<Borrowing>.Is.Anything))
                .Throw(new Exception("DB down"));

            try
            {
                this.borrowingService.BorrowBook(1, 10, 14, 2);
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to create borrowing record"));
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual("DB down", ex.InnerException.Message);
                throw;
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WithNoAvailableCopies()
        {
            var reader = new Reader
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsStaff = false,
            };

            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 5,
                BorrowingRecords = new List<Borrowing>(),
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            var result = this.borrowingService.CanBorrowBook(1, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WhenAvailabilityIsExactlyMinPercentage()
        {
            this.mockConfigRepository.MinAvailablePercentage = 0.1;
            var reader = new Reader
            {
                Id = 1,
                IsStaff = false,
            };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            for (int i = 0; i < 9; i++)
            {
                book.BorrowingRecords.Add(new Borrowing { IsActive = true, ReturnDate = null });
            }

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);
            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(10)).Return(new List<Borrowing>());

            var result = this.borrowingService.CanBorrowBook(1, 10);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WhenAvailabilityBelowMinPercentage_ReturnsFalse()
        {
            this.mockConfigRepository.MinAvailablePercentage = 0.1;

            var reader = new Reader
            {
                Id = 1,
                IsStaff = false,
            };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            for (int i = 0; i < 10; i++)
            {
                book.BorrowingRecords.Add(new Borrowing { IsActive = true, ReturnDate = null });
            }

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);
            var result = this.borrowingService.CanBorrowBook(1, 10);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_NonStaffWhenInsideDelta()
        {
            this.mockConfigRepository.MinAvailablePercentage = 0;
            this.mockConfigRepository.MinDaysBetweenBorrows = 10;

            var reader = new Reader
            {
                Id = 1,
                IsStaff = false,
            };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);

            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(new List<Borrowing>());
            var previousBorrowings = new List<Borrowing>
            {
                new Borrowing
                {
                    ReaderId = 1,
                    BookId = 10,
                    BorrowingDate = DateTime.Now.AddDays(-20),
                    ReturnDate = DateTime.Now.AddDays(-5),
                },
            };
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(10)).Return(previousBorrowings);
            var result = this.borrowingService.CanBorrowBook(1, 10);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetActiveBorrowingCount_WithValidReaderId()
        {
            var activeBorrowings = new List<Borrowing>
            {
                new Borrowing { Id = 1, ReaderId = 1, IsActive = true },
                new Borrowing { Id = 2, ReaderId = 1, IsActive = true },
                new Borrowing { Id = 3, ReaderId = 1, IsActive = true },
            };

            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(activeBorrowings);
            var result = this.borrowingService.GetActiveBorrowingCount(1);
            Assert.AreEqual(3, result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CanBorrowBook_WithAllConditionsMet()
        {
            var reader = new Reader
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsStaff = false,
            };
            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 2,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain>(),
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(book);
            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(1)).Return(new List<Borrowing>());
            var result = this.borrowingService.CanBorrowBook(1, 1);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReturnBorrowing_WithAlreadyReturnedBorrowing()
        {
            var borrowing = new Borrowing
            {
                Id = 1,
                IsActive = false,
                ReturnDate = DateTime.Now.AddDays(-5),
            };
            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.borrowingService.ReturnBorrowing(1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WithInvalidReader()
        {
            this.mockReaderRepository.Stub(x => x.GetById(999)).Return(null);
            this.borrowingService.CreateBorrowings(999, new List<int> { 1 }, DateTime.Now, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingMaxBooksPerRequest_NonStaff()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = DefaultStaffId, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(DefaultStaffId)).Return(staff);
            var bookIds = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerRequest + 1).ToList();
            this.borrowingService.CreateBorrowings(1, bookIds, DateTime.Now, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBorrowings_WhenExceedingMaxBooksPerRequest_Staff_AllowsDoubleLimit()
        {
            var reader = new Reader { Id = 1, IsStaff = true };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(reader.Id)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(staff.Id)).Return(staff);

            var bookIds = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerRequest * 2).ToList();

            var domain1 = new BookDomain { Id = 1, Name = "D1" };
            var domain2 = new BookDomain { Id = 2, Name = "D2" };

            foreach (var bookId in bookIds)
            {
                var domain = (bookId % 2 == 0) ? domain1 : domain2;

                this.mockBookRepository.Stub(x => x.GetById(bookId))
                    .Return(new Book
                    {
                        Id = bookId,
                        TotalCopies = 10,
                        ReadingRoomOnlyCopies = 0,
                        BorrowingRecords = new List<Borrowing>(),
                        Domains = new List<BookDomain> { domain },
                    });
            }

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(reader.Id)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(Arg<int>.Is.Anything)).Return(new List<Borrowing>());

            this.borrowingService.CreateBorrowings(reader.Id, bookIds, DateTime.Now.AddDays(-10), 14, staff.Id);
            this.mockBorrowingRepository.AssertWasCalled(
                x => x.Add(Arg<Borrowing>.Is.Anything),
                opt => opt.Repeat.Times(bookIds.Count));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_With3Books_WhenAnyBookNotFound()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = DefaultStaffId, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(DefaultStaffId)).Return(staff);
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(null);
            this.borrowingService.CreateBorrowings(1, new List<int> { 1, 2, 3 }, DateTime.Now, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_With3BooksFromSingleDomain()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = DefaultStaffId, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(DefaultStaffId)).Return(staff);

            var domain = new BookDomain { Id = 10, Name = "D1" };
            this.mockBookRepository.Stub(x => x.GetById(1)).Return(new Book { Id = 1, TotalCopies = 10, Domains = new List<BookDomain> { domain }, BorrowingRecords = new List<Borrowing>() });
            this.mockBookRepository.Stub(x => x.GetById(2)).Return(new Book { Id = 2, TotalCopies = 10, Domains = new List<BookDomain> { domain }, BorrowingRecords = new List<Borrowing>() });
            this.mockBookRepository.Stub(x => x.GetById(3)).Return(new Book { Id = 3, TotalCopies = 10, Domains = new List<BookDomain> { domain }, BorrowingRecords = new List<Borrowing>() });
            this.borrowingService.CreateBorrowings(1, new List<int> { 1, 2, 3 }, DateTime.Now, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingDailyLimit_NonStaff()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = DefaultStaffId, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(DefaultStaffId)).Return(staff);

            var borrowDate = DateTime.Now.AddDays(-5);

            var todayBorrowings = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerDay).Select(i => new Borrowing { Id = i, BorrowingDate = borrowDate, ReaderId = 1 }).ToList();

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate)).Return(todayBorrowings);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(todayBorrowings);

            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenExceedingPeriodLimit_NonStaff()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = DefaultStaffId, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(DefaultStaffId)).Return(staff);

            var borrowDate = DateTime.Now.AddDays(-5);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate))
                .Return(new List<Borrowing>());
            var periodBorrowings = Enumerable.Range(1, this.mockConfigRepository.MaxBooksPerPeriod)
                .Select(i => new Borrowing { Id = i, BorrowingDate = borrowDate.AddDays(-1), ReaderId = 1 }).ToList();

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(periodBorrowings);
            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenCanBorrowBookFails()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = DefaultStaffId, IsStaff = true };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(DefaultStaffId)).Return(staff);
            var borrowDate = DateTime.Now.AddDays(-5);
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(new List<Borrowing>());
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(null);
            this.borrowingService.CreateBorrowings(1, new List<int> { 10 }, borrowDate, 14, DefaultStaffId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateBorrowings_WhenRepositoryAddThrows()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(reader.Id)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(staff.Id)).Return(staff);

            var borrowDate = DateTime.Now.AddDays(-5);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate))
                .Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.SetupCanBorrowBookAlwaysTrue(readerId: reader.Id, isStaff: reader.IsStaff);

            this.mockBorrowingRepository.Stub(x => x.Add(Arg<Borrowing>.Is.Anything))
                .Throw(new Exception("DB down"));

            try
            {
                this.borrowingService.CreateBorrowings(reader.Id, new List<int> { 10 }, borrowDate, 14, staff.Id);
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to create borrowing record for book"));
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual("DB down", ex.InnerException.Message);
                throw;
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBorrowings_WithValidData()
        {
            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(reader.Id)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(staff.Id)).Return(staff);

            var borrowDate = DateTime.Now.AddDays(-5);
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(borrowDate.Date, borrowDate))
                .Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.SetupCanBorrowBookAlwaysTrue(readerId: reader.Id, isStaff: reader.IsStaff);

            var bookIds = new List<int> { 10, 11 };

            var created = new List<Borrowing>();
            this.mockBorrowingRepository.Stub(x => x.Add(Arg<Borrowing>.Is.Anything))
                .WhenCalled(invocation => created.Add((Borrowing)invocation.Arguments[0]));
            this.borrowingService.CreateBorrowings(reader.Id, bookIds, borrowDate, 14, staff.Id);
            Assert.AreEqual(2, created.Count);
            Assert.IsTrue(created.All(b => b.IsActive));
            Assert.IsTrue(created.All(b => b.InitialBorrowingDays == 14));
            Assert.IsTrue(created.All(b => b.BorrowingDate.Date == borrowDate.Date));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenBorrowingNotFound()
        {
            this.mockBorrowingRepository.Stub(x => x.GetById(999)).Return(null);
            this.borrowingService.ExtendBorrowing(999, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenBorrowingIsInactive()
        {
            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 1,
                IsActive = false,
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenReaderNotFound()
        {
            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 123,
                BookId = 5,
                IsActive = true,
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(123)).Return(null);
            this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenExceedingMaxExtensionDays()
        {
            this.mockConfigRepository.MaxExtensionDays = 10;

            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 10,
                IsActive = true,
                TotalExtensionDays = 10,
                DueDate = DateTime.Now.AddDays(10),
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(new Reader { Id = 1, IsStaff = false });
            this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenBookNotFound()
        {
            this.mockConfigRepository.MaxExtensionDays = 10;

            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 999,
                IsActive = true,
                TotalExtensionDays = 0,
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(new Reader { Id = 1, IsStaff = false });
            this.mockBookRepository.Stub(x => x.GetById(999)).Return(null);
            this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenBookUnavailable()
        {
            this.mockConfigRepository.MaxExtensionDays = 10;
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 10,
                IsActive = true,
                TotalExtensionDays = 0,
            };

            var book = new Book
            {
                Id = 10,
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 1,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(new Reader { Id = 1, IsStaff = false });
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);
            this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenAvailabilityBelowMinimumPercentage()
        {
            this.mockConfigRepository.MaxExtensionDays = 10;
            this.mockConfigRepository.MinAvailablePercentage = 0.9;

            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 10,
                IsActive = true,
                TotalExtensionDays = 0,
            };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>
                {
                    new Borrowing { IsActive = true },
                    new Borrowing { IsActive = true },
                },
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(new Reader { Id = 1, IsStaff = false });
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);
            this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenExceedingThreeMonthExtensionLimit()
        {
            this.mockConfigRepository.MaxExtensionDays = 10;
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var reader = new Reader { Id = 1, IsStaff = false };

            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 10,
                IsActive = true,
                TotalExtensionDays = 0,
                DueDate = DateTime.Now.AddDays(14),
            };

            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };
            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);

            var extensionDate = new DateTime(2026, 1, 1);
            var borrowingsInLastThreeMonths = new List<Borrowing>
            {
                new Borrowing { ReaderId = 1, TotalExtensionDays = 10 },
            };

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(borrowingsInLastThreeMonths);

            this.borrowingService.ExtendBorrowing(1, 1, extensionDate);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExtendBorrowing_WhenRepositoryUpdate()
        {
            this.mockConfigRepository.MaxExtensionDays = 10;
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var borrowing = new Borrowing
            {
                Id = 1,
                ReaderId = 1,
                BookId = 10,
                IsActive = true,
                TotalExtensionDays = 0,
                DueDate = DateTime.Now.AddDays(14),
            };

            var reader = new Reader { Id = 1, IsStaff = false };

            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.Update(Arg<Borrowing>.Is.Anything)).Throw(new Exception("DB down"));

            try
            {
                this.borrowingService.ExtendBorrowing(1, 1, DateTime.Now);
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to extend borrowing record"));
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual("DB down", ex.InnerException.Message);
                throw;
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WhenExceedingStaffDailyLimit_ForNonStaffReader_ThrowsInvalidOperationException()
        {
            this.mockConfigRepository.MaxBooksStaffPerDay = 3;
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };

            var book = new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);
            var todayStaffBorrowings = new List<Borrowing>
                {
                    new Borrowing { StaffId = 2, IsActive = true },
                    new Borrowing { StaffId = 2, IsActive = true },
                    new Borrowing { StaffId = 2, IsActive = true },
                };

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(todayStaffBorrowings);
            this.borrowingService.BorrowBook(1, 10, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WhenCanBorrowBookFails_ThrowsInvalidOperationException()
        {
            this.mockConfigRepository.MinAvailablePercentage = 0;

            var reader = new Reader { Id = 1, IsStaff = false };
            var staff = new Reader { Id = 2, IsStaff = true };
            var book = new Book
            {
                Id = 10,
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 1,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { new BookDomain { Id = 1, Name = "D1" } },
            };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.mockBookRepository.Stub(x => x.GetById(10)).Return(book);
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.borrowingService.BorrowBook(1, 10, 14, 2);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateBorrowings_WhenBorrowerIsStaff_DoesNotApplyStaffDailyLimit()
        {
            var borrowDate = DateTime.Now.AddDays(-10);

            var reader = new Reader { Id = 1, IsStaff = true };
            var staff = new Reader { Id = 2, IsStaff = true };

            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.GetById(2)).Return(staff);
            this.mockConfigRepository.MaxBooksStaffPerDay = 1;

            var overloadedStaffBorrowings = new List<Borrowing>
    {
        new Borrowing { StaffId = 2, IsActive = true },
        new Borrowing { StaffId = 2, IsActive = true },
        new Borrowing { StaffId = 2, IsActive = true },
    };

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(overloadedStaffBorrowings);

            var domain1 = new BookDomain { Id = 1, Name = "D1" };
            var domain2 = new BookDomain { Id = 2, Name = "D2" };

            var bookIds = new List<int> { 10, 11, 12 };

            this.mockBookRepository.Stub(x => x.GetById(10)).Return(new Book
            {
                Id = 10,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { domain1 },
            });

            this.mockBookRepository.Stub(x => x.GetById(11)).Return(new Book
            {
                Id = 11,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { domain2 },
            });

            this.mockBookRepository.Stub(x => x.GetById(12)).Return(new Book
            {
                Id = 12,
                TotalCopies = 10,
                ReadingRoomOnlyCopies = 0,
                BorrowingRecords = new List<Borrowing>(),
                Domains = new List<BookDomain> { domain1 },
            });

            this.mockConfigRepository.MinAvailablePercentage = 0;
            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(1)).Return(new List<Borrowing>());
            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(Arg<int>.Is.Anything)).Return(new List<Borrowing>());
            this.borrowingService.CreateBorrowings(1, bookIds, borrowDate, 14, 2);
            this.mockBorrowingRepository.AssertWasCalled(
                x => x.Add(Arg<Borrowing>.Is.Anything),
                opt => opt.Repeat.Times(bookIds.Count));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReturnBorrowing_WhenBorrowingNotFound_ThrowsInvalidOperationException()
        {
            this.mockBorrowingRepository.Stub(x => x.GetById(999)).Return(null);
            this.borrowingService.ReturnBorrowing(999, DateTime.Now);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReturnBorrowing_WhenRepositoryUpdateThrows_ThrowsInvalidOperationException()
        {
            var borrowing = new Borrowing
            {
                Id = 1,
                IsActive = true,
                ReaderId = 1,
                BookId = 10,
            };

            this.mockBorrowingRepository.Stub(x => x.GetById(1)).Return(borrowing);
            this.mockBorrowingRepository.Stub(x => x.Update(Arg<Borrowing>.Is.Anything))
                .Throw(new Exception("DB down"));

            this.borrowingService.ReturnBorrowing(1, DateTime.Now);
        }

        private void SetupCanBorrowBookAlwaysTrue(int readerId, bool isStaff)
        {
            var reader = new Reader { Id = readerId, IsStaff = isStaff };
            this.mockReaderRepository.Stub(x => x.GetById(readerId)).Return(reader);
            var domain = new BookDomain { Id = 1, Name = "D1" };

            this.mockBookRepository.Stub(x => x.GetById(Arg<int>.Is.Anything))
                .Return(new Book
                {
                    Id = 1,
                    TotalCopies = 10,
                    ReadingRoomOnlyCopies = 0,
                    BorrowingRecords = new List<Borrowing>(),
                    Domains = new List<BookDomain> { domain },
                });
            this.mockBorrowingRepository.Stub(x => x.GetActiveBorrowingsByReader(readerId))
                .Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByDateRange(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything))
                .Return(new List<Borrowing>());

            this.mockBorrowingRepository.Stub(x => x.GetBorrowingsByBook(Arg<int>.Is.Anything))
                .Return(new List<Borrowing>());
        }
    }
}
