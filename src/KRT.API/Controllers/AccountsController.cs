using KRT.Application.Accounts.Commands.CreateAccount;
using KRT.Application.Accounts.Commands.DeleteAccount;
using KRT.Application.Accounts.Commands.UpdateAccount;
using KRT.Application.Accounts.Queries.GetAccountById;
using KRT.Application.Accounts.Queries.GetAllAccounts;
using KRT.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KRT.API.Controllers;

[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
public sealed class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] AccountStatus? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllAccountsQuery(status), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAccountByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateAccountCommand(id, request.HolderName), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAccountCommand(id), cancellationToken);
        return NoContent();
    }
}
