using System;
using System.ComponentModel.DataAnnotations;

namespace TechToysDominio
{
    public class Item
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public String Nome { get; set; } = string.Empty;
        public String Unidade_Medida { get; set; } = string.Empty;
    }
}