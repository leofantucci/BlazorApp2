using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Produto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public String Nome { get; set; }
        public double Peso_Impressao { get; set; } //Gramas
        public double Tempo_Impressao { get; set; } //Minutos
        public double largura { get; set; }
        public double comprimento { get; set; }
        public double altura { get; set; }
        public MateriaPrima Materia_Prima { get; set; }
        public Impressora Impressora { get; set; }
        public double Valor_Venda { get; set;  }
        public int Estoque { get; set; }
        public bool Ativo { get; set; }
        public override string ToString()
        {
            return $"[ID: {Id}] Nome: {Nome} | Peso da impressão: {Peso_Impressao} | Tempo da Impressão: {Tempo_Impressao} | Dimensões(L,C,A): ({largura},{comprimento},{altura}) | Matéria-prima: {Materia_Prima.Nome} [{Materia_Prima.Id}] | Impressora: {Impressora.Modelo} [{Impressora.Id}] | Valor Venda: R${Valor_Venda} | Estoque: {Estoque} | Ativo: {Ativo}"; 
        }
        public Produto()
        {
            Estoque = 0;
            Ativo = true;
        }

        public double Calcular_Custo_Materia_Prima()
        {
            return Peso_Impressao * Materia_Prima.Calcular_Preco_Grama();
        }
        public double Calcular_Custo_Impressao()
        {
            return Impressora.Calcular_Custo_Impressao(Tempo_Impressao);
        }
        public double Calcular_Custo_Total()
        {
            return Calcular_Custo_Impressao() + Calcular_Custo_Materia_Prima();
        }
        public double Lucro_Liquido()
        {
            return Valor_Venda - Calcular_Custo_Total();
        }
        public void Estoque_Adicionar(int quantidade)
        {
            Estoque = Estoque + quantidade;
        }
        public int Estoque_Visualizar()
        {
            return Estoque;
        }
        public void Estoque_Remover(int quantidade)
        {
            if (Estoque - quantidade >= 0)
            {
                Estoque = Estoque - quantidade;
            }
            else
            {
                Console.WriteLine("Estoque vai ficar negativo");
            }
        }
        
        public double Calcular_Valor_Estoque()
        {
            return Estoque * Lucro_Liquido();
        }
    }
}
