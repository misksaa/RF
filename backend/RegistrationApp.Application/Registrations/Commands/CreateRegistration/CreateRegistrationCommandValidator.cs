using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Application.Common.Interfaces;

namespace RegistrationApp.Application.Registrations.Commands.CreateRegistration;

public class CreateRegistrationCommandValidator : AbstractValidator<CreateRegistrationCommand>
{
    private readonly IRegistrationDbContext _context;

    // Regular Expression for Arabic/English letters, spaces, hyphens, and apostrophes
    private static readonly Regex NameRegex = new(@"^[a-zA-Z\u0621-\u064A\s'\-]+$", RegexOptions.Compiled);

    // Regular Expression for E.164 mobile format (e.g. +201006158123)
    private static readonly Regex MobileRegex = new(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled);

    public CreateRegistrationCommandValidator(IRegistrationDbContext context)
    {
        _context = context;

        // 1. First Name Validation
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .Matches(NameRegex).WithMessage("First name must only contain Arabic or English letters, spaces, hyphens, and apostrophes.");

        // 2. Middle Name Validation (Optional)
        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("Middle name must not exceed 50 characters.")
            .Matches(NameRegex).When(x => !string.IsNullOrEmpty(x.MiddleName))
            .WithMessage("Middle name must only contain Arabic or English letters, spaces, hyphens, and apostrophes.");

        // 3. Last Name Validation
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .Matches(NameRegex).WithMessage("Last name must only contain Arabic or English letters, spaces, hyphens, and apostrophes.");

        // 4. Birth Date & Age Validation
        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required.")
            .Must(date => date.Date <= DateTime.UtcNow.Date).WithMessage("Birth date cannot be in the future.")
            .Must(BeAtLeast20YearsOld).WithMessage("Minimum age is 20 years old.");

        // 5. Mobile Number Validation & Uniqueness
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(MobileRegex).WithMessage("Mobile number must be in E.164 format (e.g., +201006158123).")
            .MustAsync(BeUniqueMobile).WithMessage("Mobile number is already registered.");

        // 6. Email Validation & Uniqueness
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be in a valid format.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.")
            .MustAsync(BeUniqueEmail).WithMessage("Email is already registered.");

        // 7. Address List Validations
        RuleFor(x => x.Addresses)
            .NotEmpty().WithMessage("At least one address is required.")
            .Must(list => list.Count >= 1 && list.Count <= 5).WithMessage("Addresses list must contain between 1 and 5 addresses.")
            .Must(list => list.Count(a => a.IsPrimary) <= 1).WithMessage("Only one address can be marked as primary.");

        // 8. Individual Address Fields Validations
        RuleForEach(x => x.Addresses).ChildRules(address =>
        {
            address.RuleFor(a => a.GovernorateId)
                .NotEmpty().WithMessage("Governorate is required.")
                .MustAsync(GovernorateExists).WithMessage("Governorate does not exist or is inactive.");

            address.RuleFor(a => a.CityId)
                .NotEmpty().WithMessage("City is required.")
                .MustAsync(CityExists).WithMessage("City does not exist or is inactive.");

            address.RuleFor(a => a)
                .MustAsync(CityBelongToGovernorate).WithMessage("City must belong to the selected Governorate.");

            address.RuleFor(a => a.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(200).WithMessage("Street must not exceed 200 characters.");

            address.RuleFor(a => a.BuildingNumber)
                .NotEmpty().WithMessage("Building number is required.")
                .MaximumLength(20).WithMessage("Building number must not exceed 20 characters.");

            address.RuleFor(a => a.FlatNumber)
                .NotEmpty().WithMessage("Flat number is required.")
                .MaximumLength(20).WithMessage("Flat number must not exceed 20 characters.");
        });
    }

    private bool BeAtLeast20YearsOld(DateTime birthDate)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthDate.Year;
        
        // Exact age calculation
        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age >= 20;
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return !await _context.Registrations
            .AnyAsync(r => r.Email == normalizedEmail, cancellationToken);
    }

    private async Task<bool> BeUniqueMobile(string mobileNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
            return true;

        var normalizedMobile = mobileNumber.Trim();
        return !await _context.Registrations
            .AnyAsync(r => r.MobileNumber == normalizedMobile, cancellationToken);
    }

    private async Task<bool> GovernorateExists(int govId, CancellationToken cancellationToken)
    {
        return await _context.Governorates
            .AnyAsync(g => g.Id == govId && g.IsActive, cancellationToken);
    }

    private async Task<bool> CityExists(int cityId, CancellationToken cancellationToken)
    {
        return await _context.Cities
            .AnyAsync(c => c.Id == cityId && c.IsActive, cancellationToken);
    }

    private async Task<bool> CityBelongToGovernorate(CreateAddressCommandDto addressDto, CancellationToken cancellationToken)
    {
        return await _context.Cities
            .AnyAsync(c => c.Id == addressDto.CityId && c.GovernorateId == addressDto.GovernorateId && c.IsActive, cancellationToken);
    }
}
