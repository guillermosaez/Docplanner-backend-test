using FluentResults.Extensions.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlotManager.Application.GetAvailability;
using SlotManager.Application.TakeSlot;

namespace SlotManager.API.Controllers.Slots;

[ApiController]
[Route("[controller]")]
public class SlotsController : ControllerBase
{
    private readonly ISender _sender;

    public SlotsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet("availability/{date}")]
    public async Task<IActionResult> GetAvailability(string date, CancellationToken cancellationToken)
    {
        var query = new GetAvailabilityQuery { Date = date };
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("take")]
    public async Task<IActionResult> TakeSlot(TakeSlotCommand request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}