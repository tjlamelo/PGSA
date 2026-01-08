using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGSA_Licence3.Migrations
{
    /// <inheritdoc />
    public partial class updateModelCours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Filiere",
                table: "Cours");

            migrationBuilder.AddColumn<int>(
                name: "CycleId",
                table: "Cours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NiveauId",
                table: "Cours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpecialiteId",
                table: "Cours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cours_CycleId",
                table: "Cours",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Cours_NiveauId",
                table: "Cours",
                column: "NiveauId");

            migrationBuilder.CreateIndex(
                name: "IX_Cours_SpecialiteId",
                table: "Cours",
                column: "SpecialiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cours_Cycles_CycleId",
                table: "Cours",
                column: "CycleId",
                principalTable: "Cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cours_Niveaux_NiveauId",
                table: "Cours",
                column: "NiveauId",
                principalTable: "Niveaux",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cours_Specialites_SpecialiteId",
                table: "Cours",
                column: "SpecialiteId",
                principalTable: "Specialites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cours_Cycles_CycleId",
                table: "Cours");

            migrationBuilder.DropForeignKey(
                name: "FK_Cours_Niveaux_NiveauId",
                table: "Cours");

            migrationBuilder.DropForeignKey(
                name: "FK_Cours_Specialites_SpecialiteId",
                table: "Cours");

            migrationBuilder.DropIndex(
                name: "IX_Cours_CycleId",
                table: "Cours");

            migrationBuilder.DropIndex(
                name: "IX_Cours_NiveauId",
                table: "Cours");

            migrationBuilder.DropIndex(
                name: "IX_Cours_SpecialiteId",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "CycleId",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "NiveauId",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "SpecialiteId",
                table: "Cours");

            migrationBuilder.AddColumn<string>(
                name: "Filiere",
                table: "Cours",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
