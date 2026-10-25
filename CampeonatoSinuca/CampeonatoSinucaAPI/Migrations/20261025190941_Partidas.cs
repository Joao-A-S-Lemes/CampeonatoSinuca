using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampeonatoSinucaAPI.Migrations
{
    /// <inheritdoc />
    public partial class Partidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModalidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Participante1Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Participante2Id = table.Column<int>(type: "INTEGER", nullable: false),
                    VencedorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partidas_Modalidades_ModalidadeId",
                        column: x => x.ModalidadeId,
                        principalTable: "Modalidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Partidas_Participantes_Participante1Id",
                        column: x => x.Participante1Id,
                        principalTable: "Participantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Participantes_Participante2Id",
                        column: x => x.Participante2Id,
                        principalTable: "Participantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Participantes_VencedorId",
                        column: x => x.VencedorId,
                        principalTable: "Participantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_ModalidadeId",
                table: "Partidas",
                column: "ModalidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_Participante1Id",
                table: "Partidas",
                column: "Participante1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_Participante2Id",
                table: "Partidas",
                column: "Participante2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_VencedorId",
                table: "Partidas",
                column: "VencedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partidas");
        }
    }
}
