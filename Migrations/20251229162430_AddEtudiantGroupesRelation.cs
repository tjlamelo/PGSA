using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGSA_Licence3.Migrations
{
    /// <inheritdoc />
    public partial class AddEtudiantGroupesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Groupes_GroupeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GroupeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GroupeId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "EtudiantGroupes",
                columns: table => new
                {
                    EtudiantsId = table.Column<int>(type: "int", nullable: false),
                    GroupesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtudiantGroupes", x => new { x.EtudiantsId, x.GroupesId });
                    table.ForeignKey(
                        name: "FK_EtudiantGroupes_Groupes_GroupesId",
                        column: x => x.GroupesId,
                        principalTable: "Groupes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtudiantGroupes_Users_EtudiantsId",
                        column: x => x.EtudiantsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EtudiantGroupes_GroupesId",
                table: "EtudiantGroupes",
                column: "GroupesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtudiantGroupes");

            migrationBuilder.AddColumn<int>(
                name: "GroupeId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupeId",
                table: "Users",
                column: "GroupeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Groupes_GroupeId",
                table: "Users",
                column: "GroupeId",
                principalTable: "Groupes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
