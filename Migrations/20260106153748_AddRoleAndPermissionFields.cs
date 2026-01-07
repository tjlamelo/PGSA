using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGSA_Licence3.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndPermissionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Niveau",
                table: "Cours",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Niveau",
                table: "Cours");
        }
    }
}
