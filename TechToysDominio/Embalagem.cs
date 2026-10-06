using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TechToysDominio
{
    public class Embalagem
    {
        public int Id { get; set; }
        public String Nome { get; set; } 
        public String Tipo { get; set; }
        public double largura { get; set; }
        public double comprimento { get; set; }
        public double altura { get; set; }
        public double Preco { get; set; }
        public int Estoque { get; set; }
        public Fornecedor Fornecedor { get; set; }
        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Tipo: {Tipo} | Largura: {largura} | comprimento: {comprimento} | altura: {altura} | Preço: R${Preco} | Estoque: {Estoque}";
        }
        public Embalagem()
        {
            Estoque = 0;
        }

        public double Calcular_Valor_Estoque()
        {
            return Convert.ToDouble(Estoque)* Preco;
        }
    }
}
