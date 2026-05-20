using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Starter.Api.Models;

namespace Starter.Api.Data.Stores;

public sealed class ApplicationUserStore(IDbConnectionFactory db) :
    IUserStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>,
    IUserLockoutStore<ApplicationUser>,
    IUserTwoFactorStore<ApplicationUser>,
    IUserClaimStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>
{
    // ── IUserStore ────────────────────────────────────────────────────────────

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.UserName);

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct)
    {
        if (user.Id == Guid.Empty) user.Id = Guid.NewGuid();
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        const string sql = """
            INSERT INTO "AspNetUsers" (
                "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled",
                "LockoutEnd", "LockoutEnabled", "AccessFailedCount",
                "FirstName", "LastName", "CreatedAt"
            ) VALUES (
                @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail,
                @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp,
                @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled,
                @LockoutEnd, @LockoutEnabled, @AccessFailedCount,
                @FirstName, @LastName, @CreatedAt
            )
            """;

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(sql, user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct)
    {
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        const string sql = """
            UPDATE "AspNetUsers" SET
                "UserName"             = @UserName,
                "NormalizedUserName"   = @NormalizedUserName,
                "Email"                = @Email,
                "NormalizedEmail"      = @NormalizedEmail,
                "EmailConfirmed"       = @EmailConfirmed,
                "PasswordHash"         = @PasswordHash,
                "SecurityStamp"        = @SecurityStamp,
                "ConcurrencyStamp"     = @ConcurrencyStamp,
                "PhoneNumber"          = @PhoneNumber,
                "PhoneNumberConfirmed" = @PhoneNumberConfirmed,
                "TwoFactorEnabled"     = @TwoFactorEnabled,
                "LockoutEnd"           = @LockoutEnd,
                "LockoutEnabled"       = @LockoutEnabled,
                "AccessFailedCount"    = @AccessFailedCount,
                "FirstName"            = @FirstName,
                "LastName"             = @LastName
            WHERE "Id" = @Id
            """;

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(sql, user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(@"DELETE FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { user.Id });
        return IdentityResult.Success;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct)
    {
        if (!Guid.TryParse(userId, out var id)) return null;
        using var conn = db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<ApplicationUser>(
            @"SELECT * FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<ApplicationUser>(
            @"SELECT * FROM ""AspNetUsers"" WHERE ""NormalizedUserName"" = @NormalizedUserName",
            new { NormalizedUserName = normalizedUserName });
    }

    // ── IUserPasswordStore ───────────────────────────────────────────────────

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken ct)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.PasswordHash is not null);

    // ── IUserEmailStore ──────────────────────────────────────────────────────

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken ct)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken ct)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<ApplicationUser>(
            @"SELECT * FROM ""AspNetUsers"" WHERE ""NormalizedEmail"" = @NormalizedEmail",
            new { NormalizedEmail = normalizedEmail });
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken ct)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    // ── IUserSecurityStampStore ──────────────────────────────────────────────

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken ct)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.SecurityStamp);

    // ── IUserLockoutStore ────────────────────────────────────────────────────

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.LockoutEnd);

    public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken ct)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken ct)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken ct)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.AccessFailedCount);

    public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.LockoutEnabled);

    public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken ct)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    // ── IUserTwoFactorStore ──────────────────────────────────────────────────

    public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken ct)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.TwoFactorEnabled);

    // ── IUserClaimStore ──────────────────────────────────────────────────────

    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(string ClaimType, string ClaimValue)>(
            @"SELECT ""ClaimType"", ""ClaimValue"" FROM ""AspNetUserClaims"" WHERE ""UserId"" = @UserId",
            new { UserId = user.Id });
        return rows.Select(r => new Claim(r.ClaimType, r.ClaimValue)).ToList();
    }

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        foreach (var claim in claims)
            await conn.ExecuteAsync(
                @"INSERT INTO ""AspNetUserClaims"" (""UserId"", ""ClaimType"", ""ClaimValue"") VALUES (@UserId, @ClaimType, @ClaimValue)",
                new { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
    }

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE ""AspNetUserClaims"" SET ""ClaimType"" = @NewType, ""ClaimValue"" = @NewValue WHERE ""UserId"" = @UserId AND ""ClaimType"" = @OldType AND ""ClaimValue"" = @OldValue",
            new { UserId = user.Id, NewType = newClaim.Type, NewValue = newClaim.Value, OldType = claim.Type, OldValue = claim.Value });
    }

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        foreach (var claim in claims)
            await conn.ExecuteAsync(
                @"DELETE FROM ""AspNetUserClaims"" WHERE ""UserId"" = @UserId AND ""ClaimType"" = @ClaimType AND ""ClaimValue"" = @ClaimValue",
                new { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var users = await conn.QueryAsync<ApplicationUser>(
            @"SELECT u.* FROM ""AspNetUsers"" u JOIN ""AspNetUserClaims"" c ON c.""UserId"" = u.""Id"" WHERE c.""ClaimType"" = @ClaimType AND c.""ClaimValue"" = @ClaimValue",
            new { ClaimType = claim.Type, ClaimValue = claim.Value });
        return users.ToList();
    }

    // ── IUserRoleStore ───────────────────────────────────────────────────────

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var roleId = await conn.QuerySingleOrDefaultAsync<Guid?>(
            @"SELECT ""Id"" FROM ""AspNetRoles"" WHERE ""NormalizedName"" = @NormalizedName",
            new { NormalizedName = roleName.ToUpperInvariant() });
        if (roleId is not null)
            await conn.ExecuteAsync(
                @"INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"") VALUES (@UserId, @RoleId) ON CONFLICT DO NOTHING",
                new { UserId = user.Id, RoleId = roleId });
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            @"DELETE FROM ""AspNetUserRoles"" ur USING ""AspNetRoles"" r WHERE ur.""RoleId"" = r.""Id"" AND ur.""UserId"" = @UserId AND r.""NormalizedName"" = @NormalizedName",
            new { UserId = user.Id, NormalizedName = roleName.ToUpperInvariant() });
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var roles = await conn.QueryAsync<string>(
            @"SELECT r.""Name"" FROM ""AspNetRoles"" r JOIN ""AspNetUserRoles"" ur ON ur.""RoleId"" = r.""Id"" WHERE ur.""UserId"" = @UserId",
            new { UserId = user.Id });
        return roles.ToList();
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM ""AspNetUserRoles"" ur JOIN ""AspNetRoles"" r ON r.""Id"" = ur.""RoleId"" WHERE ur.""UserId"" = @UserId AND r.""NormalizedName"" = @NormalizedName",
            new { UserId = user.Id, NormalizedName = roleName.ToUpperInvariant() });
        return count > 0;
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var users = await conn.QueryAsync<ApplicationUser>(
            @"SELECT u.* FROM ""AspNetUsers"" u JOIN ""AspNetUserRoles"" ur ON ur.""UserId"" = u.""Id"" JOIN ""AspNetRoles"" r ON r.""Id"" = ur.""RoleId"" WHERE r.""NormalizedName"" = @NormalizedName",
            new { NormalizedName = roleName.ToUpperInvariant() });
        return users.ToList();
    }

    public void Dispose() { }
}
