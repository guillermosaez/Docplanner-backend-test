using FluentResults;
using MassTransit;
using Moq;
using SlotManager.Application.TakeSlot;
using SlotManager.Application.TakeSlot.Validations;
using SlotManager.Domain.GetAvailability;
using SlotManager.Domain.TakeSlot;

namespace SlotManager.Application.UnitTests.TakeSlot;

public class TakeSlotCommandHandlerTests
{
    private readonly Mock<ITakeSlotRequestValidator> _takeSlotRequestValidatorMock = new();
    private readonly Mock<ISlotServiceClient> _slotServiceClientMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

    private TakeSlotCommandHandler _sut => new(_takeSlotRequestValidatorMock.Object, _slotServiceClientMock.Object, _publishEndpointMock.Object);
    
    [Fact]
    public async Task Handle_When_validation_is_ko_Then_error_is_returned()
    {
        //Arrange
        var slotStart = new DateTime(2024, 7, 7, 9, 0, 0, DateTimeKind.Utc);
        var slotEnd = slotStart.AddMinutes(30);
        var request = new TakeSlotCommand
        {
            FacilityId = Guid.NewGuid(),
            Start = slotStart,
            End = slotEnd,
            Comments = "Comments",
            Patient = new()
            {
                Name = "Patient",
                SecondName = "SecondName",
                Email = "email@test.com",
                Phone = "666 666 666"
            }
        };
        var cancellationToken = new CancellationToken(true);
        var failedResult = Result.Fail(Mock.Of<IError>());
        _takeSlotRequestValidatorMock.Setup(t => t.ValidateAsync(request, cancellationToken)).ReturnsAsync(failedResult);
        
        //Act
        var result = await _sut.Handle(request, cancellationToken);
        
        //Assert
        Assert.True(result.IsFailed);
        _slotServiceClientMock.Verify(s => s.TakeSlotAsync(It.IsAny<TakeSlotRequest>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_When_validation_is_ok_Then_request_is_made_to_service_with_all_data_filled()
    {
        //Arrange
        var slotStart = new DateTime(2024, 7, 7, 9, 0, 0, DateTimeKind.Utc);
        var slotEnd = slotStart.AddMinutes(30);
        var request = new TakeSlotCommand
        {
            FacilityId = Guid.NewGuid(),
            Start = slotStart,
            End = slotEnd,
            Comments = "Comments",
            Patient = new()
            {
                Name = "Patient",
                SecondName = "SecondName",
                Email = "email@test.com",
                Phone = "666 666 666"
            }
        };
        var cancellationToken = new CancellationToken(true);
        _takeSlotRequestValidatorMock.Setup(t => t.ValidateAsync(request, cancellationToken)).ReturnsAsync(Result.Ok);
        
        //Act
        var result = await _sut.Handle(request, cancellationToken);
        
        //Assert
        Assert.True(result.IsSuccess);
        _slotServiceClientMock.Verify(
            s => s.TakeSlotAsync(It.Is<TakeSlotRequest>(r => r.Start == request.Start
                                                             && r.End == request.End
                                                             && r.Comments == request.Comments
                                                             && r.Patient.Name == request.Patient.Value.Name
                                                             && r.Patient.SecondName == request.Patient.Value.SecondName
                                                             && r.Patient.Email == request.Patient.Value.Email
                                                             && r.Patient.Phone == request.Patient.Value.Phone
                                                             && r.FacilityId == request.FacilityId), cancellationToken),
            Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.Is<SlotBooked>(sb => sb.Slot.Start == request.Start
                                                                           && sb.Slot.End == request.End
                                                                           && sb.Slot.Comments == request.Comments
                                                                           && sb.Slot.Patient.Name == request.Patient.Value.Name
                                                                           && sb.Slot.Patient.SecondName == request.Patient.Value.SecondName
                                                                           && sb.Slot.Patient.Email == request.Patient.Value.Email
                                                                           && sb.Slot.Patient.Phone == request.Patient.Value.Phone
                                                                           && sb.Slot.FacilityId == request.FacilityId), cancellationToken),
            Times.Once);
    }
}