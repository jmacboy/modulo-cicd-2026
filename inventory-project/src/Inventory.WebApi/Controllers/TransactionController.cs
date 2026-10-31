using Inventory.Application.Transactions.CompleteTransaction;
using Inventory.Application.Transactions.CreateTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : Controller
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result.Value);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteTransaction([FromBody] CompleteTransactionCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result.Value);
    }
}