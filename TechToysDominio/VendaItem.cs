using System;

namespace TechToysDominio
{
    public class VendaItem
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; } = null!;
        public int Quantidade { get; set; }

        public double Calcular_Subtotal()
        {
            return Produto == null ? 0 : Produto.Valor_Venda * Quantidade;
        }

        public double Calcular_Custo_Producao()
        {
            return Produto == null ? 0 : Produto.Calcular_Custo_Total() * Quantidade;
        }
    }
}
