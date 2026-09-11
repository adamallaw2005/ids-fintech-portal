using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class TeamMemberService(ITeamMemberRepository teamMemberRepository) : ITeamMemberService
{
    public Task<IReadOnlyList<TeamMemberSummary>> GetAllAsync(string? search, string? status) =>
        teamMemberRepository.GetAllAsync(search, status);

    public Task<TeamMember?> GetByIdAsync(int teamMemberId) =>
        teamMemberRepository.GetByIdAsync(teamMemberId);

    public Task<int> CreateAsync(TeamMemberRequest request) =>
        teamMemberRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int teamMemberId, TeamMemberRequest request) =>
        teamMemberRepository.UpdateAsync(teamMemberId, request);

    public Task<bool> DeleteAsync(int teamMemberId) =>
        teamMemberRepository.DeleteAsync(teamMemberId);
}
