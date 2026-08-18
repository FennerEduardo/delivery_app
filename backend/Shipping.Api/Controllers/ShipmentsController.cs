using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;
using Shipping.Application.Queries;

namespace Shipping.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetShipmentById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentById(Guid id)
    {
        var shipment = await _mediator.Send(new GetShipmentByIdQuery(id));
        return shipment == null ? NotFound() : Ok(shipment);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShipments([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var shipments = await _mediator.Send(new GetShipmentsQuery(skip, take));
        return Ok(shipments);
    }

    [HttpPost("{id:guid}/quote")]
    [ProducesResponseType(typeof(ShippingQuoteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateQuote(Guid id)
    {
        var quote = await _mediator.Send(new GenerateShipmentQuoteCommand(id));
        return Ok(quote);
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmShipment(Guid id)
    {
        await _mediator.Send(new ConfirmShipmentCommand(id));
        return Ok(new { message = "Shipment confirmed successfully." });
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelShipment(Guid id, [FromBody] CancelRequest request)
    {
        await _mediator.Send(new CancelShipmentCommand(id, request.Reason));
        return Ok(new { message = "Shipment cancelled." });
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        await _mediator.Send(new ChangeShipmentStatusCommand(id, request.NewStatus, request.Comment));
        return Ok(new { message = $"Shipment status updated to '{request.NewStatus}'." });
    }

    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentStatusHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await _mediator.Send(new GetShipmentHistoryQuery(id));
        return Ok(history);
    }
}

public record CancelRequest(string Reason);
public record ChangeStatusRequest(Domain.Enums.ShipmentStatus NewStatus, string Comment);
