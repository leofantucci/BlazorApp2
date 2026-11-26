using System;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class MateriaPrima : Item // Padrão de peso - Gramas (g)
    {
        public int Peso_compra { get; set; }
        public double Preco_compra { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        public MateriaPrima()
        {
            Unidade_Medida = "g";
        }

        public MateriaPrima(int pesoCompra) : this()
        {
            Peso_compra = pesoCompra;
        }

        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Peso de compra: {Peso_compra} | Preço de compra: {Preco_compra} | Fornecedor: {Fornecedor?.Nome ?? "Nenhum"} [{Fornecedor?.Id.ToString() ?? "-"}]";
        }

        public double Calcular_Preco_Grama()
        {
            return Peso_compra > 0 ? Preco_compra / Peso_compra : 0;
        }
    }
}
