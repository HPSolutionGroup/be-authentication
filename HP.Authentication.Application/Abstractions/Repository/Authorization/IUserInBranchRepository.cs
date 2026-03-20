using HP.Authentication.Domain.Entities;

namespace HP.Authentication.Application.Abstractions.Repository.Authorization
{
    public interface IUserInBranchRepository
    {
        Task<List<UserInBranch>> GetBranchesOfUserAsync(Guid userId);
    }
}
