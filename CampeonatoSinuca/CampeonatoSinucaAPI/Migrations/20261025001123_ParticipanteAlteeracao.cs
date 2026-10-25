using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampeonatoSinucaAPI.Migrations
{
    /// <inheritdoc />
    public partial class ParticipanteAlteeracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sexo",
                table: "Participantes",
                newName: "Genero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Genero",
                table: "Participantes",
                newName: "Sexo");
        }
    }
}
