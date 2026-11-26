using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public enum TipoCliente
    {
        PF = 0,
        PJ = 1
    }

    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NomeFantasia { get; set; }

        [Required(ErrorMessage = "O tipo de cliente é obrigatório.")]
        public TipoCliente TipoCliente { get; set; } = TipoCliente.PF;

        [StringLength(14)]
        public string? CPF { get; set; }

        [StringLength(18)]
        public string? CNPJ { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }

        [StringLength(12)] public string? Numero { get; set; }
        [StringLength(100)] public string? Complemento { get; set; }
        [StringLength(100)] public string? Bairro { get; set; }
        [StringLength(100)] public string? Cidade { get; set; }
        [StringLength(2)] public string? Estado { get; set; }
        [StringLength(20)] public string? CEP { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string? Email { get; set; }

        /// <summary>
        /// Documento fiscal de acordo com o tipo do cliente (CPF para PF, CNPJ para PJ).
        /// </summary>
        public string Documento => TipoCliente == TipoCliente.PF
            ? CPF ?? string.Empty
            : CNPJ ?? string.Empty;

        public override string ToString()
        {
            var documento = string.IsNullOrWhiteSpace(Documento) ? "não informado" : Documento;
            return $"[ID: {Id}] Nome: {Nome} | Tipo: {TipoCliente} | Documento: {documento} | Telefone: {Telefone ?? "não informado"}";
        }
    }
}
