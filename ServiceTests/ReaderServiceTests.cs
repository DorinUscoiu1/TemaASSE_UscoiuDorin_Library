// <copyright file="ReaderServiceTests.cs" company="Transilvania University of Brasov">
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
    /// Comprehensive unit tests for <see cref="ReaderService"/>.
    /// </summary>
    [TestClass]
    public class ReaderServiceTests
    {
        private IReader mockReaderRepository;
        private ReaderService readerService;

        /// <summary>
        /// Initializes test fixtures before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockReaderRepository = MockRepository.GenerateStub<IReader>();
            this.readerService = new ReaderService(this.mockReaderRepository);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAllReaders_WhenCalled()
        {
            var readers = new List<Reader>
            {
                new Reader { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" },
                new Reader { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" },
            };
            this.mockReaderRepository.Stub(x => x.GetAll()).Return(readers);
            var result = this.readerService.GetAllReaders();
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.FirstName == "John"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetReaderById_WithValidId_ReturnsCorrectReader()
        {
            var reader = new Reader { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            var result = this.readerService.GetReaderById(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("John", result.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetReaderById_WithInvalidId_ReturnsNull()
        {
            this.mockReaderRepository.Stub(x => x.GetById(999)).Return(null);
            var result = this.readerService.GetReaderById(999);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateReader_WithValidData_CreatesReader()
        {
            var reader = new Reader
            {
                FirstName = "New",
                LastName = "Reader",
                Email = "new@test.com",
                Address = "Test Address",
                PhoneNumber = "123456789",
            };

            this.mockReaderRepository.Stub(x => x.Add(Arg<Reader>.Is.Anything));
            this.readerService.CreateReader(reader);
            this.mockReaderRepository.AssertWasCalled(x => x.Add(Arg<Reader>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateReader_WithNullReader_ThrowsException()
        {
            this.readerService.CreateReader(null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void CreateReader_WithInvalidData_ThrowsValidationException()
        {
            var reader = new Reader
            {
                FirstName = string.Empty,
                LastName = string.Empty,
            };
            this.readerService.CreateReader(reader);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateReader_WithValidData_UpdatesReader()
        {
            var reader = new Reader
            {
                Id = 1,
                FirstName = "Updated",
                LastName = "Reader",
                Email = "updated@test.com",
                Address = "Test Address",
            };
            this.mockReaderRepository.Stub(x => x.GetById(1)).Return(reader);
            this.mockReaderRepository.Stub(x => x.Update(Arg<Reader>.Is.Anything));
            this.readerService.UpdateReader(reader);
            this.mockReaderRepository.AssertWasCalled(x => x.Update(Arg<Reader>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateReader_WithNullReader_ThrowsException()
        {
            this.readerService.UpdateReader(null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void UpdateReader_WithInvalidData_ThrowsValidationException()
        {
            var reader = new Reader
            {
                Id = 1,
                FirstName = string.Empty,
                LastName = "Test",
            };
            this.readerService.UpdateReader(reader);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteReader_WithValidId_DeletesReader()
        {
            this.mockReaderRepository.Stub(x => x.Delete(Arg<int>.Is.Anything));
            this.readerService.DeleteReader(1);
            this.mockReaderRepository.AssertWasCalled(x => x.Delete(1));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetStaffMembers_WhenCalled_ReturnsOnlyStaff()
        {
            var readers = new List<Reader>
            {
                new Reader { Id = 1, FirstName = "Staff", LastName = "Member", IsStaff = true },
                new Reader { Id = 2, FirstName = "Another", LastName = "Staff", IsStaff = true },
            };

            this.mockReaderRepository.Stub(x => x.GetStaffMembers()).Return(readers);
            var result = this.readerService.GetStaffMembers();
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(r => r.IsStaff));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetRegularReaders_WhenCalled_ReturnsOnlyRegular()
        {
            var readers = new List<Reader>
            {
                new Reader { Id = 1, FirstName = "Regular", LastName = "Reader", IsStaff = false },
                new Reader { Id = 2, FirstName = "Another", LastName = "Regular", IsStaff = false },
            };
            this.mockReaderRepository.Stub(x => x.GetRegularReaders()).Return(readers);
            var result = this.readerService.GetRegularReaders();
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(r => !r.IsStaff));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetStaffReaders_WhenCalled_ReturnsStaffMembers()
        {
            var readers = new List<Reader>
            {
                new Reader { Id = 1, FirstName = "Staff", LastName = "Member", IsStaff = true },
            };
            this.mockReaderRepository.Stub(x => x.GetStaffMembers()).Return(readers);
            var result = this.readerService.GetStaffReaders();
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.All(r => r.IsStaff));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ValidateReader_WithValidData_ReturnsTrue()
        {
            var reader = new Reader
            {
                FirstName = "Valid",
                LastName = "Reader",
                Email = "valid@test.com",
                Address = "Test Address",
            };
            var result = this.readerService.ValidateReader(reader);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ValidateReader_WithInvalidData_ReturnsFalse()
        {
            var reader = new Reader
            {
                FirstName = string.Empty,
                LastName = string.Empty,
            };
            var result = this.readerService.ValidateReader(reader);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ValidateReader_WithNullReader_ReturnsFalse()
        {
            var result = this.readerService.ValidateReader(null);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Integration_CreateAndRetrieveReader_Succeeds()
        {
            using (var context = new LibraryDbContext())
            {
                var readerRepo = new ReaderDataService(context);
                var service = new ReaderService(readerRepo);

                var email = "integration@" + Guid.NewGuid() + ".com";
                var reader = new Reader
                {
                    FirstName = "Integration",
                    LastName = "Test",
                    Email = email,
                    Address = "Test Address",
                    PhoneNumber = "123456789",
                };

                try
                {
                    service.CreateReader(reader);
                    context.SaveChanges();
                    var created = context.Readers.FirstOrDefault(r => r.Email == email);
                    Assert.IsNotNull(created);
                    Assert.AreEqual("Integration", created.FirstName);
                    Assert.IsNotNull(created.RegistrationDate);
                }
                finally
                {
                    var toDelete = context.Readers.FirstOrDefault(r => r.Email == email);
                    if (toDelete != null)
                    {
                        context.Readers.Remove(toDelete);
                        context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Integration_UpdateReader_Succeeds()
        {
            using (var context = new LibraryDbContext())
            {
                var readerRepo = new ReaderDataService(context);
                var service = new ReaderService(readerRepo);

                var email = "update@" + Guid.NewGuid() + ".com";
                var reader = new Reader
                {
                    FirstName = "Original",
                    LastName = "Name",
                    Email = email,
                    Address = "Test Address",
                    PhoneNumber = "123456789",
                };

                try
                {
                    service.CreateReader(reader);
                    context.SaveChanges();
                    var created = context.Readers.FirstOrDefault(r => r.Email == email);
                    Assert.IsNotNull(created);
                    created.FirstName = "Updated";
                    service.UpdateReader(created);
                    context.SaveChanges();
                    var updated = context.Readers.FirstOrDefault(r => r.Email == email);
                    Assert.AreEqual("Updated", updated.FirstName);
                }
                finally
                {
                    var toDelete = context.Readers.FirstOrDefault(r => r.Email == email);
                    if (toDelete != null)
                    {
                        context.Readers.Remove(toDelete);
                        context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Integration_DeleteReader_Succeeds()
        {
            using (var context = new LibraryDbContext())
            {
                var readerRepo = new ReaderDataService(context);
                var service = new ReaderService(readerRepo);

                var email = "delete@" + Guid.NewGuid() + ".com";
                var reader = new Reader
                {
                    FirstName = "ToDelete",
                    LastName = "Reader",
                    Email = email,
                    Address = "Test Address",
                    PhoneNumber = "123456789",
                };
                service.CreateReader(reader);
                context.SaveChanges();
                var created = context.Readers.FirstOrDefault(r => r.Email == email);
                Assert.IsNotNull(created);
                service.DeleteReader(created.Id);
                context.SaveChanges();
                var deleted = context.Readers.FirstOrDefault(r => r.Email == email);
                Assert.IsNull(deleted);
            }
        }
    }
}
