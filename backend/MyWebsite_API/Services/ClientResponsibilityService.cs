using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class ClientResponsibilityService(IClientResponsibilityRepository responsibilityRepository) : IClientResponsibilityService
{
    public Task<IReadOnlyList<ClientResponsibilityDetails>> GetAllAsync(int? clientId, int? teamMemberId) =>
        responsibilityRepository.GetAllAsync(clientId, teamMemberId);

    public Task<ClientResponsibilityDetails?> GetByIdAsync(int responsibilityId) =>
        responsibilityRepository.GetByIdAsync(responsibilityId);

    public Task<int> CreateAsync(ClientResponsibilityRequest request) =>
        responsibilityRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int responsibilityId, ClientResponsibilityRequest request) =>
        responsibilityRepository.UpdateAsync(responsibilityId, request);

    public Task<bool> DeleteAsync(int responsibilityId) =>
        responsibilityRepository.DeleteAsync(responsibilityId);
}
