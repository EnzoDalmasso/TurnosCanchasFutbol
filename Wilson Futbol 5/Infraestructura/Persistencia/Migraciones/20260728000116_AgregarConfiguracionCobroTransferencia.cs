using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionCobroTransferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CobraReservaPorTransferencia",
                table: "ConfiguracionesNegocio",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ConfiguracionesNegocio",
                keyColumn: "Id",
                keyValue: 1,
                column: "CobraReservaPorTransferencia",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CobraReservaPorTransferencia",
                table: "ConfiguracionesNegocio");
        }
    }
}
