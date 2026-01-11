// <copyright file="ReaderValidator.cs" company="PlaceholderCompany">
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

    /// <summary>valid.</summary>
    public class ReaderValidator : AbstractValidator<Reader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderValidator"/> class.
        /// </summary>
        public ReaderValidator()
        {
            this.RuleFor(r => r.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            this.RuleFor(r => r.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            this.RuleFor(r => r.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

            this.RuleFor(r => r)
                .Must(r => !string.IsNullOrWhiteSpace(r.PhoneNumber) || !string.IsNullOrWhiteSpace(r.Email))
                .WithMessage("At least one contact method (phone or email) must be provided");

            this.RuleFor(r => r.PhoneNumber)
                .Matches(@"^\+?[\d\s\-\(\)]+$").When(r => !string.IsNullOrWhiteSpace(r.PhoneNumber))
                .WithMessage("Invalid phone number format");

            this.RuleFor(r => r.Email)
                .EmailAddress().When(r => !string.IsNullOrWhiteSpace(r.Email))
                .WithMessage("Invalid email format");
        }
    }
}
