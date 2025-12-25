using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGSA_Licence3.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enseignant_Specialite",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Filiere",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Niveau",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Specialite",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CycleId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NiveauId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialiteId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CycleId",
                table: "Seances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NiveauId",
                table: "Seances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialiteId",
                table: "Seances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomCycle = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cycles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Niveaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomNiveau = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Niveaux", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Specialites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomSpecialite = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialites", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CycleId",
                table: "Users",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NiveauId",
                table: "Users",
                column: "NiveauId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SpecialiteId",
                table: "Users",
                column: "SpecialiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Seances_CycleId",
                table: "Seances",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Seances_NiveauId",
                table: "Seances",
                column: "NiveauId");

            migrationBuilder.CreateIndex(
                name: "IX_Seances_SpecialiteId",
                table: "Seances",
                column: "SpecialiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seances_Cycles_CycleId",
                table: "Seances",
                column: "CycleId",
                principalTable: "Cycles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Seances_Niveaux_NiveauId",
                table: "Seances",
                column: "NiveauId",
                principalTable: "Niveaux",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Seances_Specialites_SpecialiteId",
                table: "Seances",
                column: "SpecialiteId",
                principalTable: "Specialites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Cycles_CycleId",
                table: "Users",
                column: "CycleId",
                principalTable: "Cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Niveaux_NiveauId",
                table: "Users",
                column: "NiveauId",
                principalTable: "Niveaux",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Specialites_SpecialiteId",
                table: "Users",
                column: "SpecialiteId",
                principalTable: "Specialites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seances_Cycles_CycleId",
                table: "Seances");

            migrationBuilder.DropForeignKey(
                name: "FK_Seances_Niveaux_NiveauId",
                table: "Seances");

            migrationBuilder.DropForeignKey(
                name: "FK_Seances_Specialites_SpecialiteId",
                table: "Seances");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Cycles_CycleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Niveaux_NiveauId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Specialites_SpecialiteId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Cycles");

            migrationBuilder.DropTable(
                name: "Niveaux");

            migrationBuilder.DropTable(
                name: "Specialites");

            migrationBuilder.DropIndex(
                name: "IX_Users_CycleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NiveauId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SpecialiteId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Seances_CycleId",
                table: "Seances");

            migrationBuilder.DropIndex(
                name: "IX_Seances_NiveauId",
                table: "Seances");

            migrationBuilder.DropIndex(
                name: "IX_Seances_SpecialiteId",
                table: "Seances");

            migrationBuilder.DropColumn(
                name: "CycleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NiveauId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpecialiteId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CycleId",
                table: "Seances");

            migrationBuilder.DropColumn(
                name: "NiveauId",
                table: "Seances");

            migrationBuilder.DropColumn(
                name: "SpecialiteId",
                table: "Seances");

            migrationBuilder.AlterColumn<string>(
                name: "Specialite",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Enseignant_Specialite",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Filiere",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Niveau",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
