using HP.Authentication.Application.Features.Commands.DTOs;
using MediatR;

namespace HP.Authentication.Application.Features.Commands.Queries.GetCommandById
{
    public class GetCommandByIdHandler : IRequestHandler<GetCommandByIdQuery, CommandDto>
    {
        Task<CommandDto> IRequestHandler<GetCommandByIdQuery, CommandDto>.Handle(GetCommandByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
