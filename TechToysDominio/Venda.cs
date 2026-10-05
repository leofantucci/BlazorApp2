using System;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Venda
    {
        public int Id { get; set; }
        public DateTime Data_Hora { get; set; }
        public Cliente Cliente { get; set; }
        public Produto Produto { get; set; }
        public int Quantidade { get; set; }
        public Embalagem Embalagem { get; set; }
        public int Quantidade_Embalagem { get; set; }
        public Venda()
        {
            Quantidade = 1;
            Quantidade_Embalagem = 1;
        }
        public override string ToString()
        {
            return $"[ID: {Id}] Data/Hora: ({Data_Hora})] {Cliente.Nome} [{Cliente.Id}] | {Quantidade}x {Produto.Nome} [{Produto.Id}] | {Quantidade_Embalagem}x {Embalagem.Nome} [{Embalagem.Id}] ";
        }
        public string MaisDetalhes()
        {
             return $"[Mais Detalhes - ID: {Id}] Data/Hora: {Data_Hora} | Cliente: {Cliente.Nome} [{Cliente.Id}] | Produto: {Produto.Nome} [{Produto.Id}] | Quantidade: {Quantidade} | Embalagem: {Quantidade_Embalagem}x {Embalagem.Nome} [{Embalagem.Id}] |\nCusto Total: R${Calcular_Gasto_Total()} | Lucro Bruto: R${Calcular_Lucro_Bruto()} | Lucro Líquido: R${Calcular_Lucro_Liquido()}\n";
        }

        public double Calcular_Lucro_Bruto()
        {
            return Quantidade * Produto.Valor_Venda;
        }
        public void Calcular_Embalagens()
        {
            double[] produto =
            {
                Produto.largura,
                Produto.comprimento,
                Produto.altura
            };

            double[] embalagem =
            {
                Embalagem.largura,
                Embalagem.comprimento,
                Embalagem.altura
            };

            // Ordena as dimensões para permitir a rotação
            Array.Sort(produto);
            Array.Sort(embalagem);

            // Quantos produtos cabem em uma embalagem
            int produtosPorEmbalagem =
                (int)(embalagem[0] / produto[0]) *
                (int)(embalagem[1] / produto[1]) *
                (int)(embalagem[2] / produto[2]);

            if (produtosPorEmbalagem <= 0)
            {
                Quantidade_Embalagem = 0;
                return;
            }

            // Calcula quantas embalagens são necessárias
            Quantidade_Embalagem =
                (int)Math.Ceiling((double)Quantidade / produtosPorEmbalagem);
        }

        public double Calcular_Gasto_Total()
        {
            return (Produto.Calcular_Custo_Total() * Quantidade) + (Embalagem.Preco * Quantidade_Embalagem);
        }
        public double Calcular_Lucro_Liquido()
        {
            return (Produto.Valor_Venda * Quantidade) - Calcular_Gasto_Total();

        }
    }
}