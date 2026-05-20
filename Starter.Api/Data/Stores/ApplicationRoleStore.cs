using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Starter.Api.Data.Stores;

public sealed class ApplicationRoleStore(IDbConnectionFactory db) : IRoleStore<IdentityRole<Guid>>
{
    public Task<string> GetRoleIdAsync(IdentityRole<Guid> role, CancellationToken ct) =>
        Task.FromResult(role.Id.ToString());

    public Task<string?> GetRoleNameAsync(IdentityRole<Guid> role, CancellationToken ct) =>
        Task.FromResult(role.Name);

    public Task SetRoleNameAsync(IdentityRole<Guid> role, string? roleName, CancellationToken ct)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole<Guid> role, CancellationToken ct) =>
        Task.FromResult(role.NormalizedName);

    public Task SetNormalizedRoleNameAsync(IdentityRole<Guid> role, string? normalizedName, CancellationToken ct)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole<Guid> role, CancellationToken ct)
    {
        if (role.Id == Guid.Empty) role.Id = Guid.NewGuid();
        role.ConcurrencyStamp = Guid.NewGuid().ToString();

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO ""AspNetRoles"" (""Id"", ""Name"", ""NormalizedName"", ""ConcurrencyStamp"") VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)",
            role);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole<Guid> role, CancellationToken ct)
    {
        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE ""AspNetRoles"" SET ""Name"" = @Name, ""NormalizedName"" = @NormalizedName, ""ConcurrencyStamp"" = @ConcurrencyStamp WHERE ""Id"" = @Id",
            role);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole<Guid> role, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(@"DELETE FROM ""AspNetRoles"" WHERE ""Id"" = @Id", new { role.Id });
        return IdentityResult.Success;
    }

    public async Task<IdentityRole<Guid>?> FindByIdAsync(string roleId, CancellationToken ct)
    {
        if (!Guid.TryParse(roleId, out var id)) return null;
        using var conn = db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<IdentityRole<Guid>>(
            @"SELECT * FROM ""AspNetRoles"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public async Task<IdentityRole<Guid>?> FindByNameAsync(string normalizedRoleName, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<IdentityRole<Guid>>(
            @"SELECT * FROM ""AspNetRoles"" WHERE ""NormalizedName"" = @NormalizedName",
            new { NormalizedName = normalizedRoleName });
    }

    public void Dispose() { }
}
