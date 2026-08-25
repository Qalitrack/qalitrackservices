using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePasswordPolicySingletonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PasswordPolicy previously used a random Guid.NewGuid() id
            // (BaseEntity's default), so any already-deployed install has a
            // row with a random id, not PasswordPolicy.SingletonId
            // ("00000000-0000-0000-0000-000000000e02"). Without this, the
            // app code would find no row matching the fixed id and insert a
            // fresh default-settings row alongside the old one on the next
            // save, effectively reverting that install's configured policy.
            // Renames the most-recently-updated existing row (if any) to the
            // fixed id; no-ops if that id is already in use (fresh installs
            // seeded after this change).
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    target_id text;
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM users.""PasswordPolicies"" WHERE ""Id"" = '00000000-0000-0000-0000-000000000e02') THEN
                        SELECT ""Id"" INTO target_id
                        FROM users.""PasswordPolicies""
                        ORDER BY ""UpdatedAt"" DESC, ""CreatedAt"" DESC
                        LIMIT 1;

                        IF target_id IS NOT NULL THEN
                            UPDATE users.""PasswordPolicies"" SET ""Id"" = '00000000-0000-0000-0000-000000000e02' WHERE ""Id"" = target_id;
                        END IF;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No meaningful rollback — the original random id isn't recorded anywhere.
        }
    }
}
