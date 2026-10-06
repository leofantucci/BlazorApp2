using System;
using System.ComponentModel.DataAnnotations;

namespace TechToysDominio
{
    public class Venda
    {
        public int Id { get; set; }
        public DateTime Data_Hora { get; set; }

        // Chaves estrangeiras explícitas para o EF Core
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int ProdutoId { get; set; }
        public Produto Produto { get; set; } = null!;

        public int Quantidade { get; set; }

        public int? EmbalagemId { get; set; }
        public Embalagem? Embalagem { get; set; }
        public int? Quantidade_Embalagem { get; set; }

        public Venda()
        {
            Quantidade = 1;
            Quantidade_Embalagem = 0;
            Data_Hora = DateTime.UtcNow;
        }

        public override string ToString()
        {
            var embalagemInfo = Embalagem != null 
                ? $" | {Quantidade_Embalagem ?? 0}x {Embalagem.Nome} [{Embalagem.Id}]" 
                : " | Sem embalagem";

            return $"[ID: {Id}] Data/Hora: ({Data_Hora}) | {Cliente?.Nome} [{Cliente?.Id}] | {Quantidade}x {Produto?.Nome} [{Produto?.Id}]{embalagemInfo}";
        }

        public string MaisDetalhes()
        {
            var embalagemInfo = Embalagem != null 
                ? $"{Quantidade_Embalagem ?? 0}x {Embalagem.Nome} [{Embalagem.Id}]" 
                : "Nenhuma";

            return $"[Mais Detalhes - ID: {Id}] Data/Hora: {Data_Hora} | Cliente: {Cliente?.Nome} [{Cliente?.Id}] | " +
                   $"Produto: {Produto?.Nome} [{Produto?.Id}] | Quantidade: {Quantidade} | Embalagem: {embalagemInfo} |\n" +
                   $"Custo Total: R${Calcular_Gasto_Total():F2} | Lucro Bruto: R${Calcular_Lucro_Bruto():F2} | Lucro Líquido: R${Calcular_Lucro_Liquido():F2}\n";
        }

        public double Calcular_Lucro_Bruto()
        {
            if (Produto == null) return 0;
            return Quantidade * Produto.Valor_Venda;
        }

        public void Calcular_Embalagens()
        {
            if (Produto == null || Embalagem == null)
            {
                Quantidade_Embalagem = 0;
                return;
            }

            // Evita divisão por zero se alguma dimensão for 0
            if (Produto.largura <= 0 || Produto.comprimento <= 0 || Produto.altura <= 0)
            {
                Quantidade_Embalagem = 0;
                return;
            }

            double[] dimensoesProduto =
            {
                Produto.largura,
                Produto.comprimento,
                Produto.altura
            };

            double[] dimensoesEmbalagem =
            {
                Embalagem.largura,
                Embalagem.comprimento,
                Embalagem.altura
            };

            // Ordena as dimensões para permitir a rotação
            Array.Sort(dimensoesProduto);
            Array.Sort(dimensoesEmbalagem);

            int produtosPorEmbalagem =
                (int)(dimensoesEmbalagem[0] / dimensoesProduto[0]) *
                (int)(dimensoesEmbalagem[1] / dimensoesProduto[1]) *
                (int)(dimensoesEmbalagem[2] / dimensoesProduto[2]);

            if (produtosPorEmbalagem <= 0)
            {
                Quantidade_Embalagem = 0;
                return;
            }

            Quantidade_Embalagem = (int)Math.Ceiling((double)Quantidade / produtosPorEmbalagem);
        }

        public double Calcular_Gasto_Total()
        {
            double custoProduto = Produto != null ? Produto.Calcular_Custo_Total() * Quantidade : 0;
            double custoEmbalagem = (Embalagem != null && Quantidade_Embalagem.HasValue) 
                ? Embalagem.Preco * Quantidade_Embalagem.Value 
                : 0;

            return custoProduto + custoEmbalagem;
        }

        public double Calcular_Lucro_Liquido()
        {
            return Calcular_Lucro_Bruto() - Calcular_Gasto_Total();
        }
    }
}