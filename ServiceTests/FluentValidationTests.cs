// <copyright file="FluentValidationTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace ServiceTests
{
    using System;
    using System.Collections.Generic;
    using Data.Validators;
    using Domain.Models;
    using FluentValidation.TestHelper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Fluent validation tests.
    /// </summary>
    [TestClass]
    public class FluentValidationTests
    {
        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_NegativeTotalCopies_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "Valid Title",
                ISBN = "1234567890",
                TotalCopies = -5,
                ReadingRoomOnlyCopies = 0,
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.TotalCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_NegativeReadingRoomCopies_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "Valid Title",
                ISBN = "1234567890",
                TotalCopies = 10,
                ReadingRoomOnlyCopies = -1,
            };

            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.ReadingRoomOnlyCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_ReadingRoomExceedingTotal_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "Valid Title",
                ISBN = "1234567890",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 10,
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.ReadingRoomOnlyCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_NullISBN_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "Valid Title",
                ISBN = null,
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 1,
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.ISBN);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_InvalidISBNFormat_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "Valid Title",
                ISBN = "ABC",
                TotalCopies = 5,
                ReadingRoomOnlyCopies = 1,
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.ISBN);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_ValidFirstName_ShouldNotHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = "John",
                LastName = "Doe",
            };
            var result = validator.TestValidate(author);
            result.ShouldNotHaveValidationErrorFor(a => a.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_ValidLastName_ShouldNotHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = "Jane",
                LastName = "Smith",
            };
            var result = validator.TestValidate(author);
            result.ShouldNotHaveValidationErrorFor(a => a.LastName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_NullFirstName_ShouldHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = null,
                LastName = "Doe",
            };
            var result = validator.TestValidate(author);
            result.ShouldHaveValidationErrorFor(a => a.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_EmptyFirstName_ShouldHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = string.Empty,
                LastName = "Doe",
            };
            var result = validator.TestValidate(author);
            result.ShouldHaveValidationErrorFor(a => a.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_NullLastName_ShouldHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = "John",
                LastName = null,
            };
            var result = validator.TestValidate(author);
            result.ShouldHaveValidationErrorFor(a => a.LastName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_EmptyLastName_ShouldHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = "Jane",
                LastName = string.Empty,
            };
            var result = validator.TestValidate(author);
            result.ShouldHaveValidationErrorFor(a => a.LastName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void AuthorValidator_WhitespaceFirstName_ShouldHaveValidationErrorFor()
        {
            var validator = new AuthorValidator();
            var author = new Author
            {
                FirstName = "   ",
                LastName = "Doe",
            };
            var result = validator.TestValidate(author);
            result.ShouldHaveValidationErrorFor(a => a.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_ValidFirstName_ShouldNotHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "Jane",
                LastName = "Smith",
                Address = "123 Main Street",
                Email = "jane@example.com",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_ValidEmail_ShouldNotHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "Jane",
                LastName = "Smith",
                Address = "123 Main Street",
                Email = "jane.smith@example.com",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldNotHaveValidationErrorFor(r => r.Email);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_ValidAddress_ShouldNotHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "Jane",
                LastName = "Smith",
                Address = "456 Oak Avenue, City, Country",
                Email = "jane@example.com",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldNotHaveValidationErrorFor(r => r.Address);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_NullFirstName_ShouldHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = null,
                LastName = "Smith",
                Address = "123 Main Street",
                Email = "jane@example.com",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldHaveValidationErrorFor(r => r.FirstName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_InvalidEmail_ShouldHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "Jane",
                LastName = "Smith",
                Address = "123 Main Street",
                Email = "invalid-email-without-at",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldHaveValidationErrorFor(r => r.Email);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_NullAddress_ShouldHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "Jane",
                LastName = "Smith",
                Address = null,
                Email = "jane@example.com",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldHaveValidationErrorFor(r => r.Address);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_EmptyLastName_ShouldHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "Jane",
                LastName = string.Empty,
                Address = "123 Main Street",
                Email = "jane@example.com",
                PhoneNumber = "555-1234",
            };
            var result = validator.TestValidate(reader);
            result.ShouldHaveValidationErrorFor(r => r.LastName);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void EditionValidator_ValidPublisher_ShouldNotHaveValidationErrorFor()
        {
            var validator = new EditionValidator();
            var edition = new Edition
            {
                BookId = 1,
                Publisher = "Publishing House Inc.",
                Year = 2023,
                EditionNumber = 1,
                PageCount = 300,
                BookType = "Hardcover",
            };
            var result = validator.TestValidate(edition);
            result.ShouldNotHaveValidationErrorFor(e => e.Publisher);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_EmptyTitle_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = string.Empty,
                ISBN = "1234567",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.Title);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_TitleTooLong_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var longTitle = new string('A', 201);
            var book = new Book
            {
                Title = longTitle,
                ISBN = "1234567",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.Title);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_TotalCopiesZero_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "T",
                ISBN = "1234567",
                TotalCopies = 0,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.TotalCopies);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_AuthorsEmpty_ShouldHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "T",
                ISBN = "1234567",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author>(),
            };
            var result = validator.TestValidate(book);
            result.ShouldHaveValidationErrorFor(b => b.Authors);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookValidator_ValidISBN_ShouldNotHaveValidationErrorFor()
        {
            var validator = new BookValidator();
            var book = new Book
            {
                Title = "Valid",
                ISBN = "1234567890",
                TotalCopies = 1,
                ReadingRoomOnlyCopies = 0,
                Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
            };
            var result = validator.TestValidate(book);
            result.ShouldNotHaveValidationErrorFor(b => b.ISBN);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_NoContactMethods_ShouldHaveValidationErrorForModel()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "NoContact",
                LastName = "User",
                Address = "Addr",
                Email = null,
                PhoneNumber = null,
            };
            var result = validator.TestValidate(reader);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Count > 0 && result.Errors[0].ErrorMessage.Contains("At least one contact method"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_InvalidPhone_ShouldHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "P",
                LastName = "User",
                Address = "Addr",
                Email = string.Empty,
                PhoneNumber = "not-a-phone",
            };
            var result = validator.TestValidate(reader);
            result.ShouldHaveValidationErrorFor(r => r.PhoneNumber);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void ReaderValidator_ValidPhone_ShouldNotHaveValidationErrorFor()
        {
            var validator = new ReaderValidator();
            var reader = new Reader
            {
                FirstName = "P",
                LastName = "User",
                Address = "Addr",
                Email = string.Empty,
                PhoneNumber = "+1 (555) 123-4567",
            };
            var result = validator.TestValidate(reader);
            result.ShouldNotHaveValidationErrorFor(r => r.PhoneNumber);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void EditionValidator_BookIdZero_ShouldHaveValidationErrorFor()
        {
            var validator = new EditionValidator();
            var edition = new Edition
            {
                BookId = 0,
                Publisher = "P",
                Year = 2020,
                EditionNumber = 1,
                PageCount = 100,
                BookType = "T",
            };
            var result = validator.TestValidate(edition);
            result.ShouldHaveValidationErrorFor(e => e.BookId);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void EditionValidator_EmptyBookType_ShouldHaveValidationErrorFor()
        {
            var validator = new EditionValidator();
            var edition = new Edition
            {
                BookId = 1,
                Publisher = "P",
                Year = 2020,
                EditionNumber = 1,
                PageCount = 100,
                BookType = string.Empty,
            };
            var result = validator.TestValidate(edition);
            result.ShouldHaveValidationErrorFor(e => e.BookType);
        }
    }
}
