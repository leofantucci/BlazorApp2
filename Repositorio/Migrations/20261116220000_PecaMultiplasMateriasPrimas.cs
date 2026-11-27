using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations;

public partial class PecaMultiplasMateriasPrimas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PecaMateriaPrima",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PecaId = table.Column<int>(type: "integer", nullable: false),
                MateriaPrimaId = table.Column<int>(type: "integer", nullable: false),
                Peso = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PecaMateriaPrima", x => x.Id);
                table.ForeignKey("FK_PecaMateriaPrima_MateriaPrima_MateriaPrimaId", x => x.MateriaPrimaId, "MateriaPrima", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_PecaMateriaPrima_Peca_PecaId", x => x.PecaId, "Peca", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_PecaMateriaPrima_MateriaPrimaId", "PecaMateriaPrima", "MateriaPrimaId");
        migrationBuilder.CreateIndex("IX_PecaMateriaPrima_PecaId_MateriaPrimaId", "PecaMateriaPrima", new[] { "PecaId", "MateriaPrimaId" }, unique: true);

        // Migra os cadastros antigos: a matéria-prima única recebe todo o peso da peça.
        migrationBuilder.Sql(@"INSERT INTO ""PecaMateriaPrima"" (""PecaId"", ""MateriaPrimaId"", ""Peso"")
            SELECT ""Id"", ""Materia_PrimaId"", ""Peso_Impressao"" FROM ""Peca""
            WHERE ""Materia_PrimaId"" IS NOT NULL AND ""Peso_Impressao"" > 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "PecaMateriaPrima");
}
