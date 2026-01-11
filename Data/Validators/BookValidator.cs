// <copyright file="BookValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Models;
    using FluentValidation;

    /// <summary> dfdsd.</summary>
    public class BookValidator : AbstractValidator<Book>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BookValidator"/> class.
        /// </summary>
        public BookValidator()
        {
            this.RuleFor(b => b.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            this.RuleFor(b => b.ISBN)
                .NotEmpty().WithMessage("ISBN is required")
                  .Matches(@"^\d{6,17}$")
                    .WithMessage("ISBN must be between 6 and 17 digits");

            this.RuleFor(b => b.TotalCopies)
                .GreaterThan(0).WithMessage("Total copies must be greater than 0");

            this.RuleFor(b => b.ReadingRoomOnlyCopies)
                .GreaterThanOrEqualTo(0).WithMessage("Reading room copies cannot be negative")
                .LessThanOrEqualTo(b => b.TotalCopies).WithMessage("Reading room copies cannot exceed total copies");

            this.RuleFor(b => b.Authors)
                .NotEmpty().WithMessage("Book must have at least one author");
        }
    }
}
