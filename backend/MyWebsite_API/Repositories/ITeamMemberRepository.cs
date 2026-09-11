using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface ITeamMemberRepository
{
    Task<IReadOnlyList<TeamMemberSummary>> GetAllAsync(string? search, string? status);
    Task<TeamMember?> GetByIdAsync(int teamMemberId);
    Task<int> CreateAsync(TeamMemberRequest request);
    Task<bool> UpdateAsync(int teamMemberId, TeamMemberRequest request);
    Task<bool> DeleteAsync(int teamMemberId);
}
