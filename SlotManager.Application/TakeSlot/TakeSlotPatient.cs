using System.ComponentModel.DataAnnotations;

namespace SlotManager.Application.TakeSlot;

public readonly record struct TakeSlotPatient
{
    [Required]
    public string Name { get; init; }
    public string? SecondName { get; init; }
    
    [EmailAddress]
    public string Email { get; init; }
    
    [Required]
    public string Phone { get; init; }
}