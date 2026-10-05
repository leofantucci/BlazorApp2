using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Fornecedor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public String Nome { get; set; }
        public String Email { get; set; }
        public String Telefone { get; set; }
        public String Site { get; set; }
        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Email: {Email} | Telefone: {Telefone} | Site: {Site}";
        }
    }
}
