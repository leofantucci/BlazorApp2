using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Empresa
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public String Nome { get; set; } = string.Empty;
        public String CNPJ { get; set; } = string.Empty;
        public String CPF { get; set; } = string.Empty;
        public String Nome_Fantasia { get; set; } = string.Empty;
        public String Email { get; set; } = string.Empty;
        public String Telefone { get; set; } = string.Empty;
        public String Endereco { get; set; } = string.Empty;
    }
}
