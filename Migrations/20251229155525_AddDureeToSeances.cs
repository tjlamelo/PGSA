using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGSA_Licence3.Migrations
{
    /// <inheritdoc />
    public partial class AddDureeToSeances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duree",
                table: "Seances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HeuresEffectuees",
                table: "Assiduites",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duree",
                table: "Seances");

            migrationBuilder.DropColumn(
                name: "HeuresEffectuees",
                table: "Assiduites");
        }
    }
}
