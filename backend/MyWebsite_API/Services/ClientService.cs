using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class ClientService(IClientRepository clientRepository) : IClientService
{
    public Task<IReadOnlyList<Client>> GetAllAsync(string? search, string? country, string? status, int? productId) =>
        clientRepository.GetAllAsync(search, country, status, productId);

    public Task<Client?> GetByIdAsync(int clientId) => clientRepository.GetByIdAsync(clientId);

    public Task<int> CreateAsync(ClientCreateRequest request) => clientRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int clientId, ClientUpdateRequest request) => clientRepository.UpdateAsync(clientId, request);

    public Task<bool> DeleteAsync(int clientId) => clientRepository.DeleteAsync(clientId);
}
