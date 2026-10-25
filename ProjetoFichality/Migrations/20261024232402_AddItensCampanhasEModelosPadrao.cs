using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFichality.Migrations
{
    /// <inheritdoc />
    public partial class AddItensCampanhasEModelosPadrao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EhPadrao",
                table: "ModelosFicha",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Campanhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SistemaRpgId = table.Column<int>(type: "int", nullable: false),
                    MestreUsuarioId = table.Column<int>(type: "int", nullable: false),
                    HouseRulesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campanhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campanhas_SistemasRpg_SistemaRpgId",
                        column: x => x.SistemaRpgId,
                        principalTable: "SistemasRpg",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Campanhas_Usuarios_MestreUsuarioId",
                        column: x => x.MestreUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItensSistemaRpg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaRpgId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropriedadesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EfeitosJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Custo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Peso = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensSistemaRpg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensSistemaRpg_SistemasRpg_SistemaRpgId",
                        column: x => x.SistemaRpgId,
                        principalTable: "SistemasRpg",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampanhasFichas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampanhaId = table.Column<int>(type: "int", nullable: false),
                    FichaId = table.Column<int>(type: "int", nullable: false),
                    AdicionadaEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampanhasFichas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampanhasFichas_Campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "Campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampanhasFichas_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campanhas_MestreUsuarioId",
                table: "Campanhas",
                column: "MestreUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Campanhas_SistemaRpgId",
                table: "Campanhas",
                column: "SistemaRpgId");

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasFichas_CampanhaId_FichaId",
                table: "CampanhasFichas",
                columns: new[] { "CampanhaId", "FichaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasFichas_FichaId",
                table: "CampanhasFichas",
                column: "FichaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensSistemaRpg_SistemaRpgId",
                table: "ItensSistemaRpg",
                column: "SistemaRpgId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampanhasFichas");

            migrationBuilder.DropTable(
                name: "ItensSistemaRpg");

            migrationBuilder.DropTable(
                name: "Campanhas");

            migrationBuilder.DropColumn(
                name: "EhPadrao",
                table: "ModelosFicha");
        }
    }
}
