using be_authenticationApplication.Features.Commands.DTOs;
using be_authenticationDomain.CustomException;
using MediatR;

namespace be_authenticationApplication.Features.Commands.Queries.GetCommandById
{
    public class GetCommandByIdHandler : IRequestHandler<GetCommandByIdQuery, CommandDto>
    {
        //private readonly AppDbContext _context;

        //public GetCommandByIdHandler(ICommandRepository repository)
        //{
        //    _repository = repository;
        //}

        //public async Task<GetCommandByIdResponse> Handle(
        //    GetCommandByIdQuery request,
        //    CancellationToken cancellationToken)
        //{
        //    var command = await _repository.GetByIdAsync(request.Id, cancellationToken);

        //    if (command is null)
        //        throw new CustomException.DataNotFoundException("Command not found");

        //    return new CommandDto
        //    {
        //        Id = command.Id,
        //        Name = command.Name
        //    };
        //}

        Task<CommandDto> IRequestHandler<GetCommandByIdQuery, CommandDto>.Handle(GetCommandByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
