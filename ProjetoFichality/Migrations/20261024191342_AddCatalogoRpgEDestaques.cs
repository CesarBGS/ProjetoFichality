using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFichality.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoRpgEDestaques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModeloFichaId",
                table: "Fichas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SistemaRpgId",
                table: "Fichas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SistemasRpg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegrasBasicas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Versao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CriadorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Publico = table.Column<bool>(type: "bit", nullable: false),
                    PermiteCopia = table.Column<bool>(type: "bit", nullable: false),
                    SistemaOrigemId = table.Column<int>(type: "int", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemasRpg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SistemasRpg_SistemasRpg_SistemaOrigemId",
                        column: x => x.SistemaOrigemId,
                        principalTable: "SistemasRpg",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SistemasRpg_Usuarios_CriadorUsuarioId",
                        column: x => x.CriadorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ModelosFicha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstruturaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SistemaRpgId = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosFicha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelosFicha_SistemasRpg_SistemaRpgId",
                        column: x => x.SistemaRpgId,
                        principalTable: "SistemasRpg",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Destaques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Resumo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SistemaRpgId = table.Column<int>(type: "int", nullable: true),
                    ModeloFichaId = table.Column<int>(type: "int", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destaques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Destaques_ModelosFicha_ModeloFichaId",
                        column: x => x.ModeloFichaId,
                        principalTable: "ModelosFicha",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Destaques_SistemasRpg_SistemaRpgId",
                        column: x => x.SistemaRpgId,
                        principalTable: "SistemasRpg",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fichas_ModeloFichaId",
                table: "Fichas",
                column: "ModeloFichaId");

            migrationBuilder.CreateIndex(
                name: "IX_Fichas_SistemaRpgId",
                table: "Fichas",
                column: "SistemaRpgId");

            migrationBuilder.CreateIndex(
                name: "IX_Destaques_ModeloFichaId",
                table: "Destaques",
                column: "ModeloFichaId");

            migrationBuilder.CreateIndex(
                name: "IX_Destaques_SistemaRpgId",
                table: "Destaques",
                column: "SistemaRpgId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelosFicha_SistemaRpgId",
                table: "ModelosFicha",
                column: "SistemaRpgId");

            migrationBuilder.CreateIndex(
                name: "IX_SistemasRpg_CriadorUsuarioId",
                table: "SistemasRpg",
                column: "CriadorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SistemasRpg_SistemaOrigemId",
                table: "SistemasRpg",
                column: "SistemaOrigemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fichas_ModelosFicha_ModeloFichaId",
                table: "Fichas",
                column: "ModeloFichaId",
                principalTable: "ModelosFicha",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Fichas_SistemasRpg_SistemaRpgId",
                table: "Fichas",
                column: "SistemaRpgId",
                principalTable: "SistemasRpg",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fichas_ModelosFicha_ModeloFichaId",
                table: "Fichas");

            migrationBuilder.DropForeignKey(
                name: "FK_Fichas_SistemasRpg_SistemaRpgId",
                table: "Fichas");

            migrationBuilder.DropTable(
                name: "Destaques");

            migrationBuilder.DropTable(
                name: "ModelosFicha");

            migrationBuilder.DropTable(
                name: "SistemasRpg");

            migrationBuilder.DropIndex(
                name: "IX_Fichas_ModeloFichaId",
                table: "Fichas");

            migrationBuilder.DropIndex(
                name: "IX_Fichas_SistemaRpgId",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "ModeloFichaId",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "SistemaRpgId",
                table: "Fichas");
        }
    }
}
