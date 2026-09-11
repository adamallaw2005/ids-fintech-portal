using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IClientResponsibilityRepository
{
    Task<IReadOnlyList<ClientResponsibilityDetails>> GetAllAsync(int? clientId, int? teamMemberId);
    Task<ClientResponsibilityDetails?> GetByIdAsync(int responsibilityId);
    Task<int> CreateAsync(ClientResponsibilityRequest request);
    Task<bool> UpdateAsync(int responsibilityId, ClientResponsibilityRequest request);
    Task<bool> DeleteAsync(int responsibilityId);
}
