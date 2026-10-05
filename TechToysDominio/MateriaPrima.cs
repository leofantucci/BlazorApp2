using System;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class MateriaPrima // Padrão de peso - Gramas (g)
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; }
        public int Peso_compra { get; set; }
        public double Preco_compra { get; set; }
        public Fornecedor Fornecedor { get; set; }
        public int Estoque { get; set; }

        public MateriaPrima()
        {
        }

        public MateriaPrima(int pesoCompra)
        {
            Peso_compra = pesoCompra;
            Estoque = pesoCompra;
        }

        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Peso de compra: {Peso_compra} | Preço de compra: {Preco_compra} | Fornecedor: {Fornecedor?.Nome ?? "Nenhum"} [{Fornecedor?.Id.ToString() ?? "-"}] | Estoque: {Estoque}";
        }

        public double Calcular_Preco_Grama()
        {
            return Preco_compra / Peso_compra;
        }

        public void Estoque_Adicionar(int gramas)
        {
            Estoque = Estoque + gramas;
        }

        public int Estoque_Visualizar()
        {
            return Estoque;
        }

        public void Estoque_Remover(int gramas)
        {
            if (Estoque - gramas >= 0)
            {
                Estoque = Estoque - gramas;
            }
            else
            {
                Console.WriteLine("Estoque vai ficar negativo");
            }
        }

        public double Calcular_Valor_Estoque()
        {
            return Estoque * Calcular_Preco_Grama();
        }
    }
}