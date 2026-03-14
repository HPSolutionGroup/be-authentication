using HP.Authentication.Application.Features.Commands.DTOs;
using MediatR;

namespace HP.Authentication.Application.Features.Commands.Queries.GetCommandById
{
    public class GetCommandByIdQuery : IRequest<CommandDto>
    {
        public Guid Id { get; set; }
    }
}
