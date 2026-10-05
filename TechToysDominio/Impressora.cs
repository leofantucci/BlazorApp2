using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Impressora
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public String Nome { get; set; }
        
        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [StringLength(100)]
        public String Modelo { get; set; }
        public double Kwh { get; set; }= 0.15;
        [Required(ErrorMessage = "O custo é obrigatório.")]
        static public double Custo_Reais_Kwh = 0.789;
        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Modelo: {Modelo} | Gasto: {Kwh}kwh | Custo (1kwh): R${Custo_Reais_Kwh}";
        }
        public double Calcular_Consumo_Impressao(double minutos)
        {
            return (Kwh / 60) * minutos;
        }
        public double Calcular_Custo_Impressao(double minutos)
        {
            return Calcular_Consumo_Impressao(minutos) * Custo_Reais_Kwh;
        }

    }
}
