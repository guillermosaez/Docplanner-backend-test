using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using MediatR;
using SlotManager.Application.GetAvailability.Validations;

namespace SlotManager.Application.GetAvailability;

public partial class GetAvailabilityQuery : IRequest<Result<GetAvailabilityResponse>>
{
    public required string? Date { get; init; }
    
    public Result Validate()
    {
        return ValidateEmptyDate(Date)
            .Bind(() => ValidateDateFormat(Date!))
            .Bind(() => ValidateIfItsRealDate(Date!));
    }

    private static Result ValidateEmptyDate(string? date)
    {
        var isEmptyDate = string.IsNullOrWhiteSpace(date);
        return Result.FailIf(isEmptyDate, new EmptyDateError());
    }

    private static Result ValidateDateFormat(string date)
    {
        var isValidFormat = DateFormatRegex().IsMatch(date);
        return Result.OkIf(isValidFormat, new InvalidDateFormatError(date));
    }
    
    [GeneratedRegex("^[0-9]{8}$")]
    private static partial Regex DateFormatRegex();

    private static Result ValidateIfItsRealDate(string date)
    {
        var doesDateExist = DateOnly.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        return Result.OkIf(doesDateExist, new NonExistingDateError(date));
    }
}