using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations;

public partial class ClientePessoaFisicaJuridica : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Nome", table: "Cliente", type: "character varying(150)", maxLength: 150,
            nullable: false, oldClrType: typeof(string), oldType: "character varying(100)", oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Email", table: "Cliente", type: "text", nullable: true,
            oldClrType: typeof(string), oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Telefone", table: "Cliente", type: "text", nullable: true,
            oldClrType: typeof(string), oldType: "text");

        migrationBuilder.AddColumn<int>(name: "TipoPessoa", table: "Cliente", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<string>(name: "NomeFantasia", table: "Cliente", type: "character varying(150)", maxLength: 150, nullable: true);
        migrationBuilder.AddColumn<string>(name: "CPF", table: "Cliente", type: "character varying(14)", maxLength: 14, nullable: true);
        migrationBuilder.AddColumn<string>(name: "CNPJ", table: "Cliente", type: "character varying(14)", maxLength: 14, nullable: true);
        migrationBuilder.AddColumn<string>(name: "CEP", table: "Cliente", type: "character varying(9)", maxLength: 9, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Logradouro", table: "Cliente", type: "character varying(150)", maxLength: 150, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Numero", table: "Cliente", type: "character varying(20)", maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Complemento", table: "Cliente", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Bairro", table: "Cliente", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Cidade", table: "Cliente", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Estado", table: "Cliente", type: "character varying(2)", maxLength: 2, nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "DataCadastro", table: "Cliente", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP");
        migrationBuilder.AddColumn<bool>(name: "Ativo", table: "Cliente", type: "boolean", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<string>(name: "Observacoes", table: "Cliente", type: "text", nullable: true);

        migrationBuilder.CreateIndex(name: "IX_Cliente_CPF", table: "Cliente", column: "CPF", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Cliente_CNPJ", table: "Cliente", column: "CNPJ", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Cliente_CPF", table: "Cliente");
        migrationBuilder.DropIndex(name: "IX_Cliente_CNPJ", table: "Cliente");
        migrationBuilder.DropColumn(name: "TipoPessoa", table: "Cliente");
        migrationBuilder.DropColumn(name: "NomeFantasia", table: "Cliente");
        migrationBuilder.DropColumn(name: "CPF", table: "Cliente");
        migrationBuilder.DropColumn(name: "CNPJ", table: "Cliente");
        migrationBuilder.DropColumn(name: "CEP", table: "Cliente");
        migrationBuilder.DropColumn(name: "Logradouro", table: "Cliente");
        migrationBuilder.DropColumn(name: "Numero", table: "Cliente");
        migrationBuilder.DropColumn(name: "Complemento", table: "Cliente");
        migrationBuilder.DropColumn(name: "Bairro", table: "Cliente");
        migrationBuilder.DropColumn(name: "Cidade", table: "Cliente");
        migrationBuilder.DropColumn(name: "Estado", table: "Cliente");
        migrationBuilder.DropColumn(name: "DataCadastro", table: "Cliente");
        migrationBuilder.DropColumn(name: "Ativo", table: "Cliente");
        migrationBuilder.DropColumn(name: "Observacoes", table: "Cliente");
        migrationBuilder.AlterColumn<string>(name: "Nome", table: "Cliente", type: "character varying(100)", maxLength: 100, nullable: false, oldClrType: typeof(string), oldType: "character varying(150)", oldMaxLength: 150);
        migrationBuilder.AlterColumn<string>(name: "Email", table: "Cliente", type: "text", nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);
        migrationBuilder.AlterColumn<string>(name: "Telefone", table: "Cliente", type: "text", nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);
    }
}
