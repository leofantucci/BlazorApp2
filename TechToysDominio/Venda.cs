using System;
using System.Collections.Generic;
using System.Linq;

namespace TechToysDominio
{
    public class Venda
    {
        public int Id { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public DateTime Data_Hora { get; set; }
        public List<VendaItem> ItensVenda { get; set; } = new();
        public List<VendaEmbalagem> EmbalagensVenda { get; set; } = new();

        public double Calcular_Lucro_Bruto()
        {
            return ItensVenda?.Sum(i => i.Calcular_Subtotal()) ?? 0;
        }

        public double Calcular_Custo_Producao()
        {
            return ItensVenda?.Sum(i => i.Calcular_Custo_Producao()) ?? 0;
        }

        public double Calcular_Custo_Embalagens()
        {
            return EmbalagensVenda?.Sum(e => e.Calcular_Custo()) ?? 0;
        }

        public double Calcular_Gasto_Total()
        {
            return Calcular_Custo_Producao() + Calcular_Custo_Embalagens();
        }

        public double Calcular_Lucro_Liquido()
        {
            return Calcular_Lucro_Bruto() - Calcular_Gasto_Total();
        }
    }
}
