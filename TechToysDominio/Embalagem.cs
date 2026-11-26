using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TechToysDominio
{
    public class Embalagem : Item
    {
        public String Tipo { get; set; } = string.Empty;
        public double largura { get; set; }
        public double comprimento { get; set; }
        public double altura { get; set; }
        public double Preco { get; set; }
        public Fornecedor? Fornecedor { get; set; }
        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Tipo: {Tipo} | Largura: {largura} | comprimento: {comprimento} | altura: {altura} | Preço: R${Preco}";
        }
        public Embalagem()
        {
            Unidade_Medida = "UN";
        }
    }
}
