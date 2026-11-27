using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Repositorio.Migrations;

[DbContext(typeof(MyDBContext))]
[Migration("20261126120000_AsaasPix")]
public partial class AsaasPix : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AsaasCustomerId", table: "Cliente", type: "character varying(80)", maxLength: 80, nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StatusProvedor", table: "Pagamento", type: "character varying(80)", maxLength: 80, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "PixQrCodeBase64", table: "Pagamento", type: "character varying(12000)", maxLength: 12000, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "PixCopiaECola", table: "Pagamento", type: "character varying(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<DateTime>(
            name: "PixExpiracaoUtc", table: "Pagamento", type: "timestamp with time zone", nullable: true);

        migrationBuilder.CreateTable(
            name: "AsaasWebhookEventos",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer")
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EventoExternoId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                TipoEvento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PagamentoExternoId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                RecebidoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Processado = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AsaasWebhookEventos", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_AsaasWebhookEventos_EventoExternoId",
            table: "AsaasWebhookEventos",
            column: "EventoExternoId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AsaasWebhookEventos");
        migrationBuilder.DropColumn(name: "AsaasCustomerId", table: "Cliente");
        migrationBuilder.DropColumn(name: "StatusProvedor", table: "Pagamento");
        migrationBuilder.DropColumn(name: "PixQrCodeBase64", table: "Pagamento");
        migrationBuilder.DropColumn(name: "PixCopiaECola", table: "Pagamento");
        migrationBuilder.DropColumn(name: "PixExpiracaoUtc", table: "Pagamento");
    }
}
