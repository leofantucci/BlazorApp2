using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repositorio.Migrations;

[DbContext(typeof(MyDBContext))]
[Migration("20261126010000_ComercialOnline")]
public partial class ComercialOnline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProdutoComercial",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Foto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Disponivel = table.Column<bool>(type: "boolean", nullable: false),
                ProdutoBaseId = table.Column<int>(type: "integer", nullable: true),
                DataCriacaoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DataAtualizacaoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProdutoComercial", x => x.Id);
                table.ForeignKey("FK_ProdutoComercial_Produto_ProdutoBaseId", x => x.ProdutoBaseId, "Produto", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "VendaComercial",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClienteId = table.Column<int>(type: "integer", nullable: false),
                Data_Hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                EnderecoEntrega = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                CidadeEntrega = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                EstadoEntrega = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                CEPEntrega = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VendaComercial", x => x.Id);
                table.ForeignKey("FK_VendaComercial_Cliente_ClienteId", x => x.ClienteId, "Cliente", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Pagamento",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                VendaComercialId = table.Column<int>(type: "integer", nullable: false),
                ClienteId = table.Column<int>(type: "integer", nullable: false),
                DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Metodo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                Provedor = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                TransacaoExternaId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                PayloadRetorno = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                DataConfirmacaoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pagamento", x => x.Id);
                table.ForeignKey("FK_Pagamento_Cliente_ClienteId", x => x.ClienteId, "Cliente", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Pagamento_VendaComercial_VendaComercialId", x => x.VendaComercialId, "VendaComercial", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VendaComercialItem",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                VendaComercialId = table.Column<int>(type: "integer", nullable: false),
                ProdutoComercialId = table.Column<int>(type: "integer", nullable: false),
                Quantidade = table.Column<int>(type: "integer", nullable: false),
                PrecoUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                PagamentoId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VendaComercialItem", x => x.Id);
                table.ForeignKey("FK_VendaComercialItem_Pagamento_PagamentoId", x => x.PagamentoId, "Pagamento", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_VendaComercialItem_ProdutoComercial_ProdutoComercialId", x => x.ProdutoComercialId, "ProdutoComercial", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_VendaComercialItem_VendaComercial_VendaComercialId", x => x.VendaComercialId, "VendaComercial", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ControleComercial",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                VendaComercialId = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                DataHoraUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                Responsavel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ControleComercial", x => x.Id);
                table.ForeignKey("FK_ControleComercial_VendaComercial_VendaComercialId", x => x.VendaComercialId, "VendaComercial", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddColumn<int>(name: "VendaComercialItemId", table: "PlacaNFC", type: "integer", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_ProdutoComercial_Disponivel", table: "ProdutoComercial", column: "Disponivel");
        migrationBuilder.CreateIndex(name: "IX_ProdutoComercial_ProdutoBaseId", table: "ProdutoComercial", column: "ProdutoBaseId");
        migrationBuilder.CreateIndex(name: "IX_VendaComercial_ClienteId", table: "VendaComercial", column: "ClienteId");
        migrationBuilder.CreateIndex(name: "IX_VendaComercial_Data_Hora", table: "VendaComercial", column: "Data_Hora");
        migrationBuilder.CreateIndex(name: "IX_VendaComercial_Status", table: "VendaComercial", column: "Status");
        migrationBuilder.CreateIndex(name: "IX_Pagamento_ClienteId", table: "Pagamento", column: "ClienteId");
        migrationBuilder.CreateIndex(name: "IX_Pagamento_VendaComercialId", table: "Pagamento", column: "VendaComercialId");
        migrationBuilder.CreateIndex(name: "IX_Pagamento_TransacaoExternaId", table: "Pagamento", column: "TransacaoExternaId", unique: true, filter: "\"TransacaoExternaId\" IS NOT NULL");
        migrationBuilder.CreateIndex(name: "IX_VendaComercialItem_PagamentoId", table: "VendaComercialItem", column: "PagamentoId");
        migrationBuilder.CreateIndex(name: "IX_VendaComercialItem_ProdutoComercialId", table: "VendaComercialItem", column: "ProdutoComercialId");
        migrationBuilder.CreateIndex(name: "IX_VendaComercialItem_VendaComercialId", table: "VendaComercialItem", column: "VendaComercialId");
        migrationBuilder.CreateIndex(name: "IX_ControleComercial_VendaComercialId_DataHoraUtc", table: "ControleComercial", columns: new[] { "VendaComercialId", "DataHoraUtc" });
        migrationBuilder.CreateIndex(name: "IX_PlacaNFC_VendaComercialItemId", table: "PlacaNFC", column: "VendaComercialItemId");
        migrationBuilder.AddForeignKey(name: "FK_PlacaNFC_VendaComercialItem_VendaComercialItemId", table: "PlacaNFC", column: "VendaComercialItemId", principalTable: "VendaComercialItem", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_PlacaNFC_VendaComercialItem_VendaComercialItemId", table: "PlacaNFC");
        migrationBuilder.DropIndex(name: "IX_PlacaNFC_VendaComercialItemId", table: "PlacaNFC");
        migrationBuilder.DropColumn(name: "VendaComercialItemId", table: "PlacaNFC");
        migrationBuilder.DropTable(name: "ControleComercial");
        migrationBuilder.DropTable(name: "VendaComercialItem");
        migrationBuilder.DropTable(name: "Pagamento");
        migrationBuilder.DropTable(name: "ProdutoComercial");
        migrationBuilder.DropTable(name: "VendaComercial");
    }
}
