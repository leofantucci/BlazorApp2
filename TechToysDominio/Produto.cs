using System.Collections.Generic;

namespace TechToysDominio
{
    public class Produto : Item
    {
        // Composição: quantas unidades de cada peça formam uma unidade do produto.
        public List<ProdutoPeca> Composicao { get; set; } = new();

        // Mantidos para compatibilidade com telas e dados antigos. Não são usados no cálculo de estoque.
        public MateriaPrima? Materia_Prima { get; set; }
        public Impressora? Impressora { get; set; }
        public double Peso_Impressao { get; set; }
        public double Tempo_Impressao { get; set; }
        public double largura { get; set; }
        public double comprimento { get; set; }
        public double altura { get; set; }
        public double Valor_Venda { get; set; }
        public bool Ativo { get; set; }

        public Produto()
        {
            Unidade_Medida = "UN";
            Ativo = true;
        }

        public double Calcular_Custo_Materia_Prima()
        {
            if (Composicao.Count > 0) return Composicao.Sum(x => x.Peca.Calcular_Custo_Materia_Prima() * x.Quantidade);
            return Materia_Prima == null ? 0 : Peso_Impressao * Materia_Prima.Calcular_Preco_Grama();
        }

        public double Calcular_Custo_Impressao()
        {
            if (Composicao.Count > 0) return Composicao.Sum(x => x.Peca.Calcular_Custo_Impressao() * x.Quantidade);
            return Impressora == null ? 0 : Impressora.Calcular_Custo_Impressao(Tempo_Impressao);
        }

        public double Calcular_Custo_Total() => Calcular_Custo_Materia_Prima() + Calcular_Custo_Impressao();
        public double Lucro_Liquido() => Valor_Venda - Calcular_Custo_Total();
    }
}
