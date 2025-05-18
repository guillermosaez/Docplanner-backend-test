using SlotManager.Application.GetAvailability;
using SlotManager.Application.GetAvailability.Validations;

namespace SlotManager.Application.UnitTests.GetAvailability;

public class GetAvailabilityQueryTests
{
    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("      ")]
    public void Validate_When_date_is_empty_Then_empty_date_error_is_returned(string? date)
    {
        //Arrange
        var sut = new GetAvailabilityQuery { Date = date };
        
        //Act
        var result = sut.Validate();
        
        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<EmptyDateError>(result.Errors[0]);
        Assert.Equal("Date cannot be empty.", result.Errors[0].Message);
    }
    
    [Theory]
    [InlineData("2025051")]
    [InlineData("202505188")]
    [InlineData("NotAValidDate")]
    [InlineData("202505Eighteen")]
    public void Validate_When_date_format_is_not_correct_Then_format_error_is_returned(string date)
    {
        //Arrange
        var sut = new GetAvailabilityQuery { Date = date };
        
        //Act
        var result = sut.Validate();
        
        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<InvalidDateFormatError>(result.Errors[0]);
        Assert.Equal($"Date {sut.Date} is in unexpected format. It must be YYYYMMDD (example: 20250518)", result.Errors[0].Message);
    }
    
    [Theory]
    [InlineData("20250229")]
    [InlineData("20250230")]
    [InlineData("20250132")]
    public void Validate_When_date_does_not_exist_Then_exception_is_thrown(string date)
    {
        //Arrange
        var sut = new GetAvailabilityQuery { Date = date };
        
        //Act
        var result = sut.Validate();
        
        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<NonExistingDateError>(result.Errors[0]);
        Assert.Equal($"Date {sut.Date} does not exist. Please set a right date.", result.Errors[0].Message);
    }
    
    [Fact]
    public void Validate_When_request_is_correct_Then_no_exception_is_thrown()
    {
        //Arrange
        var sut = new GetAvailabilityQuery { Date = "20250518" };
        
        //Act
        var result = sut.Validate();
        
        //Assert
        Assert.True(result.IsSuccess);
    }
}