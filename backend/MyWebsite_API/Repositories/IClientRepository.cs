using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync(string? search, string? country, string? status, int? productId);
    Task<Client?> GetByIdAsync(int clientId);
    Task<int> CreateAsync(ClientCreateRequest request);
    Task<bool> UpdateAsync(int clientId, ClientUpdateRequest request);
    Task<bool> DeleteAsync(int clientId);
}
