using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conduit.Features.Users;

[Route("users")]
public class UsersController(IMediator mediator)
{
    [HttpPost]
    public Task<UserEnvelope> Create(
        [FromBody] Create.Command command,
        CancellationToken cancellationToken
    ) => mediator.Send(command, cancellationToken);

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public Task<UserEnvelope> Login(
        [FromBody] Login.Command command,
        CancellationToken cancellationToken
    ) => mediator.Send(command, cancellationToken);
}
