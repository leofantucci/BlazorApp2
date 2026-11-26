using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class PlacasNFC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlacaNFC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodigoPublico = table.Column<string>(type: "text", nullable: false),
                    CodigoAtivacaoHash = table.Column<string>(type: "text", nullable: true),
                    SenhaHash = table.Column<string>(type: "text", nullable: true),
                    CodigoAtivacao = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    ProdutoId = table.Column<int>(type: "integer", nullable: true),
                    QRCodeId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacaoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtivacaoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataAtualizacaoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimoAcessoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalAcessos = table.Column<int>(type: "integer", nullable: false),
                    TentativasSenha = table.Column<int>(type: "integer", nullable: false),
                    BloqueadoAteUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacaNFC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacaNFC_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlacaNFC_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlacaNFC_QRCode_QRCodeId",
                        column: x => x.QRCodeId,
                        principalTable: "QRCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlacaNFCCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlacaNFCId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Icone = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacaNFCCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacaNFCCard_PlacaNFC_PlacaNFCId",
                        column: x => x.PlacaNFCId,
                        principalTable: "PlacaNFC",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacaNFC_ClienteId",
                table: "PlacaNFC",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacaNFC_CodigoPublico",
                table: "PlacaNFC",
                column: "CodigoPublico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacaNFC_ProdutoId",
                table: "PlacaNFC",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacaNFC_QRCodeId",
                table: "PlacaNFC",
                column: "QRCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacaNFCCard_PlacaNFCId_Ordem",
                table: "PlacaNFCCard",
                columns: new[] { "PlacaNFCId", "Ordem" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacaNFCCard");

            migrationBuilder.DropTable(
                name: "PlacaNFC");
        }
    }
}
