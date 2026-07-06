using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveToUsersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.RenameTable(
                name: "UserShifts",
                schema: "public",
                newName: "UserShifts",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "public",
                newName: "Users",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "public",
                newName: "UserRoles",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                schema: "public",
                newName: "UserPermissions",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "Shifts",
                schema: "public",
                newName: "Shifts",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "ShiftNotification",
                schema: "public",
                newName: "ShiftNotification",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "ShiftInstances",
                schema: "public",
                newName: "ShiftInstances",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "ShiftAttendances",
                schema: "public",
                newName: "ShiftAttendances",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "public",
                newName: "Roles",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "public",
                newName: "RolePermissions",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "PersonalAccessTokens",
                schema: "public",
                newName: "PersonalAccessTokens",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "public",
                newName: "Permissions",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "PasswordPolicies",
                schema: "public",
                newName: "PasswordPolicies",
                newSchema: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "UserShifts",
                schema: "users",
                newName: "UserShifts",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "users",
                newName: "Users",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "users",
                newName: "UserRoles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                schema: "users",
                newName: "UserPermissions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Shifts",
                schema: "users",
                newName: "Shifts",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ShiftNotification",
                schema: "users",
                newName: "ShiftNotification",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ShiftInstances",
                schema: "users",
                newName: "ShiftInstances",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ShiftAttendances",
                schema: "users",
                newName: "ShiftAttendances",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "users",
                newName: "Roles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "users",
                newName: "RolePermissions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PersonalAccessTokens",
                schema: "users",
                newName: "PersonalAccessTokens",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "users",
                newName: "Permissions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PasswordPolicies",
                schema: "users",
                newName: "PasswordPolicies",
                newSchema: "public");
        }
    }
}
