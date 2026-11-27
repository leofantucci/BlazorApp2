using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class ClienteDadosFiscais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A base anterior guardava o endereço em colunas separadas e o tipo como inteiro.
            // Consolidamos o endereço e convertemos o tipo antes de remover as colunas antigas,
            // preservando os dados já existentes.
            migrationBuilder.Sql("""
                ALTER TABLE "Cliente" ADD COLUMN "Endereco" character varying(200);

                UPDATE "Cliente"
                SET "Endereco" = NULLIF(
                    TRIM(BOTH ', ' FROM CONCAT_WS(', ',
                        NULLIF(TRIM("Logradouro"), ''),
                        NULLIF(TRIM("Numero"), ''),
                        NULLIF(TRIM("Complemento"), ''),
                        NULLIF(TRIM("Bairro"), ''),
                        NULLIF(TRIM("Cidade"), ''),
                        NULLIF(TRIM("Estado"), ''),
                        NULLIF(TRIM("CEP"), '')
                    )),
                    '');

                ALTER TABLE "Cliente" ADD COLUMN "TipoCliente" character varying(2) NOT NULL DEFAULT 'PF';

                UPDATE "Cliente"
                SET "TipoCliente" = CASE WHEN "TipoPessoa" = 1 THEN 'PJ' ELSE 'PF' END;

                ALTER TABLE "Cliente" ALTER COLUMN "TipoCliente" DROP DEFAULT;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Cliente" ALTER COLUMN "Nome" TYPE character varying(100) USING LEFT("Nome", 100);
                ALTER TABLE "Cliente" ALTER COLUMN "NomeFantasia" TYPE character varying(100) USING LEFT("NomeFantasia", 100);
                ALTER TABLE "Cliente" ALTER COLUMN "CNPJ" TYPE character varying(18);
                ALTER TABLE "Cliente" ALTER COLUMN "Telefone" TYPE character varying(20) USING LEFT("Telefone", 20);
                ALTER TABLE "Cliente" ALTER COLUMN "Email" TYPE character varying(150);
                """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Cliente_CNPJ";
                DROP INDEX IF EXISTS "IX_Cliente_CPF";

                ALTER TABLE "Cliente"
                    DROP COLUMN IF EXISTS "Ativo",
                    DROP COLUMN IF EXISTS "Bairro",
                    DROP COLUMN IF EXISTS "CEP",
                    DROP COLUMN IF EXISTS "Cidade",
                    DROP COLUMN IF EXISTS "Complemento",
                    DROP COLUMN IF EXISTS "DataCadastro",
                    DROP COLUMN IF EXISTS "Estado",
                    DROP COLUMN IF EXISTS "Instagram",
                    DROP COLUMN IF EXISTS "Logradouro",
                    DROP COLUMN IF EXISTS "Numero",
                    DROP COLUMN IF EXISTS "Observacoes",
                    DROP COLUMN IF EXISTS "TipoPessoa";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Cliente" ADD COLUMN "Instagram" text NOT NULL DEFAULT '';
                ALTER TABLE "Cliente" ADD COLUMN "TipoPessoa" integer NOT NULL DEFAULT 0;
                ALTER TABLE "Cliente" ADD COLUMN "CEP" character varying(9);
                ALTER TABLE "Cliente" ADD COLUMN "Logradouro" character varying(150);
                ALTER TABLE "Cliente" ADD COLUMN "Numero" character varying(20);
                ALTER TABLE "Cliente" ADD COLUMN "Complemento" character varying(100);
                ALTER TABLE "Cliente" ADD COLUMN "Bairro" character varying(100);
                ALTER TABLE "Cliente" ADD COLUMN "Cidade" character varying(100);
                ALTER TABLE "Cliente" ADD COLUMN "Estado" character varying(2);
                ALTER TABLE "Cliente" ADD COLUMN "DataCadastro" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                ALTER TABLE "Cliente" ADD COLUMN "Ativo" boolean NOT NULL DEFAULT true;
                ALTER TABLE "Cliente" ADD COLUMN "Observacoes" text;

                UPDATE "Cliente"
                SET "TipoPessoa" = CASE WHEN "TipoCliente" = 'PJ' THEN 1 ELSE 0 END;

                ALTER TABLE "Cliente" ALTER COLUMN "Nome" TYPE character varying(150);
                ALTER TABLE "Cliente" ALTER COLUMN "NomeFantasia" TYPE character varying(150);
                ALTER TABLE "Cliente" ALTER COLUMN "CNPJ" TYPE character varying(14) USING LEFT("CNPJ", 14);
                ALTER TABLE "Cliente" ALTER COLUMN "Telefone" TYPE character varying(30);

                CREATE UNIQUE INDEX "IX_Cliente_CPF" ON "Cliente" ("CPF");
                CREATE UNIQUE INDEX "IX_Cliente_CNPJ" ON "Cliente" ("CNPJ");

                ALTER TABLE "Cliente" DROP COLUMN "Endereco";
                ALTER TABLE "Cliente" DROP COLUMN "TipoCliente";
                """);
        }
    }
}
