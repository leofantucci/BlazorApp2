using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Cliente
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public String Nome { get; set; } 
        public String Email { get; set; }
        public String Telefone { get; set; }
        public String Instagram { get; set; }
        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Telefone: {Telefone} | Instagram: {Instagram}";
        }
    }
}
