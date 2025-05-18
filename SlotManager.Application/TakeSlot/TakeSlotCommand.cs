using System.ComponentModel.DataAnnotations;
using FluentResults;
using MediatR;

namespace SlotManager.Application.TakeSlot;

public class TakeSlotCommand : IRequest<Result>
{
    //Required fields are set as nullable to be able to actually throw an error if they're not filled.
    //It they weren't nullable, DateTime for example, fills itself as 0001-01-01 and RequiredAttribute does not throw an error.
    
    [Required]
    public required DateTime? Start { get; init; }
    
    [Required]
    public required DateTime? End { get; init; }
    public string? Comments { get; init; }
    
    [Required]
    public required TakeSlotPatient? Patient { get; init; }
    
    [Required]
    public required Guid? FacilityId { get; init; }
}