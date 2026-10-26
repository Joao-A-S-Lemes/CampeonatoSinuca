using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampeonatoSinucaAPI.Migrations
{
    /// <inheritdoc />
    public partial class PartidaFaseCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Partidas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fase",
                table: "Partidas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Partidas");

            migrationBuilder.DropColumn(
                name: "Fase",
                table: "Partidas");
        }
    }
}
