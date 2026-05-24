using System;
using RegistrationApp.Application.Common.Interfaces;

namespace RegistrationApp.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
