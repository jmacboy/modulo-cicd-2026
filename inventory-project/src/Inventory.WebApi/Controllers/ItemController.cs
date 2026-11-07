using Inventory.Application.Items.AddStock;
using Inventory.Application.Items.CreateItem;
using Inventory.Application.Items.DeleteItem;
using Inventory.Application.Items.GetItems;
using Inventory.Application.Items.UpdateItem;
using Inventory.WebApi.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Inventory.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public ItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsFailure)
        {
            return result.Error.Type == Joseco.DDD.Core.Results.ErrorType.NotFound ?
                            NotFound(result.Error.Description) :
                            BadRequest(result.Error);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var query = new GetItemsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var command = new DeleteItemCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Type == Joseco.DDD.Core.Results.ErrorType.NotFound ?
                            NotFound(result.Error.Description) :
                            BadRequest(result.Error);
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemRequest request)
    {
        var command = new UpdateItemCommand(id, request.Name);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Type == Joseco.DDD.Core.Results.ErrorType.NotFound ?
                            NotFound(result.Error.Description) :
                            BadRequest(result.Error);
        }

        return Ok(result);
    }

    [HttpPost("{id}/stock")]
    public async Task<IActionResult> AddStock(Guid id, [FromBody] AddStockRequest request)
    {
        var command = new AddStockCommand(id, request.UserCreatorId, request.Quantity, request.UnitaryCost);
        var result = await _mediator.Send(command, HttpContext.RequestAborted);

        if (result.IsFailure)
        {
            return result.Error.Type == Joseco.DDD.Core.Results.ErrorType.NotFound ?
                            NotFound(result.Error.Description) :
                            BadRequest(result.Error);
        }

        return Ok(result);
    }
}

public record UpdateItemRequest(string Name);