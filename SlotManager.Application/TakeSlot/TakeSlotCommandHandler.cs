using FluentResults;
using MassTransit;
using MediatR;
using SlotManager.Application.TakeSlot.Validations;
using SlotManager.Domain.GetAvailability;
using SlotManager.Domain.TakeSlot;

namespace SlotManager.Application.TakeSlot;

public class TakeSlotCommandHandler : IRequestHandler<TakeSlotCommand, Result>
{
    private readonly ITakeSlotRequestValidator _requestValidator;
    private readonly ISlotServiceClient _slotServiceClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public TakeSlotCommandHandler(ITakeSlotRequestValidator requestValidator, ISlotServiceClient slotServiceClient, IPublishEndpoint publishEndpoint)
    {
        _requestValidator = requestValidator;
        _slotServiceClient = slotServiceClient;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task<Result> Handle(TakeSlotCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _requestValidator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }
        var requestBody = BuildRequestToService(request);
        await _slotServiceClient.TakeSlotAsync(requestBody, cancellationToken);
        await _publishEndpoint.Publish(new SlotBooked{ Slot = requestBody }, cancellationToken);
        return Result.Ok();
    }

    private static TakeSlotRequest BuildRequestToService(TakeSlotCommand request)
    {
        var patient = request.Patient!.Value;
        return new TakeSlotRequest
        {
            Start = request.Start!.Value,
            End = request.End!.Value,
            Comments = request.Comments,
            Patient = new()
            {
                Name = patient.Name,
                SecondName = patient.SecondName,
                Email = patient.Email,
                Phone = patient.Phone
            },
            FacilityId = request.FacilityId!.Value
        };
    }
}