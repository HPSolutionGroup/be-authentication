using be_authenticationApplication.Features.Commands.DTOs;
using MediatR;

namespace be_authenticationApplication.Features.Commands.Queries.GetCommandById
{
    public class GetCommandByIdQuery: IRequest<CommandDto>
    {
        public Guid Id { get; set; }
    }
}
