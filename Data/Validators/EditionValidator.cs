// <copyright file="EditionValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Data.Validators
{
    using Domain.Models;
    using FluentValidation;

    /// <summary>valid.</summary>
    public class EditionValidator : AbstractValidator<Edition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditionValidator"/> class.
        /// </summary>
        public EditionValidator()
        {
            this.RuleFor(e => e.Publisher)
                .NotEmpty().WithMessage("Publisher is required");

            this.RuleFor(e => e.Year)
                .GreaterThan(0).WithMessage("Year must be a valid year");

            this.RuleFor(e => e.PageCount)
                .GreaterThan(0).WithMessage("Page count must be greater than zero");

            this.RuleFor(e => e.EditionNumber)
                .GreaterThan(0).WithMessage("Edition number must be greater than zero");

            this.RuleFor(e => e.BookType)
                .NotEmpty().WithMessage("Book type is required");

            this.RuleFor(e => e.BookId)
                .GreaterThan(0).WithMessage("BookId must be greater than zero");
        }
    }
}
