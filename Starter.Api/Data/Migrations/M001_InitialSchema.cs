using System.Data;
using FluentMigrator;

namespace Starter.Api.Data.Migrations;

[Migration(1)]
public sealed class M001_InitialSchema : Migration
{
    public override void Up()
    {
        Create.Table("AspNetRoles")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Name").AsString(256).Nullable()
            .WithColumn("NormalizedName").AsString(256).Nullable()
            .WithColumn("ConcurrencyStamp").AsCustom("text").Nullable();

        Create.Index("IX_AspNetRoles_NormalizedName")
            .OnTable("AspNetRoles").OnColumn("NormalizedName").Unique();

        Create.Table("AspNetUsers")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("FirstName").AsCustom("text").NotNullable()
            .WithColumn("LastName").AsCustom("text").NotNullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("UserName").AsString(256).Nullable()
            .WithColumn("NormalizedUserName").AsString(256).Nullable()
            .WithColumn("Email").AsString(256).Nullable()
            .WithColumn("NormalizedEmail").AsString(256).Nullable()
            .WithColumn("EmailConfirmed").AsBoolean().NotNullable()
            .WithColumn("PasswordHash").AsCustom("text").Nullable()
            .WithColumn("SecurityStamp").AsCustom("text").Nullable()
            .WithColumn("ConcurrencyStamp").AsCustom("text").Nullable()
            .WithColumn("PhoneNumber").AsCustom("text").Nullable()
            .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable()
            .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable()
            .WithColumn("LockoutEnd").AsDateTimeOffset().Nullable()
            .WithColumn("LockoutEnabled").AsBoolean().NotNullable()
            .WithColumn("AccessFailedCount").AsInt32().NotNullable();

        Create.Index("IX_AspNetUsers_NormalizedUserName")
            .OnTable("AspNetUsers").OnColumn("NormalizedUserName").Unique();

        Create.Index("IX_AspNetUsers_NormalizedEmail")
            .OnTable("AspNetUsers").OnColumn("NormalizedEmail");

        Create.Table("AspNetRoleClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("RoleId").AsGuid().NotNullable()
                .ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
            .WithColumn("ClaimType").AsCustom("text").Nullable()
            .WithColumn("ClaimValue").AsCustom("text").Nullable();

        Create.Index("IX_AspNetRoleClaims_RoleId")
            .OnTable("AspNetRoleClaims").OnColumn("RoleId");

        Create.Table("AspNetUserClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsGuid().NotNullable()
                .ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
            .WithColumn("ClaimType").AsCustom("text").Nullable()
            .WithColumn("ClaimValue").AsCustom("text").Nullable();

        Create.Index("IX_AspNetUserClaims_UserId")
            .OnTable("AspNetUserClaims").OnColumn("UserId");

        Create.Table("AspNetUserLogins")
            .WithColumn("LoginProvider").AsCustom("text").PrimaryKey("PK_AspNetUserLogins")
            .WithColumn("ProviderKey").AsCustom("text").PrimaryKey("PK_AspNetUserLogins")
            .WithColumn("ProviderDisplayName").AsCustom("text").Nullable()
            .WithColumn("UserId").AsGuid().NotNullable()
                .ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade);

        Create.Index("IX_AspNetUserLogins_UserId")
            .OnTable("AspNetUserLogins").OnColumn("UserId");

        Create.Table("AspNetUserRoles")
            .WithColumn("UserId").AsGuid().PrimaryKey("PK_AspNetUserRoles")
                .ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
            .WithColumn("RoleId").AsGuid().PrimaryKey("PK_AspNetUserRoles")
                .ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade);

        Create.Index("IX_AspNetUserRoles_RoleId")
            .OnTable("AspNetUserRoles").OnColumn("RoleId");

        Create.Table("AspNetUserTokens")
            .WithColumn("UserId").AsGuid().PrimaryKey("PK_AspNetUserTokens")
                .ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
            .WithColumn("LoginProvider").AsCustom("text").PrimaryKey("PK_AspNetUserTokens")
            .WithColumn("Name").AsCustom("text").PrimaryKey("PK_AspNetUserTokens")
            .WithColumn("Value").AsCustom("text").Nullable();
    }

    public override void Down()
    {
        Delete.Table("AspNetUserTokens");
        Delete.Table("AspNetUserRoles");
        Delete.Table("AspNetUserLogins");
        Delete.Table("AspNetUserClaims");
        Delete.Table("AspNetRoleClaims");
        Delete.Table("AspNetUsers");
        Delete.Table("AspNetRoles");
    }
}
