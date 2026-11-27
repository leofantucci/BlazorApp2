using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entidade = table.Column<string>(type: "text", nullable: false),
                    EntidadeId = table.Column<int>(type: "integer", nullable: false),
                    Operacao = table.Column<string>(type: "text", nullable: false),
                    DadosAntigos = table.Column<string>(type: "text", nullable: true),
                    DadosNovos = table.Column<string>(type: "text", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false),
                    Instagram = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoItem = table.Column<string>(type: "text", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Unidade_Medida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false),
                    Site = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Impressora",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kwh = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impressora", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoItem = table.Column<string>(type: "text", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    NomeItem = table.Column<string>(type: "text", nullable: false),
                    Operacao = table.Column<string>(type: "text", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    Unidade = table.Column<string>(type: "text", nullable: false),
                    Origem = table.Column<string>(type: "text", nullable: false),
                    ReferenciaId = table.Column<int>(type: "integer", nullable: true),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QRCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodigoPublico = table.Column<string>(type: "text", nullable: false),
                    CodigoAtivacaoHash = table.Column<string>(type: "text", nullable: true),
                    SenhaHash = table.Column<string>(type: "text", nullable: true),
                    CodigoAtivacao = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    LinkDestino = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_QRCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Data_Hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venda_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Embalagem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    largura = table.Column<double>(type: "double precision", nullable: false),
                    comprimento = table.Column<double>(type: "double precision", nullable: false),
                    altura = table.Column<double>(type: "double precision", nullable: false),
                    Preco = table.Column<double>(type: "double precision", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Unidade_Medida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Embalagem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Embalagem_Fornecedor_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedor",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MateriaPrima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Peso_compra = table.Column<int>(type: "integer", nullable: false),
                    Preco_compra = table.Column<double>(type: "double precision", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Unidade_Medida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MateriaPrima", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MateriaPrima_Fornecedor_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedor",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VendaEmbalagem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmbalagemId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    VendaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaEmbalagem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaEmbalagem_Embalagem_EmbalagemId",
                        column: x => x.EmbalagemId,
                        principalTable: "Embalagem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendaEmbalagem_Venda_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Venda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Peca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Peso_Impressao = table.Column<double>(type: "double precision", nullable: false),
                    Tempo_Impressao = table.Column<double>(type: "double precision", nullable: false),
                    largura = table.Column<double>(type: "double precision", nullable: false),
                    comprimento = table.Column<double>(type: "double precision", nullable: false),
                    altura = table.Column<double>(type: "double precision", nullable: false),
                    Materia_PrimaId = table.Column<int>(type: "integer", nullable: true),
                    ImpressoraId = table.Column<int>(type: "integer", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Unidade_Medida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Peca_Impressora_ImpressoraId",
                        column: x => x.ImpressoraId,
                        principalTable: "Impressora",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Peca_MateriaPrima_Materia_PrimaId",
                        column: x => x.Materia_PrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Produto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Materia_PrimaId = table.Column<int>(type: "integer", nullable: true),
                    ImpressoraId = table.Column<int>(type: "integer", nullable: true),
                    Peso_Impressao = table.Column<double>(type: "double precision", nullable: false),
                    Tempo_Impressao = table.Column<double>(type: "double precision", nullable: false),
                    largura = table.Column<double>(type: "double precision", nullable: false),
                    comprimento = table.Column<double>(type: "double precision", nullable: false),
                    altura = table.Column<double>(type: "double precision", nullable: false),
                    Valor_Venda = table.Column<double>(type: "double precision", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Unidade_Medida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produto_Impressora_ImpressoraId",
                        column: x => x.ImpressoraId,
                        principalTable: "Impressora",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Produto_MateriaPrima_Materia_PrimaId",
                        column: x => x.Materia_PrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProdutoPeca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoPeca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoPeca_Peca_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Peca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProdutoPeca_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendaItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    VendaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaItem_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendaItem_Venda_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Venda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Embalagem_FornecedorId",
                table: "Embalagem",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_TipoItem_ItemId",
                table: "Estoque",
                columns: new[] { "TipoItem", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MateriaPrima_FornecedorId",
                table: "MateriaPrima",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_DataHora",
                table: "MovimentacoesEstoque",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_Peca_ImpressoraId",
                table: "Peca",
                column: "ImpressoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Peca_Materia_PrimaId",
                table: "Peca",
                column: "Materia_PrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_ImpressoraId",
                table: "Produto",
                column: "ImpressoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_Materia_PrimaId",
                table: "Produto",
                column: "Materia_PrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoPeca_PecaId",
                table: "ProdutoPeca",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoPeca_ProdutoId_PecaId",
                table: "ProdutoPeca",
                columns: new[] { "ProdutoId", "PecaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRCode_CodigoPublico",
                table: "QRCode",
                column: "CodigoPublico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venda_ClienteId",
                table: "Venda",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaEmbalagem_EmbalagemId",
                table: "VendaEmbalagem",
                column: "EmbalagemId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaEmbalagem_VendaId",
                table: "VendaEmbalagem",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaItem_ProdutoId",
                table: "VendaItem",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaItem_VendaId",
                table: "VendaItem",
                column: "VendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "Estoque");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "ProdutoPeca");

            migrationBuilder.DropTable(
                name: "QRCode");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "VendaEmbalagem");

            migrationBuilder.DropTable(
                name: "VendaItem");

            migrationBuilder.DropTable(
                name: "Peca");

            migrationBuilder.DropTable(
                name: "Embalagem");

            migrationBuilder.DropTable(
                name: "Produto");

            migrationBuilder.DropTable(
                name: "Venda");

            migrationBuilder.DropTable(
                name: "Impressora");

            migrationBuilder.DropTable(
                name: "MateriaPrima");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "Fornecedor");
        }
    }
}
