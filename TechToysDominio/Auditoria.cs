namespace TechToysDominio
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string Entidade { get; set; } = string.Empty;   // "Cliente", "Produto", ...
        public int EntidadeId { get; set; }                    // ID do registro alterado
        public string Operacao { get; set; } = string.Empty;   // "Adicionado", "Editado", "Removido"
        public string? DadosAntigos { get; set; }              // JSON do estado anterior
        public string? DadosNovos { get; set; }                // JSON do estado novo
        public string Usuario { get; set; } = string.Empty;    // Nome do usuário logado
        public DateTime DataHora { get; set; } = DateTime.Now;
    }
}